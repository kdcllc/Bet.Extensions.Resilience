using Bet.Extensions.Hosting.Resilience.CorrelationId;
using Bet.Extensions.Http.MessageHandlers.CorrelationId;

using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Extensions.Hosting;

public static class ResilienceHostBuilderExtension
{
    /// <summary>
    /// Uses <see cref="IHostStartupFilter"/> registration based on the implementation type.
    /// </summary>
    /// <typeparam name="T">The type of the implementation.</typeparam>
    /// <param name="builder">The <see cref="IHostBuilder"/> instance.</param>
    /// <returns></returns>
    public static IHostBuilder UseStartupFilter<T>(this IHostBuilder builder)
    {
        return builder.ConfigureServices((_, services) =>
        {
            var found = services.SingleOrDefault(x => x.ServiceType == typeof(IHostedService))?.ImplementationType == typeof(HostStartupService);
            if (!found)
            {
                services.AddHostedService<HostStartupService>();
            }

            services.AddSingleton(typeof(IHostStartupFilter), typeof(T));
        });
    }

    public static IHostBuilder UseResilienceOnStartup(this IHostBuilder builder)
    {
        return builder.UseStartupFilter<PolicyConfiguratorStartupFilter>();
    }

    /// <summary>
    /// Adds support for CorrelationId per http client.
    /// https://devblogs.microsoft.com/aspnet/improvements-in-net-core-3-0-for-troubleshooting-and-monitoring-distributed-apps/.
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="configure"></param>
    /// <returns></returns>
    public static IHostBuilder UseCorrelationId(this IHostBuilder builder, Action<CorrelationIdOptions>? configure = null)
    {
        return builder.ConfigureServices((hostingContext, services) =>
        {
            services.AddCorrelationId();

            services.Configure<CorrelationIdOptions>(options => configure?.Invoke(options));

            // unfortunately diagnostic activity only no trace id available
            services.AddHostedService<CorrelationDiagnosticsListener>();
        });
    }
}
