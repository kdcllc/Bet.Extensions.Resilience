using System;

using Bet.Extensions.Resilience.Abstractions.Options;

using Microsoft.Extensions.Logging;

using Polly;
using Polly.CircuitBreaker;

namespace Bet.Extensions.Resilience.Abstractions.Policies
{
    public class CircuitBreakerPolicy<TOptions> :
        BasePolicy<TOptions>,
        ICircuitBreakerPolicy<TOptions> where TOptions : CircuitBreakerPolicyOptions
    {
        public CircuitBreakerPolicy(
            PolicyOptions policyOptions,
            IServiceProvider serviceProvider,
            IPolicyOptionsConfigurator<TOptions> policyOptionsConfigurator,
            IPolicyRegistryConfigurator registryConfigurator,
            ILogger<IPolicy<TOptions>> logger) : base(
                policyOptions,
                serviceProvider,
                policyOptionsConfigurator,
                registryConfigurator,
                logger)
        {
        }

        /// <inheritdoc/>
        public Func<ILogger<IPolicy<TOptions>>, TOptions, Action<Exception, CircuitState, TimeSpan, Context>> OnBreak { get; set; } = (logger, options) =>
        {
            return (ex, state, time, context) =>
            {
                logger.LogCircuitBreakerOnBreak(time, context, state, options, ex.GetExceptionMessages());
            };
        };

        /// <inheritdoc/>
        public Func<ILogger<IPolicy<TOptions>>, TOptions, Action<Context>> OnReset { get; set; } = (logger, options) =>
        {
            return (context) =>
            {
                logger.LogCircuitBreakerOnReset(context, options);
            };
        };

        /// <inheritdoc/>
        public Func<ILogger<IPolicy<TOptions>>, TOptions, Action> OnHalfOpen { get; set; } = (logger, options) =>
        {
            return () =>
            {
                logger.LogDebug("[CircuitBreaker Policy][OnHalfOpen]");
            };
        };

        /// <inheritdoc/>
        public override IAsyncPolicy GetAsyncPolicy()
        {
            AssertNulls();

            // TODO consider adding filtering based on configuration of the exception type, or array of types.
            return Policy
                .Handle<Exception>()
                .CircuitBreakerAsync(
                    Options.ExceptionsAllowedBeforeBreaking,
                    Options.DurationOfBreak,
                    OnBreak(Logger, Options),
                    OnReset(Logger, Options),
                    OnHalfOpen(Logger, Options))
                .WithPolicyKey($"{PolicyOptions.Name}{PolicyNameSuffix}");
        }

        /// <inheritdoc/>
        public override ISyncPolicy GetSyncPolicy()
        {
            AssertNulls();

            return Policy
                .Handle<Exception>()
                .CircuitBreaker(
                    Options.ExceptionsAllowedBeforeBreaking,
                    Options.DurationOfBreak,
                    OnBreak(Logger, Options),
                    OnReset(Logger, Options),
                    OnHalfOpen(Logger, Options))
                .WithPolicyKey(PolicyOptions.Name);
        }

        private void AssertNulls()
        {
            if (OnBreak == null
                || OnReset == null
                || OnHalfOpen == null)
            {
                throw new InvalidOperationException($"Please configure the properties: {nameof(OnBreak)}, {nameof(OnReset)}, {nameof(OnHalfOpen)}. ");
            }
        }
    }
}
