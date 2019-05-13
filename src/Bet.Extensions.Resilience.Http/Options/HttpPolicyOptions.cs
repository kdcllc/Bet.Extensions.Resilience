﻿namespace Bet.Extensions.Resilience.Http.Options
{
    /// <summary>
    /// The default policy option. The root configuration is ``.
    /// </summary>
    public class HttpPolicyOptions
    {
        public HttpPolicyOptions()
        {
            HttpCircuitBreaker = new CircuitBreakerPolicyOptions();
            HttpRetry = new RetryPolicyOptions();
        }

        public HttpPolicyOptions(
            CircuitBreakerPolicyOptions circuitBreakerPolicyOptions,
            RetryPolicyOptions retryPolicyOptions)
        {
            HttpCircuitBreaker = circuitBreakerPolicyOptions ?? throw new System.ArgumentNullException(nameof(circuitBreakerPolicyOptions));
            HttpRetry = retryPolicyOptions ?? throw new System.ArgumentNullException(nameof(retryPolicyOptions));
        }

        public CircuitBreakerPolicyOptions HttpCircuitBreaker { get; set; }

        public RetryPolicyOptions HttpRetry { get; set; }
    }
}