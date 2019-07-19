using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using Bet.Extensions.MessageHandlers.Timeout;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Xunit;

namespace Bet.Extensions.Resilience.UnitTest.MessageHandlers
{
    public class TimeoutHandlerTests
    {
        private readonly TestServer _server;

        public TimeoutHandlerTests()
        {
            var webHost = new WebHostBuilder()
                .UseUrls("https://testserver:1000")
                .UseStartup<TestStartup>()
                .ConfigureLogging((context, builder) =>
                {
                    builder.AddDebug();
                    builder.AddConsole();
                })
                .Configure(app=>
                {
                    app.Map("/delayed", (appBuilder) =>
                    {
                        appBuilder.Run(async context =>
                        {
                            context.RequestAborted.ThrowIfCancellationRequested();

                            await Task.Delay(TimeSpan.FromSeconds(4));

                            context.RequestAborted.ThrowIfCancellationRequested();

                            context.Response.StatusCode = StatusCodes.Status200OK;
                            await context.Response.WriteAsync(string.Empty);
                        });
                    });

                    app.Map("/canceled", (appBuilder) =>
                    {
                        appBuilder.Run(context =>
                        {
                            throw new OperationCanceledException();
                        });
                    });
                });

            _server = new TestServer(webHost);
        }

        [Fact]
        public async Task MakeAsyncCalls_ThrowExeptions()
        {
            var serviceCollection = new ServiceCollection();

            serviceCollection.AddTimeoutHandler(opt =>
            {
                opt.DefaultTimeout = TimeSpan.FromSeconds(3);
                opt.InnerHandler = _server.CreateHandler();
            });

            var services = serviceCollection.BuildServiceProvider();

            var timeoutHandler = services.GetRequiredService<TimeoutHandler>();

            using (var cts = new CancellationTokenSource())
            using (var client = new HttpClient(timeoutHandler))
            {
                async Task<HttpResponseMessage>  delayedAsync() => await client.GetAsync("https://testserver:1000/delayed", cts.Token) ;

                await Assert.ThrowsAsync<TimeoutException>(delayedAsync);

                cts.CancelAfter(TimeSpan.FromSeconds(1));
                async Task<HttpResponseMessage> canceledAsync() => await client.GetAsync("https://testserver:1000/delayed", cts.Token);

                await Assert.ThrowsAsync<OperationCanceledException>(canceledAsync);
            }
        }
    }
}
