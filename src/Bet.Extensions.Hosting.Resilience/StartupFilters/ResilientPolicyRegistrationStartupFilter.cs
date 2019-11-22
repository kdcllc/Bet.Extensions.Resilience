using System;
using Bet.Extensions.Resilience.Abstractions;

using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Extensions.Hosting
{
    public class ResilientPolicyRegistrationStartupFilter : IHostStartupFilter
    {
        public void Configure(IServiceProvider provider)
        {
            var registration = provider.GetService<IPolicyRegistrator>();
            if (registration != null)
            {
                registration.ConfigurePolicies();
            }
        }
    }
}
