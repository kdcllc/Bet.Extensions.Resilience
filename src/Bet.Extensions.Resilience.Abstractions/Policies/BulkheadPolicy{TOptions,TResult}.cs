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
            ILogger<IPolicy<TOptions, TResult>> logger) : base(policyOptions, policyOptionsConfigurator, registryConfigurator, logger)
        {
        }

        public Func<ILogger<IPolicy<TOptions, TResult>>, TOptions, Action<Context>> OnBulkheadRejected { get; set; } = (logger, options) =>
        {
            return (context) =>
            {
                logger.LogOnBulkheadRejected(context, options);
            };
        };

        public Func<ILogger<IPolicy<TOptions, TResult>>, TOptions, Func<Context, Task>> OnBulkheadRejectedAsync { get; set; } = (logger, options) =>
        {
            return (context) =>
            {
                logger.LogOnBulkheadRejected(context, options);
                return Task.CompletedTask;
            };
        };

        public override IAsyncPolicy<TResult> GetAsyncPolicy()
        {
            if (OnBulkheadRejectedAsync == null)
            {
                throw new InvalidOperationException($"Please configure {nameof(OnBulkheadRejectedAsync)} property");
            }

            if (Options.MaxQueuedItems.HasValue)
            {
                return Policy
                    .BulkheadAsync<TResult>(
                        Options.MaxParallelization,
                        Options.MaxQueuedItems.Value,
                        OnBulkheadRejectedAsync(Logger, Options))
                    .WithPolicyKey($"{PolicyOptions.Name}{PolicyNameSuffix}");
            }

            return Policy
                .BulkheadAsync<TResult>(
                    Options.MaxParallelization,
                    OnBulkheadRejectedAsync(Logger, Options))
                .WithPolicyKey($"{PolicyOptions.Name}{PolicyNameSuffix}");
        }

        public override ISyncPolicy<TResult> GetSyncPolicy()
        {
            if (OnBulkheadRejected == null)
            {
                throw new InvalidOperationException($"Please configure {nameof(OnBulkheadRejected)} property");
            }

            if (Options.MaxQueuedItems.HasValue)
            {
                return Policy
                    .Bulkhead<TResult>(
                        Options.MaxParallelization,
                        Options.MaxQueuedItems.Value,
                        OnBulkheadRejected(Logger, Options))
                    .WithPolicyKey(PolicyOptions.Name);
            }

            return Policy.Bulkhead<TResult>(
                                 Options.MaxParallelization,
                                 OnBulkheadRejected(Logger, Options))
                             .WithPolicyKey(PolicyOptions.Name);
        }
    }
}
