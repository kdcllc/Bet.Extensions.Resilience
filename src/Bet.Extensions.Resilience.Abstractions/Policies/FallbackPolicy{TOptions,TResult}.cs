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
        private readonly ILogger<IPolicy<TOptions>> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="FallbackPolicy{TOptions, TResult}"/> class.
        /// </summary>
        /// <param name="policyOptions"></param>
        /// <param name="policyOptionsConfigurator"></param>
        /// <param name="registryConfigurator"></param>
        /// <param name="logger"></param>
        public FallbackPolicy(
            PolicyOptions policyOptions,
            IPolicyOptionsConfigurator<TOptions> policyOptionsConfigurator,
            IPolicyRegistryConfigurator registryConfigurator,
            ILogger<IPolicy<TOptions>> logger) : base(policyOptions, policyOptionsConfigurator, registryConfigurator, logger)
        {
            _logger = logger;
        }

        public Func<ILogger<IPolicy<TOptions>>, TOptions, Func<DelegateResult<TResult>, Context, CancellationToken, Task<TResult>>> FallbackActionAsync { get; set; }

        public Func<ILogger<IPolicy<TOptions>>, TOptions, Func<DelegateResult<TResult>, Context, Task>> OnFallbackAsync { get; set; }

        public Func<ILogger<IPolicy<TOptions>>, TOptions, Func<DelegateResult<TResult>, Context, CancellationToken, TResult>> FallbackAction { get; set; }

        public Func<ILogger<IPolicy<TOptions>>, TOptions, Action<DelegateResult<TResult>, Context>> OnFallback { get; set; }

        public override IAsyncPolicy<TResult> GetAsyncPolicy()
        {
            if (FallbackActionAsync == null
                || OnFallbackAsync == null)
            {
                throw new InvalidOperationException($"Please configure {nameof(FallbackActionAsync)} and {nameof(OnFallbackAsync)} properties");
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

                .FallbackAsync(FallbackActionAsync(_logger, Options), OnFallbackAsync(_logger, Options))

                .WithPolicyKey($"{PolicyOptions.Name}{PolicyNameSuffix}");
        }

        public override ISyncPolicy<TResult> GetSyncPolicy()
        {
            if (FallbackAction == null
                || OnFallback == null)
            {
                throw new InvalidOperationException($"Please configure {nameof(FallbackAction)} and {nameof(OnFallback)} properties");
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

                .Fallback(fallbackAction: FallbackAction(_logger, Options), onFallback: OnFallback(_logger, Options))

                .WithPolicyKey(PolicyOptions.Name);
        }
    }
}
