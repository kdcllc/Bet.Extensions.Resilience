using System;
using System.Net.Http;

using Bet.Extensions.Resilience.Abstractions.Options;

namespace Bet.Extensions.Resilience.Http.Options
{
    /// <summary>
    /// The default policy option.
    /// The default configuration section name is 'Policies'.
    /// </summary>
    public class HttpPolicyOptions
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
        /// The Retry Policy for Options for <see cref="HttpResponseMessage"/>.
        /// </summary>
        public RetryPolicyOptions HttpRetry { get; set; } = new RetryPolicyOptions();

        // TODO: check this later
        public RequestTimeoutOptions HttpRequestTimeout { get; set; } = new RequestTimeoutOptions();

        /// <summary>
        /// This is used for DI mapping.
        /// </summary>
        internal string PolicyName { get; set; }
    }
}
