namespace Bet.Extensions.Resilience.Abstractions.Options;

/// <summary>
/// The <see cref="Polly.Fallback.FallbackPolicy"/> Policy Options.
/// Premise: 'If all else fails, degrade gracefully'.
/// https://github.com/App-vNext/Polly/wiki/Fallback.
/// </summary>
/// <example>
/// <![CDATA[
///  Policy.<UserAvatar>
///     .Handle<Whatever>()
///     .Fallback<UserAvatar>(UserAvatar.Blank).
/// ]]>
/// </example>
public class FallbackPolicyOptions : PolicyOptions
{
    /// <summary>
    /// The Policy Message.
    /// </summary>
    public string Message { get; set; } = string.Empty;
}
