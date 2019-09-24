using System.Net.Http;

namespace Bet.Extensions.Resilience.Http.Policies
{
    /// <summary>
    /// The default list of the Polly policies for <see cref="HttpClient"/>.
    /// </summary>
    public sealed class HttpPoliciesKeys
    {
        public const string HttpWaitAndRetryPolicyAsync = nameof(HttpWaitAndRetryPolicyAsync);
        public const string HttpWaitAndRetryPolicy = nameof(HttpWaitAndRetryPolicy);

        public const string HttpCircuitBreakerPolicyAsync = nameof(HttpCircuitBreakerPolicyAsync);
        public const string HttpCircuitBreakerPolicy = nameof(HttpCircuitBreakerPolicy);

        public const string HttpTimeoutPolicyAsync = nameof(HttpTimeoutPolicyAsync);
        public const string HttpTimeoutPolicy = nameof(HttpTimeoutPolicy);

        public const string DefaultPolicies = nameof(DefaultPolicies);
    }
}
