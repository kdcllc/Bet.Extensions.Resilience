namespace Bet.Extensions.Resilience.Abstractions.Options
{
    /// <summary>
    /// The <see cref="Polly.Fallback.FallbackPolicy"/> Policy Options.
    /// Premise: 'If all else fails, degrade gracefully'.
    /// https://github.com/App-vNext/Polly/wiki/Fallback.
    /// </summary>
    /// <example>
    ///
    ///  Policy<UserAvatar>
    ///     .Handle<Whatever>()
    ///     .Fallback<UserAvatar>(UserAvatar.Blank).
    ///
    /// </example>
    public class FallbackPolicyOptions : PolicyOptions
    {
        /// <summary>
        /// The Default policy name.
        /// </summary>
        public static string DefaultName = nameof(FallbackPolicyOptions).Substring(0, nameof(FallbackPolicyOptions).IndexOf("Options"));

        /// <summary>
        /// The Default policy options name.
        /// </summary>
        public static string DefaultNameOptionsName = DefaultName;

        /// <summary>
        /// The Policy Message.
        /// </summary>
        public string Message { get; set; } = string.Empty;
    }
}
