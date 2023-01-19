namespace Bet.Extensions.Resilience.Abstractions.Options;

/// <summary>
/// The <see cref="Polly.Timeout.TimeoutPolicy"/> Policy Options.
/// Premise: 'Don't wait forever'.
/// https://github.com/App-vNext/Polly/wiki/Timeout.
/// </summary>
public class TimeoutPolicyOptions : PolicyOptions
{
    /// <summary>
    /// The default is 100 seconds or 00:01:40.
    /// </summary>
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(100);
}
