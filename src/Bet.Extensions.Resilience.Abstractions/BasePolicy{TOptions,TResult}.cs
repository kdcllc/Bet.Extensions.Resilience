using System;

using Bet.Extensions.Resilience.Abstractions.Options;

using Microsoft.Extensions.Logging;

using Polly;

namespace Bet.Extensions.Resilience.Abstractions
{
    /// <inheritdoc/>
    public abstract class BasePolicy<TOptions, TResult> : IPolicyCreator<TOptions, TResult> where TOptions : PolicyOptions
    {
        private readonly IPolicyConfigurator<TOptions, TResult> _policyConfigurator;

        protected BasePolicy(
            string policyName,
            IPolicyConfigurator<TOptions, TResult> policyConfigurator,
            ILogger<IPolicyCreator<TOptions, TResult>> logger)
        {
            Name = policyName;

            _policyConfigurator = policyConfigurator ?? throw new ArgumentNullException(nameof(policyConfigurator));
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));

            Options = policyConfigurator.GetOptions(policyName);
        }

        /// <inheritdoc/>
        public virtual string Name { get; }

        /// <inheritdoc/>
        public virtual TOptions Options { get; }

        protected ILogger<IPolicyCreator<TOptions, TResult>> Logger { get; }

        /// <inheritdoc/>
        public abstract IAsyncPolicy<TResult> CreateAsyncPolicy();

        /// <inheritdoc/>
        public abstract ISyncPolicy<TResult> CreateSyncPolicy();

        /// <inheritdoc/>
        public virtual void RegisterPolicy()
        {
            var syncPolicyName = Name;
            var asyncPolicyName = $"{Name}Async";

            _policyConfigurator.AddPolicy(asyncPolicyName, CreateAsyncPolicy, true);
            _policyConfigurator.AddPolicy(syncPolicyName, CreateSyncPolicy, true);

            if (Logger.IsEnabled(LogLevel.Debug))
            {
                Logger.LogDebug("[Add][Polly Policy] - {policyName}", syncPolicyName);
                Logger.LogDebug("[Add][Polly Policy] - {policyName}", asyncPolicyName);
            }
        }
    }
}
