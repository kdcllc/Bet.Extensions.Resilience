using System;

using Bet.Extensions.Resilience.Abstractions.DependencyInjection;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.Hosting
{
    public class PolicyConfiguratorStartupFilter : IStartupFilter
    {
        private readonly IServiceProvider _provider;

        public PolicyConfiguratorStartupFilter(IServiceProvider provider)
        {
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        }

        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
        {
            return app =>
            {
                var policyRegistrant = _provider.GetRequiredService<DefaultPolicyProfileRegistrator>();
                policyRegistrant.Register();

                next(app);
            };
        }
    }
}
