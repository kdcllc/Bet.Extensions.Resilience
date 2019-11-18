using System;
using System.Net.Http;
using Bet.Extensions.Resilience.Abstractions;
using Bet.Extensions.Resilience.Abstractions.Options;

using Microsoft.Extensions.Logging;

using Polly;
using Polly.Extensions.Http;

namespace Bet.Extensions.Resilience.Http.Policies
{
    /// <summary>
    /// The default circuit breaker policy.
    /// </summary>
    /// <typeparam name="TOptions"></typeparam>
    public class HttpCircuitBreakerPolicy<TOptions> : BasePolicy<HttpResponseMessage, TOptions> where TOptions : PolicyOptions
    {
        public HttpCircuitBreakerPolicy(
            string policyName,
            IPolicyConfigurator<HttpResponseMessage, TOptions> policyConfigurator,
            ILogger<IPolicyCreator<HttpResponseMessage, TOptions>> logger) : base(policyName, policyConfigurator, logger)
        {
        }

        /// <inheritdoc/>
        public override IAsyncPolicy<HttpResponseMessage> CreateAsyncPolicy()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .CircuitBreakerAsync(
                    handledEventsAllowedBeforeBreaking: Options.CircuitBreaker.ExceptionsAllowedBeforeBreaking,
                    durationOfBreak: Options.CircuitBreaker.DurationOfBreak,
                    OnBreak,
                    OnReset);
        }

        /// <inheritdoc/>
        public override ISyncPolicy<HttpResponseMessage> CreateSyncPolicy()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .CircuitBreaker(
                    handledEventsAllowedBeforeBreaking: Options.CircuitBreaker.ExceptionsAllowedBeforeBreaking,
                    durationOfBreak: Options.CircuitBreaker.DurationOfBreak,
                    OnBreak,
                    OnReset);
        }

        private void OnBreak(DelegateResult<HttpResponseMessage> delegateResult, TimeSpan breakSpan, Context context)
        {
            Logger.LogWarning(
                "[CircuitBreak Policy][OnBreak] OperationKey: {OperationKey}; CorrelationId: {CorrelationId}; Duration of the break: {DurationOfBreak}; Reason:{ExceptionMessage}",
                context.OperationKey,
                context.CorrelationId,
                breakSpan,
                delegateResult.GetMessage());
        }

        private void OnReset(Context context)
        {
            Logger.LogInformation(
                "[CircuitBreak Policy][OnReset] OperationKey: {operationKey}; CorrelationId: {CorrelationId}",
                context.OperationKey,
                context.CorrelationId);
        }
    }
}
