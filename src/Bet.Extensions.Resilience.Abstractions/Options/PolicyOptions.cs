namespace Bet.Extensions.Resilience.Abstractions.Options;

/// <summary>
/// Building block for the <see cref="Polly"/> policies options.
/// </summary>
public class PolicyOptions
{
    /// <summary>
    /// Policy name that associated with the option.
    /// </summary>
    public string Name { get; set; } = string.Empty;
}
