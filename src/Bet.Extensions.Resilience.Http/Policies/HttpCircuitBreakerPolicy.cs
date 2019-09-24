using System;
using System.Net.Http;

using Bet.Extensions.Resilience.Http.Options;

using Microsoft.Extensions.Logging;

using Polly;
using Polly.Extensions.Http;

namespace Bet.Extensions.Resilience.Http.Policies
{
    /// <summary>
    /// The default circuit breaker policy.
    /// </summary>
    /// <typeparam name="TOptions"></typeparam>
    public class HttpCircuitBreakerPolicy<TOptions> : IHttpPolicy<TOptions> where TOptions : HttpPolicyOptions
    {
        private readonly IHttpPolicyConfigurator<TOptions> _policyBuilder;
        private readonly ILogger<HttpCircuitBreakerPolicy<TOptions>> _logger;
        private readonly TOptions _options;

        public HttpCircuitBreakerPolicy(
            string policyName,
            IHttpPolicyConfigurator<TOptions> policyBuilder,
            ILogger<HttpCircuitBreakerPolicy<TOptions>> logger)
        {
            Name = policyName;

            _policyBuilder = policyBuilder ?? throw new ArgumentNullException(nameof(policyBuilder));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _options = policyBuilder.GetOptions(policyName);
        }

        public virtual string Name { get; }

        public virtual IAsyncPolicy<HttpResponseMessage> CreateAsyncPolicy()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .CircuitBreakerAsync(
                    handledEventsAllowedBeforeBreaking: _options.HttpCircuitBreaker.ExceptionsAllowedBeforeBreaking,
                    durationOfBreak: _options.HttpCircuitBreaker.DurationOfBreak,
                    OnBreak,
                    OnReset);
        }

        public virtual ISyncPolicy<HttpResponseMessage> CreateSyncPolicy()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .CircuitBreaker(
                    handledEventsAllowedBeforeBreaking: _options.HttpCircuitBreaker.ExceptionsAllowedBeforeBreaking,
                    durationOfBreak: _options.HttpCircuitBreaker.DurationOfBreak,
                    OnBreak,
                    OnReset);
        }

        public virtual void RegisterPolicy()
        {
            _policyBuilder.AddPolicy($"{Name}Async", CreateAsyncPolicy, true);
            _policyBuilder.AddPolicy($"{Name}", CreateSyncPolicy, true);
        }

        private void OnBreak(DelegateResult<HttpResponseMessage> delegateResult, TimeSpan breakSpan, Context context)
        {
            _logger.LogWarning(
                "[CircuitBreak Policy][OnBreak] OperationKey: {OperationKey}; CorrelationId: {CorrelationId}; Duration of the break: {DurationOfBreak}; Reason:{ExceptionMessage}",
                context.OperationKey,
                context.CorrelationId,
                breakSpan,
                delegateResult.GetMessage());
        }

        private void OnReset(Context context)
        {
            _logger.LogInformation(
                "[CircuitBreak Policy][OnReset] OperationKey: {operationKey}; CorrelationId: {CorrelationId}",
                context.OperationKey,
                context.CorrelationId);
        }
    }
}
