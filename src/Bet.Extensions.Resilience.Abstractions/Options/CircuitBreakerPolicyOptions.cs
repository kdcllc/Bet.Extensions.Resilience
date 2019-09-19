using System;

namespace Bet.Extensions.Resilience.Http.Options
{
    /// <summary>
    /// The options for Circuit Breaker Polly Policy.
    /// </summary>
    public class CircuitBreakerPolicyOptions
    {
        /// <summary>
        /// The timespan for Circuit Breaker to wait before reseting. The default value is 30 seconds.
        /// </summary>
        public TimeSpan DurationOfBreak { get; set; } = TimeSpan.FromSeconds(30);

        /// <summary>
        /// The number of Exceptions allowed before breaking circuit breaker. The default is 2 times.
        /// </summary>
        public int ExceptionsAllowedBeforeBreaking { get; set; } = 2;
    }
}
