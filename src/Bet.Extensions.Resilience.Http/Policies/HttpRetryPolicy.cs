﻿using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

using Bet.Extensions.Resilience.Abstractions;
using Bet.Extensions.Resilience.Abstractions.Options;

using Microsoft.Extensions.Logging;

using Polly;

namespace Bet.Extensions.Resilience.Http.Policies
{
    /// <summary>
    /// Default Wait And Retry Polly Policy.
    /// </summary>
    /// <typeparam name="TOptions">The type of the options.</typeparam>
    public class HttpRetryPolicy<TOptions> : BasePolicy<TOptions, HttpResponseMessage> where TOptions : PolicyOptions
    {
        public HttpRetryPolicy(
            string policyName,
            IPolicyConfigurator<TOptions, HttpResponseMessage> policyConfigurator,
            ILogger<IPolicyCreator<TOptions, HttpResponseMessage>> logger) : base(policyName, policyConfigurator, logger)
        {
        }

        /// <inheritdoc/>
        public override IAsyncPolicy<HttpResponseMessage> CreateAsyncPolicy()
        {
            return Policy<HttpResponseMessage>
               .Handle<HttpRequestException>()
               .OrResult(TransientHttpStatusCodePredicate)
                   .WaitAndRetryAsync(
                       retryCount: Options.Retry.Count,
                       sleepDurationProvider: OnDurationAsync,
                       onRetryAsync: OnRetryAsync);
        }

        /// <inheritdoc/>
        public override ISyncPolicy<HttpResponseMessage> CreateSyncPolicy()
        {
            return Policy<HttpResponseMessage>
               .Handle<HttpRequestException>()
               .OrResult(TransientHttpStatusCodePredicate)
                .WaitAndRetry(
                    retryCount: Options.Retry.Count,
                    sleepDurationProvider: (retryAttempt, c) => OnDuration(retryAttempt, c),
                    onRetry: (result, timeSpan, retryAttempt, context) => OnRetry(result, timeSpan, retryAttempt, context));
        }

        private bool TransientHttpStatusCodePredicate(HttpResponseMessage response)
        {
            return (int)response.StatusCode >= 500 || response.StatusCode == HttpStatusCode.RequestTimeout;
        }

        private void OnRetry(DelegateResult<HttpResponseMessage> delegateResult, TimeSpan timeElapsed, int retryAttempt, Context context)
        {
            Logger.LogInformation(
                "[Retry Policy][OnRetryAsync] OperationKey: {OperationKey}; CorrelationId: {CorrelationId}; TimeElapsed: {TimeElapsed}; Retries: {retryNumber}; ExceptionMessage: {ExceptionMessage}",
                context.OperationKey,
                context.CorrelationId,
                timeElapsed,
                retryAttempt,
                delegateResult.GetMessage());
        }

        private TimeSpan OnDuration(int retryAttempt, Context context)
        {
            Logger.LogWarning(
            "[Retry Policy][OnDuration] OperationKey: {OperationKey}; CorrelationIdd: {CorrelationId}; Retry {RetryNumber} of total retries {TotalRetries};",
            context.OperationKey,
            context.CorrelationId,
            retryAttempt,
            Options.Retry.Count);

            return TimeSpan.FromSeconds(Math.Pow(Options.Retry.BackoffPower, retryAttempt));
        }

        private TimeSpan OnDurationAsync(int retryAttempt, DelegateResult<HttpResponseMessage> delegateResult, Context context)
        {
            Logger.LogWarning(
                "[Retry Policy][OnDuration] OperationKey: {OperationKey}; CorrelationIdd: {CorrelationId}; Retry {RetryNumber} of total retries {TotalRetries}; ExceptionMessage: {ExceptionMessage}",
                context.OperationKey,
                context.CorrelationId,
                retryAttempt,
                Options.Retry.Count,
                delegateResult.GetMessage());

            return TimeSpan.FromSeconds(Math.Pow(Options.Retry.BackoffPower, retryAttempt));
        }

        private Task OnRetryAsync(DelegateResult<HttpResponseMessage> delegateResult, TimeSpan timeElapsed, int retryNumber, Context context)
        {
            Logger.LogInformation(
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
