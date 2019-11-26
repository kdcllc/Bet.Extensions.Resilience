using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using Bet.Extensions.Resilience.Abstractions.Options;

using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;

namespace Bet.Extensions.Resilience.Abstractions.Internal
{
    /// <summary>
    /// The Default implementation for <see cref="IPolicyOptionsConfigurator{TOptions}"/>.
    /// </summary>
    /// <typeparam name="TOptions"></typeparam>
    internal class DefaultPolicyOptionsConfigurator<TOptions> : IPolicyOptionsConfigurator<TOptions> where TOptions : PolicyOptions
    {
        private readonly IDictionary<string, TOptions> _optionsCollection = new ConcurrentDictionary<string, TOptions>();
        private readonly IOptionsMonitor<TOptions> _optionsMonitor;
        private OptionsReloadToken _optionsReloadToken;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultPolicyOptionsConfigurator{TOptions}"/> class.
        /// </summary>
        /// <param name="optionsMonitor">The options monitor for a specific type of options.</param>
        /// <param name="policyOptions">The policy options registrant.</param>
        public DefaultPolicyOptionsConfigurator(
            IOptionsMonitor<TOptions> optionsMonitor,
            PolicyOptionsRegistrant policyOptions)
        {
            _optionsMonitor = optionsMonitor;

            // register all of the options.
            foreach (var policyOption in policyOptions.RegisteredPolicyOptions)
            {
                _optionsCollection.Add(policyOption.Key, _optionsMonitor.Get(policyOption.Key));
            }

            _optionsReloadToken = new OptionsReloadToken();

            _optionsMonitor.OnChange(changedPolicyOptions =>
            {
                // ignore the default none named option
                // Note: This listener will be called twice for every registered setting/option due to the base .net core
                // implementation not having a good way to fix. https://github.com/aspnet/AspNetCore/issues/2542
                if (string.IsNullOrWhiteSpace(changedPolicyOptions.OptionsName))
                {
                    return;
                }

                if (_optionsCollection.ContainsKey(changedPolicyOptions.OptionsName))
                {
                    var previousToken = Interlocked.Exchange(ref _optionsReloadToken, new OptionsReloadToken());

                    // update options
                    _optionsCollection[changedPolicyOptions.Name] = changedPolicyOptions;

                    previousToken.OnReload();
                }
            });
        }

        /// <inheritdoc/>
        public IReadOnlyDictionary<string, TOptions> GetAllOptions => _optionsCollection.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        /// <inheritdoc/>
        public TOptions GetOptions(string optionsName)
        {
            optionsName = optionsName ?? throw new ArgumentNullException(nameof(optionsName));

            if (!_optionsCollection.ContainsKey(optionsName))
            {
                SetOptions(optionsName);
            }

            return _optionsCollection[optionsName];
        }

        /// <inheritdoc/>
        public IChangeToken GetChangeToken()
        {
            if (_optionsReloadToken == null)
            {
                throw new InvalidOperationException($"{nameof(IPolicyOptionsConfigurator<TOptions>)} must be instantiated");
            }

            return _optionsReloadToken;
        }

        /// <summary>
        /// Stores a named setting retrieved from options system.
        /// </summary>
        /// <param name="optionsName"></param>
        private void SetOptions(string optionsName)
        {
            var settingsName = optionsName ?? throw new ArgumentNullException(nameof(optionsName));

            _optionsCollection[settingsName] = _optionsMonitor.Get(settingsName);
        }
    }
}
