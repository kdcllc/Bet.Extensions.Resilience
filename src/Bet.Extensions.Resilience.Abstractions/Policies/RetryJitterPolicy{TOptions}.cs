using System;
using System.Threading.Tasks;

using Bet.Extensions.Resilience.Abstractions.Options;

using Microsoft.Extensions.Logging;

using Polly;

namespace Bet.Extensions.Resilience.Abstractions.Policies
{
    public class RetryJitterPolicy<TOptions> :
                BasePolicy<TOptions>,
                IRetryJitterPolicy<TOptions> where TOptions : RetryJitterPolicyOptions
    {
        public RetryJitterPolicy(
            PolicyOptions policyOptions,
            IServiceProvider serviceProvider,
            IPolicyOptionsConfigurator<TOptions> policyOptionsConfigurator,
            IPolicyRegistryConfigurator registryConfigurator,
            ILogger<IPolicy<TOptions>> logger) : base(
                policyOptions,
                serviceProvider,
                policyOptionsConfigurator,
                registryConfigurator,
                logger)
        {
        }

        public Func<ILogger<IPolicy<TOptions>>, TOptions, Func<Exception, TimeSpan, int, Context, Task>> OnRetryAsync { get; set; } = (logger, options) =>
        {
            return (outcome, time, attempt, context) =>
            {
                logger.LogRetryOnRetry(time, attempt, context, options.MaxRetries, outcome.GetExceptionMessages());
                return Task.CompletedTask;
            };
        };

        public Func<ILogger<IPolicy<TOptions>>, TOptions, Action<Exception, TimeSpan, int, Context>> OnRetry { get; set; } = (logger, options) =>
        {
            return (outcome, time, attempt, context) => logger.LogRetryOnRetry(time, attempt, context, options.MaxRetries, outcome.GetExceptionMessages());
        };

        public override IAsyncPolicy GetAsyncPolicy()
        {
            if (OnRetryAsync == null)
            {
                throw new InvalidOperationException($"Please configure {nameof(OnRetryAsync)} property");
            }

            var delay = Backoff.DecorrelatedJitter(Options.MaxRetries, Options.SeedDelay, Options.MaxDelay);
            return Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(
                delay,
                OnRetryAsync(Logger, Options))
                .WithPolicyKey($"{PolicyOptions.Name}{PolicyNameSuffix}");
        }

        public override ISyncPolicy GetSyncPolicy()
        {
            if (OnRetry == null)
            {
                throw new InvalidOperationException($"Please configure {nameof(OnRetry)} property");
            }

            var delay = Backoff.DecorrelatedJitter(Options.MaxRetries, Options.SeedDelay, Options.MaxDelay);

            return Policy
               .Handle<Exception>()
               .WaitAndRetry(
               delay,
               OnRetry(Logger, Options))
               .WithPolicyKey(PolicyOptions.Name);
        }
    }
}
