using System;

using Bet.Extensions.Resilience.Abstractions.DependencyInjection;

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
            var policyRegistrant = provider.GetRequiredService<PolicyBucketConfigurator>();
            policyRegistrant.Register();
        }
    }
}
