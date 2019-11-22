using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

using Bet.Extensions.Resilience.Abstractions.Options;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using Polly;
using Polly.Registry;

namespace Bet.Extensions.Resilience.Abstractions
{
    /// <inheritdoc/>
    public class DefaultPolicyConfigurator<TOptions, TResult> : IPolicyConfigurator<TOptions, TResult> where TOptions : PolicyOptions
    {
        private readonly IDictionary<string, TOptions> _optionsCollection = new ConcurrentDictionary<string, TOptions>();
        private readonly IDictionary<string, Func<IAsyncPolicy<TResult>>> _asyncPolicyCollection = new ConcurrentDictionary<string, Func<IAsyncPolicy<TResult>>>();
        private readonly IDictionary<string, Func<ISyncPolicy<TResult>>> _syncPolicyCollection = new ConcurrentDictionary<string, Func<ISyncPolicy<TResult>>>();

        private readonly IOptionsMonitor<TOptions> _optionsMonitor;
        private readonly IPolicyRegistry<string> _policyRegistry;
        private readonly IServiceProvider _provider;

        public DefaultPolicyConfigurator(
            IServiceProvider provider,
            string parentPolicyName,
            string[] ? childrenPolicyNames = null)
        {
            _provider = provider;

            ParentPolicyName = parentPolicyName;
            ChildrenPolicyNames = childrenPolicyNames;

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

            _optionsMonitor.OnChange(newPolicyOptions =>
            {
                // ignore the default none named option
                if (string.IsNullOrWhiteSpace(newPolicyOptions.Name))
                {
                    return;
                }

                // update options
                _optionsCollection[newPolicyOptions.Name] = newPolicyOptions;

                ConfigurePolicies();
            });
        }

        public IReadOnlyDictionary<string, TOptions> OptionsCollection => _optionsCollection.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        public IReadOnlyDictionary<string, Func<IAsyncPolicy<TResult>>> AsyncPolicyCollection => _asyncPolicyCollection.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        public IReadOnlyDictionary<string, Func<ISyncPolicy<TResult>>> SyncPolicyCollection => _syncPolicyCollection.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        public string ParentPolicyName { get; private set; }

        public string[]? ChildrenPolicyNames { get; private set; }

        /// <inheritdoc/>
        public IPolicyConfigurator<TOptions, TResult> AddPolicy(
            string policyName,
            Func<IAsyncPolicy<TResult>> policyFunc,
            bool replaceIfExists = false)
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
        public IPolicyConfigurator<TOptions, TResult> AddPolicy(
            string policyName,
            Func<ISyncPolicy<TResult>> policyFunc,
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
            var registrations = _provider.GetServices(typeof(IPolicyCreator<,>).MakeGenericType(new Type[] { typeof(TOptions), typeof(TResult) }));

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
