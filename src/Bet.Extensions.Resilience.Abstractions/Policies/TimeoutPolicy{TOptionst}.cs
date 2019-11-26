﻿using System;
using System.Threading.Tasks;

using Bet.Extensions.Resilience.Abstractions.Options;

using Microsoft.Extensions.Logging;

using Polly;
using Polly.Timeout;

namespace Bet.Extensions.Resilience.Abstractions.Policies
{
    public class TimeoutPolicy<TOptions> :
                BasePolicy<TOptions>,
                ITimeoutPolicy<TOptions> where TOptions : TimeoutPolicyOptions
    {
        private readonly ILogger<IPolicy<TOptions>> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="TimeoutPolicy{TOptions}"/> class.
        /// </summary>
        /// <param name="policyOptions"></param>
        /// <param name="policyOptionsConfigurator"></param>
        /// <param name="registryConfigurator"></param>
        /// <param name="logger"></param>
        public TimeoutPolicy(
            PolicyOptions policyOptions,
            IPolicyOptionsConfigurator<TOptions> policyOptionsConfigurator,
            IPolicyRegistryConfigurator registryConfigurator,
            ILogger<IPolicy<TOptions>> logger) : base(policyOptions, policyOptionsConfigurator, registryConfigurator, logger)
        {
            _logger = logger;
        }

        /// <inheritdoc/>
        public override IAsyncPolicy GetAsyncPolicy()
        {
            return Policy
                .TimeoutAsync(Options.Timeout, TimeoutStrategy.Pessimistic, OnTimeoutAsync)
                .WithPolicyKey($"{PolicyOptions.Name}{PolicyNameSuffix}");
        }

        /// <inheritdoc/>
        public override ISyncPolicy GetSyncPolicy()
        {
            return Policy
                .Timeout(Options.Timeout, TimeoutStrategy.Pessimistic, OnTimeout)
                .WithPolicyKey(PolicyOptions.Name);
        }

        public void OnTimeout(Context context, TimeSpan timeout, Task abandonedTask, Exception ex)
        {
            _logger.LogOnTimeout(context, timeout, ex);
        }

        public virtual Task OnTimeoutAsync(Context context, TimeSpan timeout, Task abandonedTask, Exception ex)
        {
            _logger.LogOnTimeout(context, timeout, ex);
            return Task.CompletedTask;
        }
    }
}
