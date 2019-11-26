using System.Linq;

using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Extensions.Hosting
{
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
            builder.ConfigureServices((_, services) =>
            {
                var found = services.SingleOrDefault(x => x.ServiceType == typeof(IHostedService))?.ImplementationType == typeof(HostStartupService);
                if (!found)
                {
                    services.AddHostedService<HostStartupService>();
                }

                services.AddSingleton(typeof(IHostStartupFilter), typeof(T));
            });
            return builder;
        }

        public static IHostBuilder UseResilienceOnStartup(this IHostBuilder builder)
        {
            return builder.UseStartupFilter<PolicyConfiguratorStartupFilter>();
        }
    }
}
