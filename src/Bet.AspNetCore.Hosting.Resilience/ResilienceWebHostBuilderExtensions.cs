using Bet.AspNetCore.Hosting.Resilience.StartupFilter;

using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.Hosting
{
    public static class ResilienceWebHostBuilderExtensions
    {
        public static IWebHostBuilder UseResiliencePolicies(this IWebHostBuilder builder)
        {
            builder.ConfigureServices(services => services.AddTransient<IStartupFilter, ResilientPolicyRegistrationStartupFilter>());
            return builder;
        }
    }
}
