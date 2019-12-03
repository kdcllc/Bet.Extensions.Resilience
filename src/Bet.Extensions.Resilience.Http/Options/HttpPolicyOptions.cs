using Bet.Extensions.Resilience.Abstractions.Options;

namespace Bet.Extensions.Resilience.Http.Options
{
    public class HttpPolicyOptions : PolicyOptions
    {
        public FallbackPolicyOptions FallbackPolicy { get; set; } = new FallbackPolicyOptions();

        public TimeoutPolicyOptions TimeoutPolicy { get; set; } = new TimeoutPolicyOptions();

        public HttpRetryPolicyOptions HttpRetryPolicy { get; set; } = new HttpRetryPolicyOptions();

        public CircuitBreakerPolicyOptions CircuitBreakerPolicy { get; set; } = new CircuitBreakerPolicyOptions();
    }
}
