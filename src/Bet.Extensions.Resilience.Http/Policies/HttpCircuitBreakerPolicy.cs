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
    public class CircuitBreakerPolicy<TOptions> : IHttpPolicyRegistration<TOptions> where TOptions : HttpPolicyOptions
    {
        private readonly IResilienceHttpPolicyBuilder<TOptions> _policyBuilder;
        private readonly ILogger<CircuitBreakerPolicy<TOptions>> _logger;
        private readonly TOptions _options;

        public CircuitBreakerPolicy(
            string policyName,
            IResilienceHttpPolicyBuilder<TOptions> policyBuilder,
            ILogger<CircuitBreakerPolicy<TOptions>> logger)
        {
            _policyBuilder = policyBuilder ?? throw new ArgumentNullException(nameof(policyBuilder));
            _logger = logger;

            _options = policyBuilder.GetOptions(policyName);
            Name = policyName;
        }

        public string Name { get; private set; }

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
