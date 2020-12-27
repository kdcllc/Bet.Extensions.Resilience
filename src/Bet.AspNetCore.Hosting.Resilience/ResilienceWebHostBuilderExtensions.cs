using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.Hosting
{
    public static class ResilienceWebHostBuilderExtensions
    {
        public static IWebHostBuilder UseResilienceOnStartup(this IWebHostBuilder builder)
        {
            builder.ConfigureServices(services => services.AddTransient<IStartupFilter, PolicyConfiguratorStartupFilter>());
            return builder;
        }
    }
}
