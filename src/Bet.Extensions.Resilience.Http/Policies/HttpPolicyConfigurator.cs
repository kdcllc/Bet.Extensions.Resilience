using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

using Bet.Extensions.Resilience.Http.Options;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using Polly;
using Polly.Registry;

namespace Bet.Extensions.Resilience.Http.Policies
{
    /// <inheritdoc/>
    public class HttpPolicyConfigurator<TOptions> : IHttpPolicyConfigurator<TOptions> where TOptions : HttpPolicyOptions
    {
        private readonly IDictionary<string, TOptions> _optionsCollection = new ConcurrentDictionary<string, TOptions>();
        private readonly IDictionary<string, Func<IAsyncPolicy<HttpResponseMessage>>> _asyncPolicyCollection = new ConcurrentDictionary<string, Func<IAsyncPolicy<HttpResponseMessage>>>();
        private readonly IDictionary<string, Func<ISyncPolicy<HttpResponseMessage>>> _syncPolicyCollection = new ConcurrentDictionary<string, Func<ISyncPolicy<HttpResponseMessage>>>();

        private readonly IOptionsMonitor<TOptions> _optionsMonitor;
        private readonly IPolicyRegistry<string> _policyRegistry;
        private readonly IServiceProvider _provider;

        public HttpPolicyConfigurator(
            IServiceProvider provider,
            string parentPolicyName,
            string[] childrenPolicyNames = null)
        {
            _provider = provider;

            _optionsMonitor = provider.GetRequiredService<IOptionsMonitor<TOptions>>();
            _policyRegistry = provider.GetRequiredService<IPolicyRegistry<string>>();

            _optionsCollection.Add(parentPolicyName, _optionsMonitor.Get(parentPolicyName));

            if (childrenPolicyNames != null)
            {
                foreach (var child in childrenPolicyNames)
                {
                    _optionsCollection.Add(child, _optionsMonitor.Get(parentPolicyName));
                }
            }

            _optionsMonitor.OnChange(newOptions =>
            {
                // ignore the default none named option
                if (string.IsNullOrWhiteSpace(newOptions.PolicyName))
                {
                    return;
                }

                // update options
                _optionsCollection[newOptions.PolicyName] = newOptions;

                ConfigurePolicies();
            });
        }

        public IReadOnlyDictionary<string, TOptions> OptionsCollection => _optionsCollection.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        public IReadOnlyDictionary<string, Func<IAsyncPolicy<HttpResponseMessage>>> AsyncPolicyCollection => _asyncPolicyCollection.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        public IReadOnlyDictionary<string, Func<ISyncPolicy<HttpResponseMessage>>> SyncPolicyCollection => _syncPolicyCollection.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        /// <inheritdoc/>
        public IHttpPolicyConfigurator<TOptions> AddPolicy(string policyName, Func<IAsyncPolicy<HttpResponseMessage>> policyFunc, bool replaceIfExists = false)
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

            if (!_asyncPolicyCollection.ContainsKey(policyName))
            {
                _asyncPolicyCollection.Add(policyName, policyFunc);
            }
            else if (replaceIfExists)
            {
                _asyncPolicyCollection[policyName] = policyFunc;
            }

            return this;
        }

        /// <inheritdoc/>
        public IHttpPolicyConfigurator<TOptions> AddPolicy(
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

            if (!_syncPolicyCollection.ContainsKey(policyName))
            {
                _syncPolicyCollection.Add(policyName, policyFunc);
            }
            else if (replaceIfExists)
            {
                _syncPolicyCollection[policyName] = policyFunc;
            }

            return this;
        }

        /// <inheritdoc/>
        public void ConfigurePolicies()
        {
            var registrations = _provider.GetServices(typeof(IHttpPolicy<>).MakeGenericType(new Type[] { typeof(TOptions) }));

            foreach (var registration in registrations)
            {
                if (registration != null)
                {
                    var method = registration.GetType().GetMethod("RegisterPolicy");
                    method.Invoke(registration, Array.Empty<object>());
                }
            }

            // Recreate added policies. Those policies will utilize the updated setting value stored in _settings
            foreach (var asyncPolicy in _asyncPolicyCollection)
            {
                _policyRegistry[asyncPolicy.Key] = asyncPolicy.Value();
            }

            foreach (var syncPolicy in _syncPolicyCollection)
            {
                _policyRegistry[syncPolicy.Key] = syncPolicy.Value();
            }
        }

        /// <inheritdoc/>
        public TOptions GetOptions(string optionsName)
        {
            optionsName = optionsName ?? throw new ArgumentNullException(nameof(optionsName));

            if (_optionsCollection.ContainsKey(optionsName))
            {
                return _optionsCollection[optionsName];
            }
            else
            {
                return null;
            }
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
