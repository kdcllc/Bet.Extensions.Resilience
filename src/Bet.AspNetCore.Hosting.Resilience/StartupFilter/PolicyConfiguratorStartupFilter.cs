using System;

using Bet.Extensions.Resilience.Abstractions;

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
                var registration = _provider.GetService<IPolicyRegistrator>();
                registration?.ConfigurePolicies();

                next(app);
            };
        }
    }
}
