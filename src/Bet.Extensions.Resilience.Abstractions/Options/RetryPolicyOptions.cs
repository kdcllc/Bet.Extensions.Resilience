namespace Bet.Extensions.Resilience.Abstractions.Options
{
    /// <summary>
    /// The <see cref="Polly.Retry.RetryPolicy"/> Policy Options.
    /// https://github.com/App-vNext/Polly/wiki/Retry.
    /// </summary>
    public class RetryPolicyOptions : PolicyOptions
    {
        /// <summary>
        /// The Default policy name.
        /// </summary>
        public static string DefaultName = nameof(RetryPolicyOptions).Substring(0, nameof(RetryPolicyOptions).IndexOf("Options"));

        /// <summary>
        /// The Default policy options name.
        /// </summary>
        public static string DefaultNameOptionsName = DefaultName;

        /// <summary>
        /// The retry count. The default value is 3.
        /// </summary>
        public int Count { get; set; } = 3;

        /// <summary>
        /// The back off power to be used for each retry.
        /// </summary>
        public int BackoffPower { get; set; } = 2;
    }
}
