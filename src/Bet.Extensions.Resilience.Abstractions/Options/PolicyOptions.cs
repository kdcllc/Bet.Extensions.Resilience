using System;
using System.Net.Http;

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
        /// This value matches the default of <see cref="HttpClient"/>.
        /// </summary>
        public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(100);

        /// <summary>
        /// The Circuit Breaker Policy Options for <see cref="HttpResponseMessage"/>.
        /// </summary>
        public CircuitBreakerPolicyOptions HttpCircuitBreaker { get; set; } = new CircuitBreakerPolicyOptions();

        /// <summary>
        /// The Retry Policy Options for <see cref="HttpResponseMessage"/>.
        /// </summary>
        public RetryPolicyOptions HttpRetry { get; set; } = new RetryPolicyOptions();

        /// <summary>
        /// The Retry Policy with Jitter Options for <see cref="HttpResponseMessage"/>.
        /// </summary>
        public RetryJitterOptions HttpJitterRetry { get; set; } = new RetryJitterOptions();

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
