using System.Net.Http;

namespace Bet.Extensions.Resilience.Abstractions
{
    /// <summary>
    /// The default list of the Polly policies for <see cref="HttpClient"/>.
    /// </summary>
    public sealed class PolicyName
    {
        public const string RetryPolicyAsync = nameof(RetryPolicyAsync);
        public const string RetryPolicy = nameof(RetryPolicy);

        public const string RetryJitterAsync = nameof(RetryJitterAsync);
        public const string RetryJitter = nameof(RetryJitter);

        public const string CircuitBreakerPolicyAsync = nameof(CircuitBreakerPolicyAsync);
        public const string CircuitBreakerPolicy = nameof(CircuitBreakerPolicy);

        public const string TimeoutPolicyAsync = nameof(TimeoutPolicyAsync);
        public const string TimeoutPolicy = nameof(TimeoutPolicy);

        public const string DefaultPolicy = nameof(DefaultPolicy);
    }
}
