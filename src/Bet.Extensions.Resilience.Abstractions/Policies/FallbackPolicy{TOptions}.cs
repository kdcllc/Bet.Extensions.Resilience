using System;
using System.Threading;
using System.Threading.Tasks;

using Bet.Extensions.Resilience.Abstractions.Options;

using Microsoft.Extensions.Logging;

using Polly;
using Polly.Bulkhead;
using Polly.CircuitBreaker;
using Polly.Timeout;

namespace Bet.Extensions.Resilience.Abstractions.Policies
{
    public class FallbackPolicy<TOptions> :
        BasePolicy<TOptions>,
        IFallbackPolicy<TOptions> where TOptions : FallbackPolicyOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FallbackPolicy{TOptions}"/> class.
        /// </summary>
        /// <param name="policyOptions"></param>
        /// <param name="serviceProvider"></param>
        /// <param name="policyOptionsConfigurator"></param>
        /// <param name="registryConfigurator"></param>
        /// <param name="logger"></param>
        public FallbackPolicy(
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

        public Func<ILogger<IPolicy<TOptions>>, TOptions, Action<Exception, Context, CancellationToken>> FallBackAction { get; set; } = (logger, options) =>
        {
            return (ex, context, token) =>
            {
                logger.LogOnFallabck(context, ex.GetExceptionMessages());
            };
        };

        public Func<ILogger<IPolicy<TOptions>>, TOptions, Action<Exception, Context>> OnFallback { get; set; } = (logger, options) =>
        {
            return (ex, context) =>
            {
                logger.LogOnFallabck(context, ex.GetExceptionMessages());
            };
        };

        public Func<ILogger<IPolicy<TOptions>>, TOptions, Func<Exception, Context, CancellationToken, Task>> FallBackActionAsync { get; set; } = (logger, options) =>
        {
            return (ex, context, token) =>
            {
                logger.LogOnFallabck(context, ex.GetExceptionMessages());
                return Task.CompletedTask;
            };
        };

        public Func<ILogger<IPolicy<TOptions>>, TOptions, Func<Exception, Context, Task>> OnFallbackAsync { get; set; } = (logger, options) =>
        {
            return (ex, context) =>
            {
                logger.LogOnFallabck(context, ex.GetExceptionMessages());
                return Task.CompletedTask;
            };
        };

        public override IAsyncPolicy GetAsyncPolicy()
        {
            if (FallBackActionAsync == null
                || OnFallbackAsync == null)
            {
                throw new InvalidOperationException($"Please configure {nameof(FallBackActionAsync)} and {nameof(OnFallbackAsync)} properties");
            }

            return Policy

                // Polly timeout policy exception
                .Handle<TimeoutRejectedException>()

                // Polly Broken Circuit
                .Or<BrokenCircuitException>()

                .Or<TimeoutRejectedException>()

                // Message Handler timeout
                .Or<TimeoutException>()

                // Client canceled
                .Or<TaskCanceledException>()

                // failed bulkhead policy
                .Or<BulkheadRejectedException>()
                .FallbackAsync(fallbackAction: FallBackActionAsync(Logger, Options), onFallbackAsync: OnFallbackAsync(Logger, Options))
                .WithPolicyKey($"{PolicyOptions.Name}{PolicyNameSuffix}");
        }

        public override ISyncPolicy GetSyncPolicy()
        {
            if (FallBackAction == null
                || OnFallback == null)
            {
                throw new InvalidOperationException($"Please configure {nameof(FallBackAction)} and {nameof(OnFallback)} properties");
            }

            return Policy

                // Polly timeout policy exception
                .Handle<TimeoutRejectedException>()

                // Polly Broken Circuit
                .Or<BrokenCircuitException>()

                .Or<TimeoutRejectedException>()

                // Message Handler timeout
                .Or<TimeoutException>()

                // Client canceled
                .Or<TaskCanceledException>()

                // failed bulkhead policy
                .Or<BulkheadRejectedException>()

                .Fallback(fallbackAction: FallBackAction(Logger, Options), onFallback: OnFallback(Logger, Options))

                .WithPolicyKey(PolicyOptions.Name);
        }
    }
}
