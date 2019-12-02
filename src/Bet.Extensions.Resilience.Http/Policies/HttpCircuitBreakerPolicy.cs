using System.Net.Http;

using Bet.Extensions.Resilience.Abstractions;
using Bet.Extensions.Resilience.Abstractions.Options;
using Bet.Extensions.Resilience.Abstractions.Policies;
using Bet.Extensions.Resilience.Http.Options;

using Microsoft.Extensions.Logging;

using Polly;
using Polly.Extensions.Http;

namespace Bet.Extensions.Resilience.Http.Policies
{
    /// <summary>
    /// The default http circuit breaker policy.
    /// </summary>
    /// <typeparam name="TOptions"></typeparam>
    public class HttpCircuitBreakerPolicy :
        CircuitBreakerPolicy<HttpCircuitBreakerPolicyOptions, HttpResponseMessage>,
        IHttpCircuitBreakerPolicy
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HttpCircuitBreakerPolicy"/> class.
        /// </summary>
        /// <param name="policyOptions"></param>
        /// <param name="policyOptionsConfigurator"></param>
        /// <param name="registryConfigurator"></param>
        /// <param name="logger"></param>
        public HttpCircuitBreakerPolicy(
            PolicyOptions policyOptions,
            IPolicyOptionsConfigurator<HttpCircuitBreakerPolicyOptions> policyOptionsConfigurator,
            IPolicyRegistryConfigurator registryConfigurator,
            ILogger<IPolicy<HttpCircuitBreakerPolicyOptions, HttpResponseMessage>> logger) : base(policyOptions, policyOptionsConfigurator, registryConfigurator, logger)
        {
            OnBreak = (logger, options) =>
            {
                return (outcome, state, time, context) =>
                {
                    logger.LogCircuitBreakerOnBreak(time, context, state, options, outcome.GetMessage());
                };
            };
        }

        /// <inheritdoc/>
        public override IAsyncPolicy<HttpResponseMessage> GetAsyncPolicy()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .CircuitBreakerAsync(
                    handledEventsAllowedBeforeBreaking: Options.ExceptionsAllowedBeforeBreaking,
                    durationOfBreak: Options.DurationOfBreak,
                    OnBreak(Logger, Options),
                    OnReset(Logger, Options),
                    OnHalfOpen(Logger, Options))
                   .WithPolicyKey($"{PolicyOptions.Name}{PolicyNameSuffix}");
        }

        /// <inheritdoc/>
        public override ISyncPolicy<HttpResponseMessage> GetSyncPolicy()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .CircuitBreaker(
                    handledEventsAllowedBeforeBreaking: Options.ExceptionsAllowedBeforeBreaking,
                    durationOfBreak: Options.DurationOfBreak,
                    OnBreak(Logger, Options),
                    OnReset(Logger, Options),
                    OnHalfOpen(Logger, Options))
                    .WithPolicyKey(PolicyOptions.Name);
        }
    }
}
