using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.Hosting;

public static class ResilienceWebHostBuilderExtensions
{
    /// <summary>
    /// Adds Resilient Engine to the application.
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static IWebHostBuilder UseResilienceOnStartup(this IWebHostBuilder builder)
    {
        builder.ConfigureServices(services => services.AddTransient<IStartupFilter, PolicyConfiguratorStartupFilter>());
        return builder;
    }
}
