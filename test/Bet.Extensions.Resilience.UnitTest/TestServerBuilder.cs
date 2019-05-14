using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Logging;

using Xunit.Abstractions;

namespace Bet.Extensions.Resilience.UnitTest
{
    internal sealed class TestServerBuilder
    {
        private ITestOutputHelper _output;

        public TestServerBuilder(ITestOutputHelper output)
        {
            _output = output;
        }

        public TestServer GetSimpleServer()
        {
            var testServer = new TestServer(
                new WebHostBuilder()
                .ConfigureLogging(logger =>
                {
                    logger.AddDebug();
                    logger.AddProvider(new TestLoggerProvider(_output));
                })
                .Configure(app =>
                {
                    app.Map("/", (response) =>
                    {
                        response.Run(async context =>
                        {
                            context.Response.StatusCode = StatusCodes.Status200OK;
                            await context.Response.WriteAsync(string.Empty);
                        });
                    });
                }))
                { BaseAddress = new Uri("https://localhost") };

            return testServer;
        }
    }
}
