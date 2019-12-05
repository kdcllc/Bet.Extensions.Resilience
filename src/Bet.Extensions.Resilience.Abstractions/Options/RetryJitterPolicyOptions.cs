using System;

namespace Bet.Extensions.Resilience.Abstractions.Options
{
    /// <summary>
    /// The <see cref="Polly.Retry.RetryPolicy"/> with Jitter Policy Options.
    /// https://github.com/App-vNext/Polly/wiki/Retry-with-jitter.
    /// </summary>
    public class RetryJitterPolicyOptions : PolicyOptions
    {
        /// <summary>
        /// Maximum retries for the Retries. The default is 2.
        /// </summary>
        public int MaxRetries { get; set; } = 2;

        /// <summary>
        /// The Maximum delay within the retry. The default is 200 seconds.
        /// </summary>
        public TimeSpan MaxDelay { get; set; } = TimeSpan.FromSeconds(200);

        /// <summary>
        /// The seed time delay.
        /// </summary>
        public TimeSpan SeedDelay { get; set; } = TimeSpan.FromMilliseconds(100);
    }
}
