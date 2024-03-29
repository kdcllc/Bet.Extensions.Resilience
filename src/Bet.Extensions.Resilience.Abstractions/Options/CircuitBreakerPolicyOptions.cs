﻿namespace Bet.Extensions.Resilience.Abstractions.Options;

/// <summary>
/// The <see cref="Polly.CircuitBreaker.CircuitBreakerPolicy"/> Policy Options.
/// </summary>
public class CircuitBreakerPolicyOptions : PolicyOptions
{
    /// <summary>
    /// The Timespan for Circuit Breaker to wait before reseting. The default value is 30 seconds.
    /// </summary>
    public TimeSpan DurationOfBreak { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// The number of Exceptions allowed before breaking circuit breaker. The default is 5 times.
    /// </summary>
    public int ExceptionsAllowedBeforeBreaking { get; set; } = 5;
}
