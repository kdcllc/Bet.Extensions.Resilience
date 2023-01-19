using Bet.Extensions.Resilience.Abstractions.Options;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Polly;
using Polly.Registry;

namespace Bet.Extensions.Resilience.Abstractions;

public class PolicyBucket<TPolicy, TOptions> : IPolicyBucketLabel
    where TPolicy : IsPolicy
    where TOptions : PolicyOptions
{
    private readonly IOptionsFactory<PolicyBucketOptions<TOptions>> _optionsFactory;
    private readonly IOptionsMonitor<TOptions> _optionsMonitor;
    private readonly Dictionary<string, PolicyLoader<TPolicy, TOptions>> _policies;
    private readonly IPolicyRegistry<string> _policyRegistry;
    private readonly ILoggerFactory _loggerFactory;

    public PolicyBucket(
        IOptionsFactory<PolicyBucketOptions<TOptions>> optionsFactory,
        IOptionsMonitor<TOptions> optionsMonitor,
        IPolicyRegistry<string> policyRegistry,
        ILoggerFactory loggerFactory)
    {
        _optionsFactory = optionsFactory;
        _optionsMonitor = optionsMonitor;

        _policies = new Dictionary<string, PolicyLoader<TPolicy, TOptions>>();
        _policyRegistry = policyRegistry;
        _loggerFactory = loggerFactory;
    }

    public IsPolicy GetPolicy(string policyName)
    {
        if (_policies.ContainsKey(policyName))
        {
            return _policies[policyName].GetPolicy();
        }

        var options = _optionsFactory.Create(policyName);

        var policy = new PolicyLoader<TPolicy, TOptions>(options, _optionsMonitor, _policyRegistry, _loggerFactory);

        _policies.Add(policyName, policy);

        return policy.GetPolicy();
    }
}
