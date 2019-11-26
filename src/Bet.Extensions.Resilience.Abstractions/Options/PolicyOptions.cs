using System;

namespace Bet.Extensions.Resilience.Abstractions.Options
{
    /// <summary>
    /// The default policy option.
    /// The default configuration section name is 'Policies'.
    /// </summary>
    public class PolicyOptions
    {
        /// <summary>
        /// The timeout policy options. The default is 100 seconds or 00:01:40.
        /// </summary>
        public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(100);

        /// <summary>
        /// The Circuit Breaker Policy Options.
        /// </summary>
        public CircuitBreakerPolicyOptions CircuitBreaker { get; set; } = new CircuitBreakerPolicyOptions();

        /// <summary>
        /// The Retry Policy Options.
        /// </summary>
        public RetryPolicyOptions Retry { get; set; } = new RetryPolicyOptions();

        /// <summary>
        /// The Retry Policy with Jitter Options.
        /// </summary>
        public RetryJitterPolicyOptions JitterRetry { get; set; } = new RetryJitterPolicyOptions();

        /// <summary>
        /// Adds Builkhead Policy Options.
        /// </summary>
        public BulkheadPolicyOptions Bulkhead { get; set; } = new BulkheadPolicyOptions();

        /// <summary>
        /// This is used for DI mapping.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Configuration section name associated with this configuration.
        /// </summary>
        public string OptionsName { get; set; } = string.Empty;
    }
}
