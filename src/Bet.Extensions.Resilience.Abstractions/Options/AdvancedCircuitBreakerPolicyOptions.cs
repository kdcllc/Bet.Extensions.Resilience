namespace Bet.Extensions.Resilience.Abstractions.Options;

/// <summary>
/// The <see cref="Polly.CircuitBreaker.AsyncCircuitBreakerPolicy"/> Policy Options.
/// https://github.com/App-vNext/Polly/wiki/Advanced-Circuit-Breaker.
/// </summary>
public class AdvancedCircuitBreakerPolicyOptions : PolicyOptions
{
    /// <summary>
    /// The Failure threshold in percents.
    /// Reacts on proportion of failures, the failureThreshold; eg break if over 50% of actions result in a handled failure.
    /// The default is 0.5.
    /// For example: if set to 0.5 ... Break on >=50% actions result in handled exceptions...
    /// </summary>
    public double FailureThreshold { get; set; } = 0.5;

    /// <summary>
    /// The sampling occurs over the specified period of time.
    /// Measures that proportion over a rolling samplingDuration, so that older failures can be excluded and have no effect.
    /// The default is 00:00:05.
    /// For example: 00:00:10 ... over any 10 seconds period.
    /// </summary>
    public TimeSpan SamplingDuration { get; set; } = TimeSpan.FromSeconds(5);

    /// <summary>
    /// The minimum actions to be performed during the sampling duration.
    /// Imposes a minimumThroughput before acting,
    /// such that the circuit reacts only when statistically significant, and does not trip in 'slow' periods
    /// The default is 20.
    /// For example: 8 ... provided at least 8 actions in the 10 second period.
    /// </summary>
    public int MinimumThroughput { get; set; } = 20;

    /// <summary>
    /// The duration of the break. The default is 30 seconds.
    /// </summary>
    public TimeSpan DurationOfBreak { get; set; } = TimeSpan.FromSeconds(30);
}
