using Polly;
using Polly.Wrap;

namespace Bet.Extensions.Resilience.Abstractions.Executor;

public class PolicySyncExecutor : IPolicySyncExecutor
{
    private readonly IEnumerable<ISyncPolicy> _syncPolicies;
    private readonly PolicyWrap _policyWrap;

    /// <summary>
    /// Initializes a new instance of the <see cref="PolicySyncExecutor"/> class.
    /// </summary>
    /// <param name="policies"></param>
    public PolicySyncExecutor(IEnumerable<ISyncPolicy> policies)
    {
        _syncPolicies = policies ?? throw new ArgumentNullException(nameof(policies));
        _policyWrap = Policy.Wrap(_syncPolicies.ToArray());
    }

    /// <inheritdoc/>
    public T Execute<T>(Func<T> action)
    {
        return _policyWrap.Execute(action);
    }

    /// <inheritdoc/>
    public void Execute(Action action)
    {
        _policyWrap.Execute(action);
    }
}
