using System.Text;

using Bet.Extensions.Http.MessageHandlers.CookieAuthentication;
using Bet.Extensions.Resilience.Http.Abstractions.Options;
using Bet.Extensions.Testing.Logging;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Newtonsoft.Json;

using Xunit;
using Xunit.Abstractions;

namespace Bet.AspNetCore.Resilience.UnitTest.MessageHandlers;

public class CookieAuthenticationHandlerTests
{
    private readonly TestServer _server;

    public CookieAuthenticationHandlerTests(ITestOutputHelper output)
    {
        Output = output;

        var webHost = new WebHostBuilder()
           .UseUrls("https://testserver:2000")
           .UseStartup<MessageHandlersStartup>()
           .ConfigureLogging((context, builder) =>
           {
               builder.AddDebug();
               builder.AddConsole();
           })
           .Configure(app =>
           {
               app.MapWhen(
                   context => context.Request.Path == "/auth" && context.Request.Method == "POST",
                   (appBuilder) =>
                   {
                       appBuilder.Run(async context =>
                       {
                           context.Response.StatusCode = StatusCodes.Status200OK;
                           context.Response.Headers.Add("Set-Cookie", "authenticated");

                           await context.Response.WriteAsync(string.Empty);
                       });
                   });

               app.Map("/test", (appBuilder) =>
                   {
                       appBuilder.Run(async context =>
                       {
                           if (context.Request.Headers.ContainsKey("Cookie"))
                           {
                               context.Response.StatusCode = StatusCodes.Status200OK;
                           }
                           else
                           {
                               context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                           }

                           await context.Response.WriteAsync(string.Empty);
                       });
                   });
           });

        _server = new TestServer(webHost);
    }

    public ITestOutputHelper Output { get; }

    [Fact]
    public async Task Successfully_Authenticate_With_Cookie()
    {
        // arrange
        var serviceCollection = new ServiceCollection();

        var dic = new Dictionary<string, string>
        {
            { "O:BaseAddress", "https://testserver:2000/" },
            { "O:Timeout", "00:01:40" },
            { "O:ContentType", "application/json" },
            { "O:Username", "username" },
            { "O:Password", "password" }
        };

        var config = new ConfigurationBuilder().AddInMemoryCollection(dic).Build();
        serviceCollection.AddSingleton<IConfiguration>(config);

        serviceCollection.AddLogging(builder => builder.AddXunit(Output));

        serviceCollection.TryAddTransient<IConfigureOptions<HttpClientOptions>>(sp =>
        {
            return new ConfigureOptions<HttpClientOptions>((options) =>
            {
                var configuration = sp.GetRequiredService<IConfiguration>();
                configuration.Bind("O", options);
            });
        });

        serviceCollection.TryAddTransient<IConfigureOptions<HttpClientBasicAuthOptions>>(sp =>
        {
            return new ConfigureOptions<HttpClientBasicAuthOptions>((options) =>
            {
                var configuration = sp.GetRequiredService<IConfiguration>();
                configuration.Bind("O", options);

                // httpAuthClientConfigure?.Invoke(options);
            });
        });

        serviceCollection.AddTransient((sp) =>
        {
            var logger = sp.GetRequiredService<ILogger<CookieAuthenticationHandler>>();

            var authOptions = sp.GetRequiredService<IOptions<HttpClientBasicAuthOptions>>().Value;

            var options = new CookieAuthenticationHandlerOptions { InnerHandler = _server.CreateHandler() };

            options.Options = new CookieGeneratorOptions(authOptions)
            {
                AuthenticationRequest = (opt) =>
                {
                    var request = new HttpRequestMessage(HttpMethod.Post, $"{opt.BaseAddress}auth");

                    var authRequest = new { email = opt.Username, password = opt.Password };
                    var json = JsonConvert.SerializeObject(authRequest);

                    request.Content = new StringContent(json, Encoding.UTF8, opt.ContentType);

                    return request;
                }
            };

            return new CookieAuthenticationHandler(options, logger);
        });

        var services = serviceCollection.BuildServiceProvider();

        var handler = services.GetRequiredService<CookieAuthenticationHandler>();

        using (var client = new HttpClient(handler))
        {
            var threads = new Thread[10];

            for (var i = 0; i < threads.Length; i++)
            {
                threads[i] = new Thread(new ThreadStart(async () =>
                {
                    await client.GetAsync("https://testserver:2000/test");
                    await Task.Delay(1000);
                }));
            }

            foreach (var thread in threads)
            {
                thread.Start();
            }

            foreach (var thread in threads)
            {
                thread.Join();
            }

            var response = await client.GetAsync("https://testserver:2000/test");

            Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);
        }
    }
}
