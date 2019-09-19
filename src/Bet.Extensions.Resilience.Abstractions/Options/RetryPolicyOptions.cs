namespace Bet.Extensions.Resilience.Abstractions.Options
{
    /// <summary>
    /// The options for Retry Polly Policy.
    /// </summary>
    public class RetryPolicyOptions
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
}
