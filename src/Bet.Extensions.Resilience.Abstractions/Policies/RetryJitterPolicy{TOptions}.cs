﻿using System;
using System.Threading.Tasks;

using Bet.Extensions.Resilience.Abstractions.Options;

using Microsoft.Extensions.Logging;

using Polly;

namespace Bet.Extensions.Resilience.Abstractions.Policies
{
    //public class RetryJitterPolicy :
    //            BasePolicy<RetryJitterPolicyOptions>,
    //            IRetryJitterPolicy<RetryJitterPolicyOptions>
    //{
    //    public RetryJitterPolicy(
    //        PolicyOptions policyOptions,
    //        IPolicyOptionsConfigurator<RetryJitterPolicyOptions> policyOptionsConfigurator,
    //        IPolicyRegistryConfigurator registryConfigurator,
    //        ILogger<IPolicy<RetryJitterPolicyOptions>> logger) : base(policyOptions, policyOptionsConfigurator, registryConfigurator, logger)
    //    {
    //    }

    //    public Func<ILogger<IPolicy<TOptions>>, TOptions, Func<int, Exception, Context, TimeSpan>> OnDuration { get; set; } = (logger, options) =>
    //    {
    //        return (attempt, outcome, context) =>
    //        {

    //            return TimeSpan.FromSeconds(Math.Pow(options.BackoffPower, attempt));
    //        };
    //    };

    //    public Func<ILogger<IPolicy<TOptions>>, TOptions, Func<Exception, TimeSpan, int, Context, Task>> OnRetryAsync { get; set; } = (logger, options) =>
    //    {
    //        return (outcome, time, attempt, context) =>
    //        {

    //            return Task.CompletedTask;
    //        };
    //    };

    //    public Func<ILogger<IPolicy<TOptions>>, TOptions, Action<Exception, TimeSpan, int, Context>> OnRetry { get; set; } = (logger, options) =>
    //    {
    //        return (outcome, time, attempt, context) => { };
    //    };

    //    public override IAsyncPolicy GetAsyncPolicy()
    //    {
    //        if (OnDuration == null
    //          || OnRetryAsync == null)
    //        {
    //            throw new InvalidOperationException($"Please configure {nameof(OnDuration)} and {nameof(OnRetryAsync)} properties");
    //        }

    //        var delay = Backoff.DecorrelatedJitter(Options.MaxRetries, TimeSpan.FromMilliseconds(100), Options.MaxDelay);
    //        return Policy
    //            .Handle<Exception>()
    //            .WaitAndRetryAsync(
    //            delay,
    //            OnDuration(Logger, Options),
    //            OnRetryAsync(Logger, Options))
    //            .WithPolicyKey($"{PolicyOptions.Name}{PolicyNameSuffix}");
    //    }

    //    public override ISyncPolicy GetSyncPolicy()
    //    {
    //        if (OnDuration == null
    //           || OnRetryAsync == null)
    //        {
    //            throw new InvalidOperationException($"Please configure {nameof(OnDuration)} and {nameof(OnRetry)} properties");
    //        }

    //        return Policy
    //           .Handle<Exception>()
    //           .WaitAndRetry(
    //           Options.Count,
    //           OnDuration(Logger, Options),
    //           OnRetry(Logger, Options))
    //           .WithPolicyKey(PolicyOptions.Name);
    //    }
    //}
}
