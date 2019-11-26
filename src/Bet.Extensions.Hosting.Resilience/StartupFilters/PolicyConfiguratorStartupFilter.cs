using System;

using Bet.Extensions.Resilience.Abstractions;

using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Extensions.Hosting
{
    /// <summary>
    /// The <see cref="IHost"/> startup filter infrastructure for registering Resilience policies.
    /// </summary>
    public class PolicyConfiguratorStartupFilter : IHostStartupFilter
    {
        public void Configure(IServiceProvider provider)
        {
            var registration = provider.GetService<IPolicyRegistrator>();
            registration?.ConfigurePolicies();
        }
    }
}
