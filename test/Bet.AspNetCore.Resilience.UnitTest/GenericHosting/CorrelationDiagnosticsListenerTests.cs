using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Bet.Extensions.Http.MessageHandlers;
using Bet.Extensions.Http.MessageHandlers.CorrelationId;
using Bet.Extensions.Resilience.Hosting.CorrelationId;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;
using Moq.Protected;
using Xunit;
using Xunit.Abstractions;

namespace Bet.AspNetCore.Resilience.UnitTest.GenericHosting
{
    public class CorrelationDiagnosticsListenerTests
    {
        public CorrelationDiagnosticsListenerTests(ITestOutputHelper output)
        {
            Output = output;
        }

        public ITestOutputHelper Output { get; }

        [Fact]
        public async Task Should_Return_CorrelationId_By_Default()
        {
            using var testServer = new WebServerBuilder(Output).CreateServer();

            // arrange
            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            handlerMock
               .Protected()

               // Setup the PROTECTED method to mock
               .Setup<Task<HttpResponseMessage>>(
                  "SendAsync",
                  ItExpr.IsAny<HttpRequestMessage>(),
                  ItExpr.IsAny<CancellationToken>()
               )

               // prepare the expected response of the mocked http call
               .ReturnsAsync(() =>
                {
                    return new HttpResponseMessage()
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = new StringContent("[{'id':1,'value':'1'}]"),
                    };
                })
               .Verifiable();

            var host = new HostBuilder()
                .ConfigureServices(services =>
                {
                    services.AddCorrelationId();

                    // unfortunately diagnostic activity only
                    services.AddHostedService<CorrelationDiagnosticsListener>();

                    services.Configure<CorrelationIdOptions>(x => { });

                    var primaryHandler = testServer.CreateHandler();

                    // TODO: figure out why HttpMessageHandler is not working with diagnostics with mocks
                    services.AddHttpClient("Client1")
                            .ConfigureHttpClient(configClient => configClient.BaseAddress = new Uri("http://testserver:5000"))
                            .ConfigurePrimaryHttpMessageHandler(() => handlerMock.Object);

                    services.AddHttpClient("Client2")
                            .ConfigureHttpClient(configClient => configClient.BaseAddress = new Uri("https://google.com"))
                            .ConfigurePrimaryHttpMessageHandler(() => new DefaultHttpClientHandler());
                })
                .Build();

            await host.StartAsync();

            var hostServices = host.Services;

            //var client1 = hostServices.GetRequiredService<IHttpClientFactory>().CreateClient("Client1");

            //var request = new HttpRequestMessage(HttpMethod.Get, "/service");

            //var response = await client1.SendAsync(request, CancellationToken.None);

            var client2 = hostServices.GetRequiredService<IHttpClientFactory>().CreateClient("Client2");
            var response = await client2.GetAsync(string.Empty);

            var expectedHeaderName = new CorrelationIdOptions().Header;

            var header = response.Headers.GetValues(expectedHeaderName);

            Assert.NotNull(header);

            await host.StopAsync();
        }
    }
}
