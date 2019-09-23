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
    public class CircuitBreakerPolicy : IHttpPolicyRegistration<HttpPolicyOptions>
    {
        private readonly IResilienceHttpPolicyBuilder<HttpPolicyOptions> _policyBuilder;
        private readonly ILogger<CircuitBreakerPolicy> _logger;
        private readonly HttpPolicyOptions _options;
        private readonly string _policyName;

        public CircuitBreakerPolicy(
            string policyName,
            IResilienceHttpPolicyBuilder<HttpPolicyOptions> policyBuilder,
            ILogger<CircuitBreakerPolicy> logger)
        {
            _policyBuilder = policyBuilder ?? throw new ArgumentNullException(nameof(policyBuilder));
            _logger = logger;

            _options = policyBuilder.GetOptions(policyName);
            _policyName = policyName;
        }

        public IAsyncPolicy<HttpResponseMessage> CreateAsyncPolicy()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .CircuitBreakerAsync(
                    handledEventsAllowedBeforeBreaking: _options.HttpCircuitBreaker.ExceptionsAllowedBeforeBreaking,
                    durationOfBreak: _options.HttpCircuitBreaker.DurationOfBreak,
                    OnBreak,
                    OnReset);
        }

        public ISyncPolicy<HttpResponseMessage> CreateSyncPolicy()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .CircuitBreaker(
                    handledEventsAllowedBeforeBreaking: _options.HttpCircuitBreaker.ExceptionsAllowedBeforeBreaking,
                    durationOfBreak: _options.HttpCircuitBreaker.DurationOfBreak,
                    OnBreak,
                    OnReset);
        }

        public void RegisterPolicy()
        {
            _policyBuilder.AddPolicy($"{_policyName}Async", CreateAsyncPolicy);
            _policyBuilder.AddPolicy($"{_policyName}", CreateAsyncPolicy);
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
