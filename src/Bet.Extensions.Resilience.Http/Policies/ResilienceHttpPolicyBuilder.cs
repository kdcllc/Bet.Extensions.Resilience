using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Http;

using Bet.Extensions.Resilience.Http.Options;

using Microsoft.Extensions.Options;

using Polly;
using Polly.Registry;

namespace Bet.Extensions.Resilience.Http.Policies
{
    /// <inheritdoc/>
    public class ResilienceHttpPolicyBuilder<TOptions> : IResilienceHttpPolicyBuilder<TOptions> where TOptions : HttpPolicyOptions
    {
        private readonly IDictionary<string, TOptions> _optionsCollection = new ConcurrentDictionary<string, TOptions>();
        private readonly IDictionary<string, Func<IAsyncPolicy<HttpResponseMessage>>> _asyncPolicies = new ConcurrentDictionary<string, Func<IAsyncPolicy<HttpResponseMessage>>>();
        private readonly IDictionary<string, Func<ISyncPolicy<HttpResponseMessage>>> _syncPolicies = new ConcurrentDictionary<string, Func<ISyncPolicy<HttpResponseMessage>>>();

        private readonly IOptionsMonitor<TOptions> _optionsMonitor;
        private readonly IPolicyRegistry<string> _policyRegistry;

        public ResilienceHttpPolicyBuilder(IOptionsMonitor<TOptions> optionsMonitor, IPolicyRegistry<string> policyRegistry)
        {
            _optionsMonitor = optionsMonitor;
            _policyRegistry = policyRegistry;

            _optionsMonitor.OnChange(newOptions =>
            {
                // ignore the default none named option
                if (string.IsNullOrWhiteSpace(newOptions.PolicyName))
                {
                    return;
                }

                // update options
                _optionsCollection[newOptions.PolicyName] = newOptions;

                RegisterPolicies();
            });
        }

        /// <inheritdoc/>
        public IResilienceHttpPolicyBuilder<TOptions> AddPolicy(string policyName, Func<IAsyncPolicy<HttpResponseMessage>> policyFunc, bool replaceIfExists = false)
        {
            if (!_policyRegistry.ContainsKey(policyName))
            {
                SetOptions(policyName);
                _policyRegistry.Add(policyName, policyFunc());
            }
            else if (replaceIfExists)
            {
                _policyRegistry[policyName] = policyFunc();
            }

            if (!_asyncPolicies.ContainsKey(policyName))
            {
                _asyncPolicies.Add(policyName, policyFunc);
            }
            else if (replaceIfExists)
            {
                _asyncPolicies[policyName] = policyFunc;
            }

            return this;
        }

        /// <inheritdoc/>
        public IResilienceHttpPolicyBuilder<TOptions> AddPolicy(
            string policyName,
            Func<ISyncPolicy<HttpResponseMessage>> policyFunc,
            bool replaceIfExists = false)
        {
            if (!_policyRegistry.ContainsKey(policyName))
            {
                _policyRegistry.Add(policyName, policyFunc());
            }
            else if (replaceIfExists)
            {
                _policyRegistry[policyName] = policyFunc();
            }

            if (!_syncPolicies.ContainsKey(policyName))
            {
                _syncPolicies.Add(policyName, policyFunc);
            }
            else if (replaceIfExists)
            {
                _syncPolicies[policyName] = policyFunc;
            }

            return this;
        }

        /// <inheritdoc/>
        public void RegisterPolicies()
        {
            // Recreate added policies. Those policies will utilize the updated setting value stored in _settings
            foreach (var asyncPolicy in _asyncPolicies)
            {
                _policyRegistry[asyncPolicy.Key] = asyncPolicy.Value();
            }

            foreach (var syncPolicy in _syncPolicies)
            {
                _policyRegistry[syncPolicy.Key] = syncPolicy.Value();
            }
        }

        /// <inheritdoc/>
        public TOptions GetOptions(string settingsName)
        {
            settingsName = settingsName ?? throw new ArgumentNullException(nameof(settingsName));
            return _optionsCollection[settingsName];
        }

        /// <summary>
        /// Stores a named setting retrieved from options system.
        /// </summary>
        /// <param name="name"></param>
        private void SetOptions(string name)
        {
            var settingsName = name ?? throw new ArgumentNullException(nameof(name));

            _optionsCollection[settingsName] = _optionsMonitor.Get(settingsName);
        }
    }
}
