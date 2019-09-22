using System.Net.Http;

namespace Bet.Extensions.Resilience.Http.Setup
{
    /// <summary>
    /// The default list of the Polly policies for <see cref="HttpClient"/>.
    /// </summary>
    public sealed class PolicyKeys
    {
        public const string HttpWaitAndRetryPolicyAsync = nameof(HttpWaitAndRetryPolicyAsync);
        public const string HttpWaitAndRetryPolicy = nameof(HttpWaitAndRetryPolicy);

        public const string HttpCircuitBreakerPolicyAsync = nameof(HttpCircuitBreakerPolicyAsync);
        public const string HttpCircuitBreakerPolicy = nameof(HttpCircuitBreakerPolicy);

        public const string HttpRequestTimeoutPolicyAsync = nameof(HttpRequestTimeoutPolicyAsync);
        public const string HttpRequestTimeoutPolicy = nameof(HttpRequestTimeoutPolicy);
    }
}
