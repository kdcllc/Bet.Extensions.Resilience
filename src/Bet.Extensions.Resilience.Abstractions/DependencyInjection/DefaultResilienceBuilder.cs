using System.Collections.Generic;

using Bet.Extensions.Resilience.Abstractions.Options;

using Microsoft.Extensions.DependencyInjection;

using Polly;

namespace Bet.Extensions.Resilience.Abstractions.DependencyInjection
{
    public class DefaultResilienceBuilder<TPolicy, TOptions> : IResilienceBuilder<TPolicy, TOptions> where TPolicy : IsPolicy where TOptions : PolicyOptions
    {
        public DefaultResilienceBuilder(IServiceCollection services, string policyName)
        {
            Services = services ?? throw new System.ArgumentNullException(nameof(services));
            PolicyName = policyName ?? throw new System.ArgumentNullException(nameof(policyName));

            PolicyNames.Add(PolicyName);
        }

        public IServiceCollection Services { get; }

        public string PolicyName { get; }

        public List<string> PolicyNames { get; set; } = new List<string>();
    }
}
