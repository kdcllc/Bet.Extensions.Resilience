using Bet.Extensions.Testing.Logging;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Xunit.Abstractions;

namespace Bet.AspNetCore.Resilience.UnitTest.GenericHosting;

/// <summary>
/// This server is used to simulate the Web Server that serves the requests back to the user.
/// </summary>
public class WebServerBuilder
{
    private readonly ITestOutputHelper _output;

    public WebServerBuilder(ITestOutputHelper output)
    {
        _output = output ?? throw new ArgumentNullException(nameof(output));
    }

    public int CallCount { get; private set; }

    public TestServer CreateServer()
    {
        var testServer = new TestServer(
            new WebHostBuilder()
            .UseUrls("http://testserver:5000")
            .ConfigureLogging(logger =>
            {
                logger.AddDebug();
                logger.AddXunit(_output);
            })
            .ConfigureServices((context, services) =>
            {
                // services.AddCorrelationId();
                // services.AddHostedService<CorrelationDiagnosticsListener>();
            })
            .Configure(app =>
            {
                // app.UseCorrelationId();
                app.MapWhen(
                  context => context.Request.Path.Value.Contains("/service1"),
                  (IApplicationBuilder pp) =>
                  {
                      pp.Run(async (context) =>
                      {
                          await context.Response.WriteAsync($"Path: {context.Request.Path} - Path Base: {context.Request.PathBase}");
                      });
                  });

                app.Run(context =>
                        {
                            context.Response.Headers.Add("content-type", "text/html");
                            return context.Response.WriteAsync(@"
                            <a href=""/hello"">/hello</a> <a href=""/hello/world"">/hello/world</a>
                        ");
                        });
            }));

        return testServer;
    }
}
