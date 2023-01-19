namespace Bet.Extensions.Resilience.Abstractions.Options;

/// <summary>
/// Commonly used policy options.
/// </summary>
public static class PolicyOptionsKeys
{
    public const string AdvancedCircuitBreakerPolicy = nameof(AdvancedCircuitBreakerPolicy);

    public const string BulkheadPolicy = nameof(BulkheadPolicy);

    public const string CircuitBreakerPolicy = nameof(CircuitBreakerPolicy);

    public const string FallbackPolicy = nameof(FallbackPolicy);

    public const string RetryJitterPolicy = nameof(RetryJitterPolicy);

    public const string RetryPolicy = nameof(RetryPolicy);

    public const string TimeoutPolicy = nameof(TimeoutPolicy);
}
