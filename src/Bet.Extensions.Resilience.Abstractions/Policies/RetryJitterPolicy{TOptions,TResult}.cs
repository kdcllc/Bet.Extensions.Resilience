using System;
using System.Threading.Tasks;

using Bet.Extensions.Resilience.Abstractions.Options;

using Microsoft.Extensions.Logging;

using Polly;

namespace Bet.Extensions.Resilience.Abstractions.Policies
{
    public class RetryJitterPolicy<TOptions, TResult> :
                        BasePolicy<TOptions, TResult>,
                        IRetryJitterPolicy<TOptions, TResult> where TOptions : RetryJitterPolicyOptions
    {
        public RetryJitterPolicy(
            PolicyOptions policyOptions,
            IPolicyOptionsConfigurator<TOptions> policyOptionsConfigurator,
            IPolicyRegistryConfigurator registryConfigurator,
            ILogger<IPolicy<TOptions, TResult>> logger) : base(policyOptions, policyOptionsConfigurator, registryConfigurator, logger)
        {
        }

        public Func<ILogger<IPolicy<TOptions, TResult>>, TOptions, Func<DelegateResult<TResult>, TimeSpan, int, Context, Task>> OnRetryAsync { get; set; } = (logger, options) =>
        {
            return (outcome, time, attempt, context) =>
            {
                logger.LogRetryOnRetry(time, attempt, context, options.MaxRetries, outcome.GetExceptionMessages());
                return Task.CompletedTask;
            };
        };

        public Func<ILogger<IPolicy<TOptions, TResult>>, TOptions, Action<DelegateResult<TResult>, TimeSpan, int, Context>> OnRetry { get; set; } = (logger, options) =>
        {
            return (outcome, time, attempt, context) => logger.LogRetryOnRetry(time, attempt, context, options.MaxRetries, outcome.GetExceptionMessages());
        };

        public override IAsyncPolicy<TResult> GetAsyncPolicy()
        {
            if (OnRetryAsync == null)
            {
                throw new InvalidOperationException($"Please configure {nameof(OnRetryAsync)} property");
            }

            var delay = Backoff.DecorrelatedJitter(Options.MaxRetries, Options.SeedDelay, Options.MaxDelay);
            return Policy<TResult>
                .Handle<Exception>()
                .WaitAndRetryAsync(
                delay,
                OnRetryAsync(Logger, Options))
                .WithPolicyKey($"{PolicyOptions.Name}{PolicyNameSuffix}");
        }

        public override ISyncPolicy<TResult> GetSyncPolicy()
        {
            if (OnRetry == null)
            {
                throw new InvalidOperationException($"Please configure {nameof(OnRetry)} property");
            }

            var delay = Backoff.DecorrelatedJitter(Options.MaxRetries, Options.SeedDelay, Options.MaxDelay);

            return Policy<TResult>
               .Handle<Exception>()
               .WaitAndRetry(
               delay,
               OnRetry(Logger, Options))
               .WithPolicyKey(PolicyOptions.Name);
        }
    }
}
