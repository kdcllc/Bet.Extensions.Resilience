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
    public class FallbackPolicy<TOptions, TResult> :
        BasePolicy<TOptions, TResult>,
        IFallbackPolicy<TOptions, TResult> where TOptions : FallbackPolicyOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FallbackPolicy{TOptions, TResult}"/> class.
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
            ILogger<IPolicy<TOptions, TResult>> logger) : base(
                policyOptions,
                serviceProvider,
                policyOptionsConfigurator,
                registryConfigurator,
                logger)
        {
        }

        public Func<ILogger<IPolicy<TOptions, TResult>>, TOptions, Func<DelegateResult<TResult>, Context, CancellationToken, Task<TResult>>> FallBackActionAsync { get; set; } = (logger, options) =>
        {
            return (outcome, context, token) =>
            {
                logger.LogOnFallabck(context, outcome.GetExceptionMessages());
                return Task.FromResult(outcome.Result);
            };
        };

        public Func<ILogger<IPolicy<TOptions, TResult>>, TOptions, Func<DelegateResult<TResult>, Context, Task>> OnFallbackAsync { get; set; } = (logger, options) => (outcome, context) => Task.CompletedTask;

        public Func<ILogger<IPolicy<TOptions, TResult>>, TOptions, Func<DelegateResult<TResult>, Context, CancellationToken, TResult>> FallBackAction { get; set; } = (logger, options) =>
        {
            return (outcome, context, token) =>
            {
                logger.LogOnFallabck(context, outcome.GetExceptionMessages());
                return outcome.Result;
            };
        };

        public Func<ILogger<IPolicy<TOptions, TResult>>, TOptions, Action<DelegateResult<TResult>, Context>> OnFallback { get; set; } = (logger, options) => (outcome, context) => { };

        public override IAsyncPolicy<TResult> GetAsyncPolicy()
        {
            if (FallBackActionAsync == null
                || OnFallbackAsync == null)
            {
                throw new InvalidOperationException($"Please configure {nameof(FallBackActionAsync)} and {nameof(OnFallbackAsync)} properties");
            }

            return Policy<TResult>

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

                .FallbackAsync(FallBackActionAsync(Logger, Options), OnFallbackAsync(Logger, Options))

                .WithPolicyKey($"{PolicyOptions.Name}{PolicyNameSuffix}");
        }

        public override ISyncPolicy<TResult> GetSyncPolicy()
        {
            if (FallBackAction == null
                || OnFallback == null)
            {
                throw new InvalidOperationException($"Please configure {nameof(FallBackAction)} and {nameof(OnFallback)} properties");
            }

            return Policy<TResult>

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
