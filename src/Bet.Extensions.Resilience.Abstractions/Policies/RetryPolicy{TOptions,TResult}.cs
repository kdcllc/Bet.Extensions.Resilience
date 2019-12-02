using System;
using System.Threading.Tasks;

using Bet.Extensions.Resilience.Abstractions.Options;

using Microsoft.Extensions.Logging;

using Polly;

namespace Bet.Extensions.Resilience.Abstractions.Policies
{
    public class RetryPolicy<TOptions, TResult> :
                BasePolicy<TOptions, TResult>,
                IRetryPolicy<TOptions, TResult> where TOptions : RetryPolicyOptions
    {
        public RetryPolicy(
            PolicyOptions policyOptions,
            IPolicyOptionsConfigurator<TOptions> policyOptionsConfigurator,
            IPolicyRegistryConfigurator registryConfigurator,
            ILogger<IPolicy<TOptions, TResult>> logger) : base(policyOptions, policyOptionsConfigurator, registryConfigurator, logger)
        {
        }

        public Func<ILogger<IPolicy<TOptions, TResult>>, TOptions, Func<int, DelegateResult<TResult>, Context, TimeSpan>> OnDuration { get; set; } = (logger, options) =>
        {
            return (attempt, outcome, context) =>
            {
                logger.LogRetryOnDuration(attempt, context, options, outcome.GetExceptionMessages());
                return TimeSpan.FromSeconds(Math.Pow(options.BackoffPower, attempt));
            };
        };

        public Func<ILogger<IPolicy<TOptions, TResult>>, TOptions, Func<DelegateResult<TResult>, TimeSpan, int, Context, Task>> OnRetryAsync { get; set; } = (logger, options) =>
        {
            return (outcome, time, attempt, context) =>
            {
                logger.LogRetryOnRetry(time, attempt, context, options, outcome.GetExceptionMessages());
                return Task.CompletedTask;
            };
        };

        public Func<ILogger<IPolicy<TOptions, TResult>>, TOptions, Action<DelegateResult<TResult>, TimeSpan, int, Context>> OnRetry { get; set; } = (logger, options) =>
        {
            return (outcome, time, attempt, context) => logger.LogRetryOnRetry(time, attempt, context, options, outcome.GetExceptionMessages());
        };

        public override IAsyncPolicy<TResult> GetAsyncPolicy()
        {
            if (OnDuration == null
                || OnRetryAsync == null)
            {
                throw new InvalidOperationException($"Please configure {nameof(OnDuration)} and {nameof(OnRetryAsync)} properties");
            }

            return Policy<TResult>
                .Handle<Exception>()
                .WaitAndRetryAsync(
                Options.Count,
                OnDuration(Logger, Options),
                OnRetryAsync(Logger, Options))
                .WithPolicyKey($"{PolicyOptions.Name}{PolicyNameSuffix}");
        }

        public override ISyncPolicy<TResult> GetSyncPolicy()
        {
            if (OnDuration == null
                || OnRetryAsync == null)
            {
                throw new InvalidOperationException($"Please configure {nameof(OnDuration)} and {nameof(OnRetry)} properties");
            }

            return Policy<TResult>
               .Handle<Exception>()
               .WaitAndRetry(
               Options.Count,
               OnDuration(Logger, Options),
               OnRetry(Logger, Options))
               .WithPolicyKey(PolicyOptions.Name);
        }
    }
}
