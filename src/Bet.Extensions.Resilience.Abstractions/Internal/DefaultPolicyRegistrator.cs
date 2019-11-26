using System;
using System.Collections.Generic;

using Bet.Extensions.Resilience.Abstractions.Policies;

using Microsoft.Extensions.DependencyInjection;

using Polly.Registry;

namespace Bet.Extensions.Resilience.Abstractions.Internal
{
    /// <summary>
    /// The default implementation of <see cref="IPolicyRegistrator"/>.
    /// </summary>
    internal class DefaultPolicyRegistrator : IPolicyRegistrator
    {
        private readonly PolicyRegistrant _policyRegistrant;
        private readonly IPolicyRegistry<string> _policyRegistry;
        private readonly IServiceProvider _provider;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultPolicyRegistrator"/> class.
        /// </summary>
        /// <param name="provider">The DI provider.</param>
        /// <param name="policyRegistrant">The policy registrant to be used for registrations.</param>
        /// <param name="policyRegistry">The policy registry.</param>
        public DefaultPolicyRegistrator(
            IServiceProvider provider,
            PolicyRegistrant policyRegistrant,
            IPolicyRegistry<string> policyRegistry)
        {
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
            _policyRegistrant = policyRegistrant ?? throw new ArgumentNullException(nameof(policyRegistrant));
            _policyRegistry = policyRegistry ?? throw new ArgumentNullException(nameof(policyRegistry));
        }

        /// <inheritdoc/>
        public void ConfigurePolicies()
        {
            foreach (var policy in _policyRegistrant.RegisteredPolicies)
            {
                if (!_policyRegistry.ContainsKey(policy.Key))
                {
                    IEnumerable<object> configurators;

                    var paramTypes = new List<Type> { policy.Value.optionsType };

                    if (policy.Value.resultType == null)
                    {
                        configurators = _provider.GetServices(typeof(IPolicy<>).MakeGenericType(paramTypes.ToArray()));
                    }
                    else
                    {
                        paramTypes.Add(policy.Value.resultType);
                        configurators = _provider.GetServices(typeof(IPolicy<,>).MakeGenericType(paramTypes.ToArray()));
                    }

                    foreach (var configurator in configurators)
                    {
                        if (configurator != null)
                        {
                            var method = configurator.GetType().GetMethod("ConfigurePolicy");
                            method?.Invoke(configurator, Array.Empty<object>());
                        }
                    }
                }
            }
        }
    }
}
