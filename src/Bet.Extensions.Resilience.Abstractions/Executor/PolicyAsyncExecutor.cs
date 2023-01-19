using Polly;
using Polly.Wrap;

namespace Bet.Extensions.Resilience.Abstractions.Executor;

/// <inheritdoc/>
public sealed class PolicyAsyncExecutor : IPolicyAsyncExecutor
{
    private readonly IEnumerable<IAsyncPolicy> _asyncPolicies;
    private readonly AsyncPolicyWrap _policyWrap;

    /// <summary>
    /// Initializes a new instance of the <see cref="PolicyAsyncExecutor"/> class.
    /// </summary>
    /// <param name="policies"></param>
    public PolicyAsyncExecutor(IEnumerable<IAsyncPolicy> policies)
    {
        _asyncPolicies = policies ?? throw new ArgumentNullException(nameof(policies));
        _policyWrap = Policy.WrapAsync(_asyncPolicies.ToArray());
    }

    /// <inheritdoc/>
    public async Task<T> ExecuteAsync<T>(Func<Task<T>> action)
    {
        return await _policyWrap.ExecuteAsync(action);
    }

    /// <inheritdoc/>
    public async Task ExecuteAsync(Func<Task> action)
    {
        await _policyWrap.ExecuteAsync(action);
    }

    public async Task<T> ExecuteAsync<T>(Func<CancellationToken, Task<T>> action, CancellationToken cancellationToken)
    {
        return await _policyWrap.ExecuteAsync(action, cancellationToken);
    }
}
