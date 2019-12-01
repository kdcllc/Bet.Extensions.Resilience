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
        public BulkheadPolicy(
            PolicyOptions policyOptions,
            IPolicyOptionsConfigurator<TOptions> policyOptionsConfigurator,
            IPolicyRegistryConfigurator registryConfigurator,
            ILogger<IPolicy<TOptions>> logger) : base(policyOptions, policyOptionsConfigurator, registryConfigurator, logger)
        {
        }

        public Func<ILogger<IPolicy<TOptions>>, TOptions, Action<Context>> OnBulkheadRejected { get; set; } = (logger, options) =>
        {
            return (context) =>
            {
                logger.LogOnBulkheadRejected(context, options);
            };
        };

        public Func<ILogger<IPolicy<TOptions>>, TOptions, Func<Context, Task>> OnBulkheadRejectedAsync { get; set; } = (logger, options) =>
        {
            return (context) =>
            {
                logger.LogOnBulkheadRejected(context, options);
                return Task.CompletedTask;
            };
        };

        public override IAsyncPolicy GetAsyncPolicy()
        {
            if (OnBulkheadRejectedAsync == null)
            {
                throw new InvalidOperationException($"Please configure {nameof(OnBulkheadRejectedAsync)} property");
            }

            if (Options.MaxQueuedItems.HasValue)
            {
                return Policy
                    .BulkheadAsync(
                        Options.MaxParallelization,
                        Options.MaxQueuedItems.Value,
                        OnBulkheadRejectedAsync(Logger, Options))
                    .WithPolicyKey($"{PolicyOptions.Name}{PolicyNameSuffix}");
            }

            return Policy
                .BulkheadAsync(
                    Options.MaxParallelization,
                    OnBulkheadRejectedAsync(Logger, Options))
                .WithPolicyKey($"{PolicyOptions.Name}{PolicyNameSuffix}");
        }

        public override ISyncPolicy GetSyncPolicy()
        {
            if (OnBulkheadRejected == null)
            {
                throw new InvalidOperationException($"Please configure {nameof(OnBulkheadRejected)} property");
            }

            if (Options.MaxQueuedItems.HasValue)
            {
                return Policy
                    .Bulkhead(
                        Options.MaxParallelization,
                        Options.MaxQueuedItems.Value,
                        OnBulkheadRejected(Logger, Options))
                    .WithPolicyKey(PolicyOptions.Name);
            }

            return Policy.Bulkhead(
                                 Options.MaxParallelization,
                                 OnBulkheadRejected(Logger, Options))
                             .WithPolicyKey(PolicyOptions.Name);
        }
    }
}
