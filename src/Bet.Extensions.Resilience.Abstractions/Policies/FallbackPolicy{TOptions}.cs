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
        private readonly ILogger<IPolicy<TOptions>> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="FallbackPolicy{TOptions}"/> class.
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

        public Func<ILogger<IPolicy<TOptions>>, TOptions, Action<Exception, Context, CancellationToken>> FallBackAction { get; set; }

        public Func<ILogger<IPolicy<TOptions>>, TOptions, Action<Exception, Context>> OnFallback { get; set; }

        public Func<ILogger<IPolicy<TOptions>>, TOptions, Func<Exception, Context, CancellationToken, Task>> FallbackActionAsync { get; set; }

        public Func<ILogger<IPolicy<TOptions>>, TOptions, Func<Exception, Context, Task>> OnFallbackAsync { get; set; }

        public override IAsyncPolicy GetAsyncPolicy()
        {
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
                .FallbackAsync(fallbackAction: FallbackActionAsync(_logger, Options), onFallbackAsync: OnFallbackAsync(_logger, Options))
                .WithPolicyKey($"{PolicyOptions.Name}{PolicyNameSuffix}");
        }

        public override ISyncPolicy GetSyncPolicy()
        {
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

                .Fallback(fallbackAction: FallBackAction(_logger, Options), onFallback: OnFallback(_logger, Options))

                .WithPolicyKey(PolicyOptions.Name);
        }
    }
}
