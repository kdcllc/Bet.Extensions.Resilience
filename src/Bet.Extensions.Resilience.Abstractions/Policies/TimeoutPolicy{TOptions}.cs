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
        }

        public Func<ILogger<IPolicy<TOptions>>, TOptions, Func<Context, TimeSpan, Task, Exception, Task>> OnTimeoutAsync { get; set; } = (logger, options) =>
        {
            return (context, timeout, abandonedTask, ex) =>
            {
                logger.LogOnTimeout(context, timeout, ex.GetExceptionMessages());
                return Task.CompletedTask;
            };
        };

        public Func<ILogger<IPolicy<TOptions>>, TOptions, Action<Context, TimeSpan, Task, Exception>> OnTimeout { get; set; } = (logger, options) =>
        {
            return (context, timeout, abandonedTask, ex) =>
            {
                logger.LogOnTimeout(context, timeout, ex.GetExceptionMessages());
            };
        };

        /// <inheritdoc/>
        public override IAsyncPolicy GetAsyncPolicy()
        {
            if (OnTimeoutAsync == null)
            {
                throw new InvalidOperationException($"Please configure {nameof(OnTimeoutAsync)} property");
            }

            return Policy
                .TimeoutAsync(Options.Timeout, TimeoutStrategy.Pessimistic, OnTimeoutAsync(Logger, Options))
                .WithPolicyKey($"{PolicyOptions.Name}{PolicyNameSuffix}");
        }

        /// <inheritdoc/>
        public override ISyncPolicy GetSyncPolicy()
        {
            if (OnTimeout == null)
            {
                throw new InvalidOperationException($"Please configure {nameof(OnTimeout)} property");
            }

            return Policy
                .Timeout(Options.Timeout, TimeoutStrategy.Pessimistic, OnTimeout(Logger, Options))
                .WithPolicyKey(PolicyOptions.Name);
        }
    }
}