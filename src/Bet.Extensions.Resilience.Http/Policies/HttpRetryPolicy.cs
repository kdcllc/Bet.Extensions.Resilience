using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

using Bet.Extensions.Resilience.Http.Options;

using Microsoft.Extensions.Logging;

using Polly;

namespace Bet.Extensions.Resilience.Http.Policies
{
    public class HttpRetryPolicy<TOptions> : IHttpPolicy<TOptions> where TOptions : HttpPolicyOptions
    {
        private readonly IHttpPolicyConfigurator<TOptions> _policyBuilder;
        private readonly ILogger<HttpRetryPolicy<TOptions>> _logger;
        private readonly TOptions _options;

        public HttpRetryPolicy(
            string policyName,
            IHttpPolicyConfigurator<TOptions> policyBuilder,
            ILogger<HttpRetryPolicy<TOptions>> logger)
        {
            Name = policyName;
            _policyBuilder = policyBuilder ?? throw new ArgumentNullException(nameof(policyBuilder));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _options = _policyBuilder.GetOptions(policyName);
        }

        public virtual string Name { get; }

        public virtual IAsyncPolicy<HttpResponseMessage> CreateAsyncPolicy()
        {
            return Policy<HttpResponseMessage>
               .Handle<HttpRequestException>()
               .OrResult(TransientHttpStatusCodePredicate)
                   .WaitAndRetryAsync(
                       retryCount: _options.HttpRetry.Count,
                       sleepDurationProvider: OnDurationAsync,
                       onRetryAsync: OnRetryAsync);
        }

        public virtual ISyncPolicy<HttpResponseMessage> CreateSyncPolicy()
        {
            return Policy<HttpResponseMessage>
               .Handle<HttpRequestException>()
               .OrResult(TransientHttpStatusCodePredicate)
                .WaitAndRetry(
                    retryCount: _options.HttpRetry.Count,
                    sleepDurationProvider: (retryAttempt, c) => OnDuration(retryAttempt, c),
                    onRetry: (result, timeSpan, retryAttempt, context) => OnRetry(result, timeSpan, retryAttempt, context));
        }

        public virtual void RegisterPolicy()
        {
            _policyBuilder.AddPolicy($"{Name}Async", CreateAsyncPolicy, true);
            _policyBuilder.AddPolicy($"{Name}", CreateSyncPolicy, true);
        }

        private bool TransientHttpStatusCodePredicate(HttpResponseMessage response)
        {
            return (int)response.StatusCode >= 500 || response.StatusCode == HttpStatusCode.RequestTimeout;
        }

        private void OnRetry(DelegateResult<HttpResponseMessage> result, TimeSpan timeSpan, int retryAttempt, Context context)
        {
            throw new NotImplementedException();
        }

        private TimeSpan OnDuration(int retryAttempt, Context context)
        {
            _logger.LogWarning(
            "[Retry Policy][OnDuration] OperationKey: {OperationKey}; CorrelationIdd: {CorrelationId}; Retry {RetryNumber} of total retries {TotalRetries};",
            context.OperationKey,
            context.CorrelationId,
            retryAttempt,
            _options.HttpRetry.Count);

            return TimeSpan.FromSeconds(Math.Pow(_options.HttpRetry.BackoffPower, retryAttempt));
        }

        private TimeSpan OnDurationAsync(int retryAttempt, DelegateResult<HttpResponseMessage> delegateResult, Context context)
        {
            _logger.LogWarning(
                "[Retry Policy][OnDuration] OperationKey: {OperationKey}; CorrelationIdd: {CorrelationId}; Retry {RetryNumber} of total retries {TotalRetries}; ExceptionMessage: {ExceptionMessage}",
                context.OperationKey,
                context.CorrelationId,
                retryAttempt,
                _options.HttpRetry.Count,
                delegateResult.GetMessage());

            return TimeSpan.FromSeconds(Math.Pow(_options.HttpRetry.BackoffPower, retryAttempt));
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
