using System;

namespace Bet.Extensions.Resilience.Abstractions.Options
{
    /// <summary>
    /// Retry with Jitter Policy Options.
    /// </summary>
    public class RetryJitterPolicyOptions
    {
        /// <summary>
        /// Maximum retries for the Retries. The default is 2.
        /// </summary>
        public int MaxRetries { get; set; } = 2;

        /// <summary>
        /// The Maximum delay within the retry. The default is 200 seconds.
        /// </summary>
        public TimeSpan MaxDelay { get; set; } = TimeSpan.FromSeconds(200);
    }
}
