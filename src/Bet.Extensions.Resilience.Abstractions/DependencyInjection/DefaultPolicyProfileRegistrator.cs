using System;

using Bet.Extensions.Resilience.Abstractions.Internal;

namespace Bet.Extensions.Resilience.Abstractions.DependencyInjection
{
    public class DefaultPolicyProfileRegistrator
    {
        private readonly IServiceProvider _provider;

        public DefaultPolicyProfileRegistrator(IServiceProvider provider)
        {
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        }

        public void Register()
        {
            var policyRegistrant = (PolicyRegistrant)_provider.GetService(typeof(PolicyRegistrant));

            foreach (var reg in policyRegistrant.RegisteredPolicies)
            {
                var policy = (IPolicyProfileDescriptor)_provider.GetService(reg.Value);

                var policyOptionsRegistrant = (PolicyOptionsRegistrant)_provider.GetService(typeof(PolicyOptionsRegistrant));

                foreach (var item in policyOptionsRegistrant.RegisteredPolicyOptions)
                {
                    policy.GetPolicy(item.Key);
                }
            }
        }
    }
}
