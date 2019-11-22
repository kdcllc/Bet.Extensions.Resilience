using System;

using Bet.Extensions.Resilience.Abstractions;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;

using Microsoft.Extensions.DependencyInjection;

namespace Bet.AspNetCore.Hosting.Resilience.StartupFilter
{
    public class ResilientPolicyRegistrationStartupFilter : IStartupFilter
    {
        private readonly IServiceProvider _provider;

        public ResilientPolicyRegistrationStartupFilter(IServiceProvider provider)
        {
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        }

        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
        {
            return app =>
            {
                var registration = _provider.GetService<IPolicyRegistrator>();
                if (registration != null)
                {
                    registration.ConfigurePolicies();
                }

                next(app);
            };
        }
    }
}
