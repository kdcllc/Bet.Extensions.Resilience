using System.Net;
using System.Text;

using Bet.Extensions.Http.MessageHandlers.Authorize;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Newtonsoft.Json;

using Polly;
using Polly.CircuitBreaker;

using Xunit;

namespace Bet.AspNetCore.Resilience.UnitTest.MessageHandlers
{
    public class AuthorizeHandlerTest : IDisposable
    {
        private const string BaseUrl = "http://testserver:7290/";
        private readonly TestServer _server;
        private readonly HttpClient _client;
        private int _authSuccessCallCnt;

        public AuthorizeHandlerTest()
        {
            var webHost = new WebHostBuilder()
                    .UseUrls(BaseUrl)
                    .UseStartup<TestStartup>()
                    .ConfigureServices(services => { })
                    .ConfigureLogging((_, builder) =>
                    {
                        builder.AddDebug();
                        builder.AddConsole();
                    })
                    .Configure(app =>
                    {
                        app.Map("/auth/token", (appBuilder) =>
                        {
                            appBuilder.Run(async context =>
                            {
                                context.Response.StatusCode = StatusCodes.Status200OK;

                                var expiresInSeconds = context.Request.Query.ContainsKey("expires-in")
                                    ? Convert.ToInt32(context.Request.Query["expires-in"][0])
                                    : 0;

                                var expiresIn = (int)DateTimeOffset.UtcNow.AddSeconds(expiresInSeconds).ToUnixTimeSeconds();

                                var responseToken = new AuthorizeTokenResponse("accessToken", "refreshToken", "Bearer", expiresIn);

                                _authSuccessCallCnt++;

                                await context.Response.WriteAsync(JsonConvert.SerializeObject(responseToken));
                            });
                        });

                        app.Map("/unauth/token", (appBuilder) =>
                        {
                            appBuilder.Run(async context =>
                            {
                                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                                await context.Response.WriteAsync(string.Empty);
                            });
                        });

                        app.Map("/test", (appBuilder) =>
                        {
                            appBuilder.Run(async context =>
                            {
                                context.Response.StatusCode = StatusCodes.Status200OK;
                                await context.Response.WriteAsync(string.Empty);
                            });
                        });
                    });

            _server = new TestServer(webHost);

            _client = _server.CreateClient();
        }

        [Fact]
        public async Task Should_Throw_AuthorizingMessageHandlerException_When_Unauthorized_Async()
        {
            var logger = _server.Host.Services.GetRequiredService<ILogger<AuthorizeHandler<AuthHttpClientOptions, AuthorizeTokenResponse>>>();

            var httpClientOptions = new AuthHttpClientOptions
            {
                BaseAddress = new Uri(BaseUrl),
                Password = "MyPassword",
                Username = "MyUserName"
            };

            var handlerConfiguration = new AuthorizeHandlerConfiguration<AuthHttpClientOptions, AuthorizeTokenResponse>(
                (authClientOptions) =>
                {
                    var body =
                        $"grant_type=password&username={authClientOptions.Username}&password={authClientOptions.Password}";

                    return new HttpRequestMessage(
                        HttpMethod.Post,
                        new Uri(authClientOptions.BaseAddress, "unauth/token"))
                    {
                        Content = new StringContent(body, Encoding.UTF8, "application/x-www-form-urlencoded")
                    };
                },
                response => (response.AccessToken, DateTimeOffset.FromUnixTimeSeconds(response.ExpiresInSeconds)));

            using var authHandler = new AuthorizeHandler<AuthHttpClientOptions, AuthorizeTokenResponse>(
                                    httpClientOptions,
                                    handlerConfiguration,
                                    AuthType.Bearer,
                                    logger)
            { InnerHandler = _server.CreateHandler() };

            using var client = new HttpClient(authHandler);
            var uri = new Uri($"{BaseUrl}test");
            var ex = await Assert.ThrowsAsync<AuthorizeHandlerException>(async () => await client.GetAsync(uri));
            Assert.Equal(HttpStatusCode.Unauthorized, ex.StatusCode);
        }

        [Fact]
        public async Task Should_Retry_When_UnAuthorized()
        {
            var logger = _server.Host.Services.GetRequiredService<ILogger<AuthorizeHandler<AuthHttpClientOptions, AuthorizeTokenResponse>>>();

            var httpClientOptions = new AuthHttpClientOptions
            {
                BaseAddress = new Uri(BaseUrl),
                Password = "MyPassword",
                Username = "MyUserName"
            };

            var handlerConfiguration = new AuthorizeHandlerConfiguration<AuthHttpClientOptions, AuthorizeTokenResponse>(
                (authClientOptions) =>
                {
                    var body =
                        $"grant_type=password&username={authClientOptions.Username}&password={authClientOptions.Password}";

                    return new HttpRequestMessage(
                            HttpMethod.Post,
                            new Uri(authClientOptions.BaseAddress, "unauth/token"))
                    {
                        Content = new StringContent(body, Encoding.UTF8, "application/x-www-form-urlencoded")
                    };
                },
                response =>
                {
                    var accessToken = response.AccessToken;
                    var offsetTime = DateTimeOffset.FromUnixTimeSeconds(response.ExpiresInSeconds);
                    var nextTime = offsetTime.Subtract(DateTimeOffset.Now).Subtract(TimeSpan.FromMinutes(5));
                    var expiresIn = DateTimeOffset.FromUnixTimeSeconds(response.ExpiresInSeconds);

                    return (accessToken, expiresIn);
                });

            using var authHandler = new AuthorizeHandler<AuthHttpClientOptions, AuthorizeTokenResponse>(
                                    httpClientOptions,
                                    handlerConfiguration,
                                    AuthType.Bearer,
                                    logger)
            { InnerHandler = _server.CreateHandler() };
            using var client = new HttpClient(authHandler);
            var uri = new Uri($"{BaseUrl}test");

            var retryPolicy = Policy.Handle<Exception>(e => !(e is BrokenCircuitException))
                .WaitAndRetryForeverAsync(_ => TimeSpan.FromMilliseconds(200));

            var circuitBreakerAsync = Policy
                    .Handle<AuthorizeHandlerException>(e => e.StatusCode == HttpStatusCode.Unauthorized)
                    .CircuitBreakerAsync(2, TimeSpan.FromSeconds(2));

            var policyWrap = Policy.WrapAsync(retryPolicy, circuitBreakerAsync);

            var ex = await Assert.ThrowsAsync<BrokenCircuitException>(async () =>
            {
                await policyWrap.ExecuteAsync(async (token) => await client.GetAsync(uri, token), CancellationToken.None);
            });

            Assert.True(ex is BrokenCircuitException);
        }

        [Fact]
        public async Task Should_ReAuthenticate_When_Token_Expires()
        {
            var logger = _server.Host.Services.GetRequiredService<ILogger<AuthorizeHandler<AuthHttpClientOptions, AuthorizeTokenResponse>>>();

            var httpClientOptions = new AuthHttpClientOptions
            {
                BaseAddress = new Uri(BaseUrl),
                Password = "MyPassword",
                Username = "MyUserName"
            };

            var handlerConfigurationOptions = new AuthorizeHandlerConfiguration<AuthHttpClientOptions, AuthorizeTokenResponse>(
                  (authClientOptions) =>
                  {
                      var body = $"grant_type=password&username={authClientOptions.Username}&password={authClientOptions.Password}";
                      return new HttpRequestMessage(HttpMethod.Post, new Uri(authClientOptions.BaseAddress, "auth/token"))
                      {
                          Content = new StringContent(body, Encoding.UTF8, "application/x-www-form-urlencoded")
                      };
                  },
                  response => (response.AccessToken, DateTimeOffset.FromUnixTimeSeconds(response.ExpiresInSeconds)));

            using var authHandler = new AuthorizeHandler<AuthHttpClientOptions, AuthorizeTokenResponse>(
                httpClientOptions,
                handlerConfigurationOptions,
                AuthType.Bearer,
                logger);
            authHandler.InnerHandler = _server.CreateHandler();
            using var client = new HttpClient(authHandler);
            var uri = new Uri($"{BaseUrl}test");
            await client.GetAsync(uri);
            await client.GetAsync(uri);

            // it should of had to call the auth/token endpoint a 2nd time for the 2nd call since the token would of expired
            Assert.Equal(2, _authSuccessCallCnt);
        }

        [Fact]
        public async Task Should_ReAuthenticate_When_Token_Invalid()
        {
            var logger = _server.Host.Services.GetRequiredService<ILogger<AuthorizeHandler<AuthHttpClientOptions, AuthorizeTokenResponse>>>();

            var authConfiguration = new AuthHttpClientOptions
            {
                BaseAddress = new Uri(BaseUrl),
                Password = "MyPassword",
                Username = "MyUserName"
            };

            var handlerConfigurationOptions = new AuthorizeHandlerConfiguration<AuthHttpClientOptions, AuthorizeTokenResponse>(
                  (authClientOptions) =>
                  {
                      var body = $"grant_type=password&username={authClientOptions.Username}&password={authClientOptions.Password}";
                      return new HttpRequestMessage(HttpMethod.Post, new Uri(authClientOptions.BaseAddress, "auth/token"))
                      {
                          Content = new StringContent(body, Encoding.UTF8, "application/x-www-form-urlencoded")
                      };
                  },
                  response => (response.AccessToken, DateTimeOffset.FromUnixTimeSeconds(response.ExpiresInSeconds)));

            using var authHandler = new AuthorizeHandler<AuthHttpClientOptions, AuthorizeTokenResponse>(
                authConfiguration,
                handlerConfigurationOptions,
                AuthType.Bearer,
                logger)
            { InnerHandler = _server.CreateHandler() };
            using var client = new HttpClient(authHandler);
            // go to the endpoint that returns unauthorized to force the authHandler to try to authenticate once more
            var uri = new Uri($"{BaseUrl}unauth/token");
            await client.GetAsync(uri);

            // it should of had to call the auth/token endpoint a 2nd time since the endpoint is returning unauthorized
            Assert.Equal(2, _authSuccessCallCnt);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _client?.Dispose();
                _server?.Dispose();
            }
        }
    }
}
