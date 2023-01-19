using Bet.Extensions.Http.MessageHandlers.HttpTimeout;
using Bet.Extensions.Testing.Logging;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Xunit;
using Xunit.Abstractions;

namespace Bet.AspNetCore.Resilience.UnitTest.MessageHandlers;

public class HttpTimeoutHandlerTests
{
    private readonly TestServer _server;

    public HttpTimeoutHandlerTests(ITestOutputHelper output)
    {
        Output = output;

        var webHost = new WebHostBuilder()
            .UseUrls("https://testserver:1000")
            .UseStartup<MessageHandlersStartup>()
            .ConfigureLogging((context, builder) =>
            {
                builder.AddDebug();
                builder.AddConsole();
            })
            .Configure(app =>
            {
                app.Map("/delayed", (appBuilder) =>
                {
                    appBuilder.Run(async context =>
                    {

                        await Task.Delay(TimeSpan.FromSeconds(6));

                        context.Response.StatusCode = StatusCodes.Status200OK;
                        await context.Response.WriteAsync(string.Empty);
                    });
                });

                app.Map("/canceled", (appBuilder) =>
                {
                    appBuilder.Run(async context =>
                    {

                        await Task.Delay(TimeSpan.FromSeconds(6));

                        context.Response.StatusCode = StatusCodes.Status200OK;
                        await context.Response.WriteAsync(string.Empty);
                    });
                });
            });

        _server = new TestServer(webHost);
    }

    public ITestOutputHelper Output { get; }

    [Fact]
    public async Task Timeout_ThrowExeption_When_Called()
    {
        var services = new ServiceCollection();

        services.AddLogging(builder =>
        {
            builder.AddXunit(Output);
        });

        services.AddHttpClient().AddHttpTimeoutHandler(opt =>
        {
            opt.DefaultTimeout = TimeSpan.FromSeconds(2);
            opt.InnerHandler = _server.CreateHandler();
        });

        var sp = services.BuildServiceProvider();

        var timeoutHandler = sp.GetRequiredService<HttpTimeoutHandler>();

        using var cts = new CancellationTokenSource();
        using var client = new HttpClient(timeoutHandler);
        client.Timeout = Timeout.InfiniteTimeSpan;

        async Task<HttpResponseMessage> TimeoutAsync()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "https://testserver:1000/delayed");
            // request.SetTimeout(TimeSpan.FromSeconds(2));
            var response = await client.SendAsync(request, cts.Token);
            return response;
        }

        await Assert.ThrowsAsync<TimeoutException>(TimeoutAsync);
    }

    [Fact]
    public async Task OperationCanceledException_ThrowExeption_When_Called()
    {
        var services = new ServiceCollection();

        services.AddLogging(builder =>
        {
            builder.AddXunit(Output);
        });

        services.AddHttpTimeoutHandler(opt =>
        {
            opt.DefaultTimeout = TimeSpan.FromSeconds(4);
            opt.InnerHandler = _server.CreateHandler();
        });

        var sp = services.BuildServiceProvider();

        var timeoutHandler = sp.GetRequiredService<HttpTimeoutHandler>();
        using var cts = new CancellationTokenSource();
        using var client = new HttpClient(timeoutHandler);

        cts.CancelAfter(TimeSpan.FromSeconds(1));
        async Task<HttpResponseMessage> CanceledAsync()
        {
            var response = await client.GetAsync("https://testserver:1000/canceled", cts.Token);

            return response;
        }

        await Assert.ThrowsAsync<TaskCanceledException>(CanceledAsync);
    }
}
