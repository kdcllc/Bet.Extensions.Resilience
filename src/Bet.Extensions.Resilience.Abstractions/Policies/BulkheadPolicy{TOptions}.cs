using System;
using System.Threading.Tasks;

using Bet.Extensions.Resilience.Abstractions.Options;

using Microsoft.Extensions.Logging;

using Polly;

namespace Bet.Extensions.Resilience.Abstractions.Policies
{
    public class BulkheadPolicy<TOptions> :
        BasePolicy<TOptions>,
        IBulkheadPolicy<TOptions>
        where TOptions : BulkheadPolicyOptions
    {
        private readonly ILogger<IPolicy<TOptions>> _logger;

        public BulkheadPolicy(
            PolicyOptions policyOptions,
            IPolicyOptionsConfigurator<TOptions> policyOptionsConfigurator,
            IPolicyRegistryConfigurator registryConfigurator,
            ILogger<IPolicy<TOptions>> logger) : base(policyOptions, policyOptionsConfigurator, registryConfigurator, logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public override IAsyncPolicy GetAsyncPolicy()
        {
            return Policy.BulkheadAsync(
                Options.MaxParallelization,
                Options.MaxQueuedItems,
                ctx =>
                {
                    _logger.LogOnBulkheadRejected(ctx);
                    return Task.CompletedTask;
                })
                .WithPolicyKey(PolicyOptions.Name);
        }

        public override ISyncPolicy GetSyncPolicy()
        {
            return Policy.Bulkhead(
                 Options.MaxParallelization,
                 Options.MaxQueuedItems,
                 _logger.LogOnBulkheadRejected)
                .WithPolicyKey(PolicyOptions.Name);
        }
    }
}
