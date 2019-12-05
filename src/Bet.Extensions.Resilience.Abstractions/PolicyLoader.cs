using System;

using Bet.Extensions.Resilience.Abstractions.Options;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Polly;
using Polly.Registry;

namespace Bet.Extensions.Resilience.Abstractions
{
    public class PolicyLoader<TPolicy, TOptions> where TPolicy : IsPolicy where TOptions : PolicyOptions
    {
        private readonly ILogger<PolicyLoader<TPolicy, TOptions>> _logger;
        private readonly IPolicyRegistry<string> _policyRegistry;
        private readonly PolicyProfileOptions<TOptions> _policyProfileOptions;

        private TOptions _options;

        public PolicyLoader(
            PolicyProfileOptions<TOptions> policyProfileOptions,
            IOptionsMonitor<TOptions> optionsMonitor,
            IPolicyRegistry<string> policyRegistry,
            ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<PolicyLoader<TPolicy, TOptions>>();
            _policyRegistry = policyRegistry ?? throw new ArgumentNullException(nameof(policyRegistry));

            _options = optionsMonitor.Get(policyProfileOptions.Name);

            optionsMonitor.OnChange((options, name) =>
            {
                if (name == policyProfileOptions.Name)
                {
                    _options = options;
                    GetPolicy();
                }
            });
            _policyProfileOptions = policyProfileOptions ?? throw new ArgumentNullException(nameof(policyProfileOptions));
        }

        public IsPolicy GetPolicy()
        {
            var policy = _policyProfileOptions.ConfigurePolicy(_options, _logger);
            AddPolicy(_options.Name, () => policy, true);
            return _policyProfileOptions.ConfigurePolicy(_options, _logger);
        }

        public PolicyLoader<TPolicy, TOptions> AddPolicy(string policyName, Func<IsPolicy> policyFunc, bool replaceIfExists = false)
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
    }
}
