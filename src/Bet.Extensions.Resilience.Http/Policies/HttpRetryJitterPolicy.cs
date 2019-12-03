using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

using Bet.Extensions.Resilience.Abstractions;
using Bet.Extensions.Resilience.Abstractions.Options;
using Bet.Extensions.Resilience.Abstractions.Policies;
using Bet.Extensions.Resilience.Http.Options;

using Microsoft.Extensions.Logging;

using Polly;

namespace Bet.Extensions.Resilience.Http.Policies
{
    public class HttpRetryJitterPolicy :
        RetryJitterPolicy<HttpRetryJitterPolicyOptions, HttpResponseMessage>,
        IHttpRetryJitterPolicy
    {
        public HttpRetryJitterPolicy(
            PolicyOptions policyOptions,
            IPolicyOptionsConfigurator<HttpRetryJitterPolicyOptions> policyOptionsConfigurator,
            IPolicyRegistryConfigurator registryConfigurator,
            ILogger<IPolicy<HttpRetryJitterPolicyOptions, HttpResponseMessage>> logger) : base(policyOptions, policyOptionsConfigurator, registryConfigurator, logger)
        {
        }

        public override IAsyncPolicy<HttpResponseMessage> GetAsyncPolicy()
        {
            OnRetryAsync = (logger, options) =>
            {
                return (outcome, time, attempt, context) =>
                {
                    logger.LogRetryOnRetry(time, attempt, context, options.MaxRetries, outcome.GetMessage());
                    return Task.CompletedTask;
                };
            };

            var delay = Backoff.DecorrelatedJitter(Options.MaxRetries, Options.SeedDelay, Options.MaxDelay);

            return Policy<HttpResponseMessage>
             .Handle<HttpRequestException>()

             .OrResult(TransientHttpStatusCodePredicate)

             // HttpStatusCode.TooManyRequests if not specific logic is applied for the retries.
             .OrResult(x => (int)x.StatusCode == 429)

             .WaitAndRetryAsync(
                    delay,
                    OnRetryAsync(Logger, Options))
             .WithPolicyKey($"{PolicyOptions.Name}{PolicyNameSuffix}");
        }

        public override ISyncPolicy<HttpResponseMessage> GetSyncPolicy()
        {
            OnRetry = (logger, options) =>
            {
                return (outcome, time, attempt, context) => logger.LogRetryOnRetry(time, attempt, context, options.MaxRetries, outcome.GetMessage());
            };

            var delay = Backoff.DecorrelatedJitter(Options.MaxRetries, Options.SeedDelay, Options.MaxDelay);

            return Policy<HttpResponseMessage>
               .Handle<HttpRequestException>()

               .OrResult(TransientHttpStatusCodePredicate)

               // HttpStatusCode.TooManyRequests if not specific logic is applied for the retries.
               .OrResult(x => (int)x.StatusCode == 429)

               .WaitAndRetry(
                    delay,
                    onRetry: OnRetry(Logger, Options))
               .WithPolicyKey(PolicyOptions.Name);
        }

        private bool TransientHttpStatusCodePredicate(HttpResponseMessage response)
        {
            return (int)response.StatusCode >= 500 || response.StatusCode == HttpStatusCode.RequestTimeout;
        }
    }
}
