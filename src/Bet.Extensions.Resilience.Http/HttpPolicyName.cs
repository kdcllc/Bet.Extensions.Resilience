using Bet.Extensions.Resilience.Abstractions.Options;

namespace Bet.Extensions.Resilience.Http
{
    public static class HttpPolicyName
    {
        public const string DefaultHttpPolicy = "HttpPolicies";

        public const string DefaultHttpPolicyOptionsName = DefaultHttpPolicy;

        public static string DefaultHttpTimeoutPolicy = $"Http{TimeoutPolicyOptions.DefaultName}";

        public static string DefaultHttpFallbackPolicyPolicy = $"Http{FallbackPolicyOptions.DefaultName}";

        public static string DefaultHttpRetryPolicy = $"Http{RetryPolicyOptions.DefaultName}";

        public static string DefaultHttpCircuitBreakerPolicy = $"Http{CircuitBreakerPolicyOptions.DefaultName}";
    }
}
