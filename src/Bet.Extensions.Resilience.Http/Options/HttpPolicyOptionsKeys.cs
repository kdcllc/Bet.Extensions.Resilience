namespace Bet.Extensions.Resilience.Http.Options
{
    public static class HttpPolicyOptionsKeys
    {
        public const string DefaultHttpPolicy = "HttpPolicies";

        public const string HttpAdvancedCircuitBreakerPolicy = nameof(HttpAdvancedCircuitBreakerPolicy);

        public const string HttpBulkheadPolicy = nameof(HttpBulkheadPolicy);

        public const string HttpCircuitBreakerPolicy = nameof(HttpCircuitBreakerPolicy);

        public const string HttpFallbackPolicy = nameof(HttpFallbackPolicy);

        public const string HttpRetryJitterPolicy = nameof(HttpRetryJitterPolicy);

        public const string HttpRetryPolicy = nameof(HttpRetryPolicy);

        public const string HttpTimeoutPolicy = nameof(HttpTimeoutPolicy);
    }
}
