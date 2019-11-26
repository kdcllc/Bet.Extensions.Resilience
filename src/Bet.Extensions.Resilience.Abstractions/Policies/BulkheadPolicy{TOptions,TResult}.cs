using System;
using System.Threading.Tasks;

using Bet.Extensions.Resilience.Abstractions.Options;

using Microsoft.Extensions.Logging;

using Polly;

namespace Bet.Extensions.Resilience.Abstractions.Policies
{
    /// <summary>
    /// The <see cref="Polly.Bulkhead.BulkheadPolicy"/> wrapper.
    /// </summary>
    /// <typeparam name="TOptions"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    public class BulkheadPolicy<TOptions, TResult> :
        BasePolicy<TOptions, TResult>,
        IBulkheadPolicy<TOptions, TResult>
        where TOptions : BulkheadPolicyOptions
    {
        private readonly ILogger<IPolicy<TOptions>> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="BulkheadPolicy{TOptions, TResult}"/> class.
        /// </summary>
        /// <param name="policyOptions"></param>
        /// <param name="policyOptionsConfigurator"></param>
        /// <param name="registryConfigurator"></param>
        /// <param name="logger"></param>
        public BulkheadPolicy(
            PolicyOptions policyOptions,
            IPolicyOptionsConfigurator<TOptions> policyOptionsConfigurator,
            IPolicyRegistryConfigurator registryConfigurator,
            ILogger<IPolicy<TOptions>> logger) : base(policyOptions, policyOptionsConfigurator, registryConfigurator, logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public override IAsyncPolicy<TResult> GetAsyncPolicy()
        {
            return Policy.BulkheadAsync<TResult>(
              Options.MaxParallelization,
              Options.MaxQueuedItems,
              ctx =>
              {
                  _logger.LogOnBulkheadRejected(ctx);
                  return Task.CompletedTask;
              })
              .WithPolicyKey(PolicyOptions.Name);
        }

        public override ISyncPolicy<TResult> GetSyncPolicy()
        {
            return Policy.Bulkhead<TResult>(
                Options.MaxParallelization,
                Options.MaxQueuedItems,
                _logger.LogOnBulkheadRejected)
                .WithPolicyKey(PolicyOptions.Name);
        }
    }
}
