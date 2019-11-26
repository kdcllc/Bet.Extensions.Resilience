using System;
using System.Net.Http;

using Bet.Extensions.Resilience.Abstractions;
using Bet.Extensions.Resilience.Abstractions.Options;
using Bet.Extensions.Resilience.Abstractions.Policies;

using Microsoft.Extensions.Logging;

using Polly;
using Polly.Extensions.Http;

namespace Bet.Extensions.Resilience.Http.Policies
{
    /// <summary>
    /// The default circuit breaker policy.
    /// </summary>
    /// <typeparam name="TOptions"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    public class HttpCircuitBreakerPolicy<TOptions, TResult> :
        BasePolicy<TOptions, HttpResponseMessage>,
        IHttpCircuitBreakerPolicy<TOptions, HttpResponseMessage>
        where TOptions : CircuitBreakerPolicyOptions
    {
        private readonly ILogger<IPolicy<TOptions>> _logger;

        public HttpCircuitBreakerPolicy(
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
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .CircuitBreakerAsync(
                    handledEventsAllowedBeforeBreaking: Options.ExceptionsAllowedBeforeBreaking,
                    durationOfBreak: Options.DurationOfBreak,
                    OnBreak,
                    OnReset);
        }

        /// <inheritdoc/>
        public override ISyncPolicy<HttpResponseMessage> GetSyncPolicy()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .CircuitBreaker(
                    handledEventsAllowedBeforeBreaking: Options.ExceptionsAllowedBeforeBreaking,
                    durationOfBreak: Options.DurationOfBreak,
                    OnBreak,
                    OnReset);
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
