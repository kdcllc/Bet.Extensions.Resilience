using System.Net.Http;

namespace Bet.Extensions.Resilience.Http.Setup
{
    /// <summary>
    /// The default list of the Polly policies for <see cref="HttpClient"/>.
    /// </summary>
    public class PolicyKeys
    {
        public const string HttpRetryAsyncPolicy = nameof(HttpRetryAsyncPolicy);
        public const string HttpRetrySyncPolicy = nameof(HttpRetrySyncPolicy);

        public const string HttpCircuitBreakerAsyncPolicy = nameof(HttpCircuitBreakerAsyncPolicy);
        public const string HttpCircuitBreakerSyncPolicy = nameof(HttpCircuitBreakerSyncPolicy);
    }
}
