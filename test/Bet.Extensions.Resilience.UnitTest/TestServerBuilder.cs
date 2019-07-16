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
        private readonly ITestOutputHelper _output;

        public TestServerBuilder(ITestOutputHelper output)
        {
            _output = output;
        }

        public int CallCount { get; private set; }

        public TestServer GetSimpleServer()
        {
            var testServer = new TestServer(
                new WebHostBuilder()
                .UseUrls("http://testserver:5000")
                .ConfigureLogging(logger =>
                {
                    logger.AddDebug();
                    logger.AddProvider(new XunitLoggerProvider(_output));
                })
                .Configure(app =>
                {
                    app.Run(async context =>
                      {
                          if (CallCount++ % 2 == 0)
                          {
                              context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                          }
                          else
                          {
                              context.Response.StatusCode = StatusCodes.Status200OK;
                          }

                          await context.Response.WriteAsync(string.Empty);
                      });
                }));

            return testServer;
        }
    }
}
