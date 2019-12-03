using System;

using Microsoft.Extensions.Logging;

using Polly;
using Polly.Registry;

namespace Bet.Extensions.Resilience.Abstractions.Internal
{
    /// <summary>
    /// The Default implementation of <see cref="IPolicyRegistryConfigurator"/>.
    /// </summary>
    internal class DefaultPolicyRegistryConfigurator : IPolicyRegistryConfigurator
    {
        private readonly IPolicyRegistry<string> _policyRegistry;
        private readonly ILogger<DefaultPolicyRegistryConfigurator> _logger;

        public DefaultPolicyRegistryConfigurator(
            IPolicyRegistry<string> policyRegistry,
            ILogger<DefaultPolicyRegistryConfigurator> logger)
        {
            _policyRegistry = policyRegistry ?? throw new ArgumentNullException(nameof(policyRegistry));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        public IPolicyRegistryConfigurator AddPolicy(string policyName, Func<IsPolicy> policyFunc, bool replaceIfExists = false)
        {
            if (!_policyRegistry.ContainsKey(policyName))
            {
                _policyRegistry.Add(policyName, policyFunc());
                _logger.LogDebug("{policyName} was successfully added to Polly registry", policyName);
            }
            else if (replaceIfExists)
            {
                _policyRegistry[policyName] = policyFunc();
                _logger.LogDebug("{policyName} was successfully updated with Polly registry", policyName);
            }

            return this;
        }

        /// <inheritdoc/>
        public bool IsPolicyConfigured(string policyName)
        {
            return _policyRegistry.ContainsKey(policyName);
        }
    }
}
