using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

using Bet.Extensions.Resilience.Abstractions;
using Bet.Extensions.Resilience.Abstractions.Options;
using Bet.Extensions.Resilience.Abstractions.Policies;

using Microsoft.Extensions.Logging;

using Polly;

namespace Bet.Extensions.Resilience.Http.Policies
{
    /// <summary>
    /// Default Wait And Retry Polly Policy.
    /// </summary>
    /// <typeparam name="TOptions">The type of the options.</typeparam>
    public class HttpRetryPolicy<TOptions> :
        RetryPolicy<TOptions, HttpResponseMessage>,
        IHttpRetryPolicy<TOptions>
        where TOptions : RetryPolicyOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HttpRetryPolicy{TOptions}"/> class.
        /// </summary>
        /// <param name="policyOptions"></param>
        /// <param name="policyOptionsConfigurator"></param>
        /// <param name="registryConfigurator"></param>
        /// <param name="logger"></param>
        public HttpRetryPolicy(
            PolicyOptions policyOptions,
            IPolicyOptionsConfigurator<TOptions> policyOptionsConfigurator,
            IPolicyRegistryConfigurator registryConfigurator,
            ILogger<IPolicy<TOptions>> logger) : base(policyOptions, policyOptionsConfigurator, registryConfigurator, logger)
        {
            OnDuration = (logger, options) =>
            {
                return (attempt, outcome, context) =>
                {
                    logger.LogRetryOnDuration(attempt, context, options, outcome.GetMessage());
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
                    logger.LogRetryOnRetry(time, attempt, context, options, outcome.GetMessage());
                    return Task.CompletedTask;
                };
            };

            return Policy<HttpResponseMessage>
               .Handle<HttpRequestException>()
               .OrResult(TransientHttpStatusCodePredicate)
                   .WaitAndRetryAsync(
                       retryCount: Options.Count,
                       sleepDurationProvider: OnDuration(Logger, Options),
                       onRetryAsync: OnRetryAsync(Logger, Options));
        }

        /// <inheritdoc/>
        public override ISyncPolicy<HttpResponseMessage> GetSyncPolicy()
        {
            OnRetry = (logger, options) =>
            {
                return (outcome, time, attempt, context) => logger.LogRetryOnRetry(time, attempt, context, options, outcome.GetMessage());
            };

            return Policy<HttpResponseMessage>
               .Handle<HttpRequestException>()
               .OrResult(TransientHttpStatusCodePredicate)
                .WaitAndRetry(
                    retryCount: Options.Count,
                    sleepDurationProvider: OnDuration(Logger, Options),
                    onRetry: OnRetry(Logger, Options));
        }

        private bool TransientHttpStatusCodePredicate(HttpResponseMessage response)
        {
            return (int)response.StatusCode >= 500 || response.StatusCode == HttpStatusCode.RequestTimeout;
        }
    }
}
