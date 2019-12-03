using System;
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
    /// <summary>
    /// Default Wait And Retry Polly Policy.
    /// </summary>
    public class HttpRetryPolicy :
        RetryPolicy<HttpRetryPolicyOptions, HttpResponseMessage>,
        IHttpRetryPolicy
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HttpRetryPolicy"/> class.
        /// </summary>
        /// <param name="policyOptions"></param>
        /// <param name="policyOptionsConfigurator"></param>
        /// <param name="registryConfigurator"></param>
        /// <param name="logger"></param>
        public HttpRetryPolicy(
            PolicyOptions policyOptions,
            IPolicyOptionsConfigurator<HttpRetryPolicyOptions> policyOptionsConfigurator,
            IPolicyRegistryConfigurator registryConfigurator,
            ILogger<IPolicy<HttpRetryPolicyOptions, HttpResponseMessage>> logger) : base(policyOptions, policyOptionsConfigurator, registryConfigurator, logger)
        {
            OnDuration = (logger, options) =>
            {
                return (attempt, outcome, context) =>
                {
                    logger.LogRetryOnDuration(attempt, context, options.Count, outcome.GetMessage());
                    return TimeSpan.FromSeconds(Math.Pow(options.BackoffPower, attempt));
                };
            };
        }

        /// <inheritdoc/>
        public override IAsyncPolicy<HttpResponseMessage> GetAsyncPolicy()
        {
            OnRetryAsync = (logger, options) =>
            {
                return (outcome, time, attempt, context) =>
                {
                    logger.LogRetryOnRetry(time, attempt, context, options.Count, outcome.GetMessage());
                    return Task.CompletedTask;
                };
            };

            return Policy<HttpResponseMessage>
               .Handle<HttpRequestException>()

               .OrResult(TransientHttpStatusCodePredicate)

               // HttpStatusCode.TooManyRequests if not specific logic is applied for the retries.
               .OrResult(x => (int)x.StatusCode == 429)

               .WaitAndRetryAsync(
                       retryCount: Options.Count,
                       sleepDurationProvider: OnDuration(Logger, Options),
                       onRetryAsync: OnRetryAsync(Logger, Options))
               .WithPolicyKey($"{PolicyOptions.Name}{PolicyNameSuffix}");
        }

        /// <inheritdoc/>
        public override ISyncPolicy<HttpResponseMessage> GetSyncPolicy()
        {
            OnRetry = (logger, options) =>
            {
                return (outcome, time, attempt, context) => logger.LogRetryOnRetry(time, attempt, context, options.Count, outcome.GetMessage());
            };

            return Policy<HttpResponseMessage>
               .Handle<HttpRequestException>()

               .OrResult(TransientHttpStatusCodePredicate)

               // HttpStatusCode.TooManyRequests if not specific logic is applied for the retries.
               .OrResult(x => (int)x.StatusCode == 429)

               .WaitAndRetry(
                    retryCount: Options.Count,
                    sleepDurationProvider: OnDuration(Logger, Options),
                    onRetry: OnRetry(Logger, Options))
               .WithPolicyKey(PolicyOptions.Name);
        }

        private bool TransientHttpStatusCodePredicate(HttpResponseMessage response)
        {
            return (int)response.StatusCode >= 500 || response.StatusCode == HttpStatusCode.RequestTimeout;
        }
    }
}
