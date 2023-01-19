namespace Bet.Extensions.Resilience.Abstractions.Options;

/// <summary>
/// The <see cref="Polly.Retry.RetryPolicy"/> Policy Options.
/// https://github.com/App-vNext/Polly/wiki/Retry.
/// </summary>
public class RetryPolicyOptions : PolicyOptions
{
    /// <summary>
    /// The retry count. The default value is 3.
    /// </summary>
    public int Count { get; set; } = 3;

    /// <summary>
    /// The back off power to be used for each retry.
    /// </summary>
    public int BackoffPower { get; set; } = 2;
}
