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
    /// <typeparam name="TResult"></typeparam>
    public class HttpRetryPolicy<TOptions, TResult> :
        BasePolicy<TOptions, HttpResponseMessage>,
        IHttpRetryPolicy<TOptions, HttpResponseMessage>
        where TOptions : RetryPolicyOptions
    {
        private readonly ILogger<IPolicy<TOptions>> _logger;

        public HttpRetryPolicy(
            PolicyOptions policyOptions,
            IPolicyOptionsConfigurator<TOptions> policyOptionsConfigurator,
            IPolicyRegistryConfigurator registryConfigurator,
            ILogger<IPolicy<TOptions>> logger) : base(policyOptions, policyOptionsConfigurator, registryConfigurator, logger)
        {
            _logger = logger;
        }

        /// <inheritdoc/>
        public override IAsyncPolicy<HttpResponseMessage> GetAsyncPolicy()
        {
            return Policy<HttpResponseMessage>
               .Handle<HttpRequestException>()
               .OrResult(TransientHttpStatusCodePredicate)
                   .WaitAndRetryAsync(
                       retryCount: Options.Count,
                       sleepDurationProvider: OnDurationAsync,
                       onRetryAsync: OnRetryAsync);
        }

        /// <inheritdoc/>
        public override ISyncPolicy<HttpResponseMessage> GetSyncPolicy()
        {
            return Policy<HttpResponseMessage>
               .Handle<HttpRequestException>()
               .OrResult(TransientHttpStatusCodePredicate)
                .WaitAndRetry(
                    retryCount: Options.Count,
                    sleepDurationProvider: (retryAttempt, c) => OnDuration(retryAttempt, c),
                    onRetry: (result, timeSpan, retryAttempt, context) => OnRetry(result, timeSpan, retryAttempt, context));
        }

        private bool TransientHttpStatusCodePredicate(HttpResponseMessage response)
        {
            return (int)response.StatusCode >= 500 || response.StatusCode == HttpStatusCode.RequestTimeout;
        }

        private void OnRetry(DelegateResult<HttpResponseMessage> delegateResult, TimeSpan timeElapsed, int retryAttempt, Context context)
        {
            _logger.LogInformation(
                "[Retry Policy][OnRetryAsync] OperationKey: {OperationKey}; CorrelationId: {CorrelationId}; TimeElapsed: {TimeElapsed}; Retries: {retryNumber}; ExceptionMessage: {ExceptionMessage}",
                context.OperationKey,
                context.CorrelationId,
                timeElapsed,
                retryAttempt,
                delegateResult.GetMessage());
        }

        private TimeSpan OnDuration(int retryAttempt, Context context)
        {
            _logger.LogWarning(
            "[Retry Policy][OnDuration] OperationKey: {OperationKey}; CorrelationIdd: {CorrelationId}; Retry {RetryNumber} of total retries {TotalRetries};",
            context.OperationKey,
            context.CorrelationId,
            retryAttempt,
            Options.Count);

            return TimeSpan.FromSeconds(Math.Pow(Options.BackoffPower, retryAttempt));
        }

        private TimeSpan OnDurationAsync(int retryAttempt, DelegateResult<HttpResponseMessage> delegateResult, Context context)
        {
            _logger.LogWarning(
                "[Retry Policy][OnDuration] OperationKey: {OperationKey}; CorrelationIdd: {CorrelationId}; Retry {RetryNumber} of total retries {TotalRetries}; ExceptionMessage: {ExceptionMessage}",
                context.OperationKey,
                context.CorrelationId,
                retryAttempt,
                Options.Count,
                delegateResult.GetMessage());

            return TimeSpan.FromSeconds(Math.Pow(Options.BackoffPower, retryAttempt));
        }

        private Task OnRetryAsync(DelegateResult<HttpResponseMessage> delegateResult, TimeSpan timeElapsed, int retryNumber, Context context)
        {
            _logger.LogInformation(
                "[Retry Policy][OnRetryAsync] OperationKey: {OperationKey}; CorrelationId: {CorrelationId}; TimeElapsed: {TimeElapsed}; Retries: {retryNumber}; ExceptionMessage: {ExceptionMessage}",
                context.OperationKey,
                context.CorrelationId,
                timeElapsed,
                retryNumber,
                delegateResult.GetMessage());

            return Task.CompletedTask;
        }
    }
}
