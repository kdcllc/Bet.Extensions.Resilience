using Bet.Extensions.Resilience.Abstractions;
using Bet.Extensions.Resilience.Abstractions.Options;

using Microsoft.Extensions.Logging;

using Polly.Bulkhead;
using Polly.CircuitBreaker;
using Polly.Timeout;

namespace Polly;

public static partial class PolicyShapes
{
    public static string PolicyNameSuffix => "Async";

    public static void CreateTimeoutAsync<TOptions>(
        this PolicyBucketOptions<TOptions> policyProfile,
        TimeoutStrategy timeoutStrategy = TimeoutStrategy.Pessimistic,
        bool addSyffix = false) where TOptions : TimeoutPolicyOptions
    {
        policyProfile.ConfigurePolicy = (options, logger) =>
        {
            var keyName = addSyffix ? $"{options.Name}{PolicyNameSuffix}" : options.Name;

            var policy = Policy
                    .TimeoutAsync(options.Timeout, timeoutStrategy, OnTimeoutAsync)
                    .WithPolicyKey(keyName);

            Task OnTimeoutAsync(Context context, TimeSpan timeout, Task abandonedTask, Exception ex)
            {
                logger.LogOnTimeout(context, timeout, ex.GetExceptionMessages());
                return Task.CompletedTask;
            }

            return policy;
        };
    }

    public static void CreateTimeout<TOptions>(
        this PolicyBucketOptions<TOptions> policyProfile,
        TimeoutStrategy timeoutStrategy = TimeoutStrategy.Pessimistic)
        where TOptions : TimeoutPolicyOptions
    {
        policyProfile.ConfigurePolicy = (options, logger) =>
        {
            var policy = Policy
                    .Timeout(options.Timeout, timeoutStrategy, OnTimeout)
                    .WithPolicyKey(options.Name);

            void OnTimeout(Context context, TimeSpan timeout, Task abandonedTask, Exception ex)
            {
                logger.LogOnTimeout(context, timeout, ex.GetExceptionMessages());
            }

            return policy;
        };
    }

    public static void CreateFallabckAsync<TOptions>(
        this PolicyBucketOptions<TOptions> policyProfile,
        bool addSyffix = false)
        where TOptions : FallbackPolicyOptions
    {
        policyProfile.ConfigurePolicy = (options, logger) =>
        {
            var keyName = addSyffix ? $"{options.Name}{PolicyNameSuffix}" : options.Name;

            return Policy

                // Polly timeout policy exception
                .Handle<TimeoutRejectedException>()

                // Polly Broken Circuit
                .Or<BrokenCircuitException>()

                .Or<TimeoutRejectedException>()

                // Message Handler timeout
                .Or<TimeoutException>()

                // Client canceled
                .Or<TaskCanceledException>()

                // failed bulkhead policy
                .Or<BulkheadRejectedException>()
                .FallbackAsync(fallbackAction: FallBackActionAsync, onFallbackAsync: OnFallbackAsync)
                .WithPolicyKey(keyName);

            Task FallBackActionAsync(Exception ex, Context context, CancellationToken cancellationToken)
            {
                logger.LogOnFallabck(context, ex.GetExceptionMessages());
                return Task.CompletedTask;
            }

            Task OnFallbackAsync(Exception ex, Context context)
            {
                logger.LogOnFallabck(context, ex.GetExceptionMessages());
                return Task.CompletedTask;
            }
        };
    }

    public static void CreateFallabck<TOptions>(this PolicyBucketOptions<TOptions> policyProfile)
        where TOptions : FallbackPolicyOptions
    {
        policyProfile.ConfigurePolicy = (options, logger) =>
        {
            return Policy

                // Polly timeout policy exception
                .Handle<TimeoutRejectedException>()

                // Polly Broken Circuit
                .Or<BrokenCircuitException>()

                .Or<TimeoutRejectedException>()

                // Message Handler timeout
                .Or<TimeoutException>()

                // Client canceled
                .Or<TaskCanceledException>()

                // failed bulkhead policy
                .Or<BulkheadRejectedException>()
                .Fallback(fallbackAction: FallBackAction, onFallback: OnFallback)
                .WithPolicyKey(options.Name);

            void FallBackAction(Exception ex, Context context, CancellationToken cancellationToken)
            {
                logger.LogOnFallabck(context, ex.GetExceptionMessages());
            }

            void OnFallback(Exception ex, Context context)
            {
                logger.LogOnFallabck(context, ex.GetExceptionMessages());
            }
        };
    }

    public static void CreateBulkheadAsync<TOptions>(
        this PolicyBucketOptions<TOptions> policyProfile,
        bool addSyffix = false)
        where TOptions : BulkheadPolicyOptions
    {
        policyProfile.ConfigurePolicy = (options, logger) =>
        {
            var keyName = addSyffix ? $"{options.Name}{PolicyNameSuffix}" : options.Name;

            if (options.MaxQueuedItems.HasValue)
            {
                return Policy
                       .BulkheadAsync(
                           options.MaxParallelization,
                           options.MaxQueuedItems.Value,
                           OnBulkheadRejectedAsync)
                       .WithPolicyKey(keyName);
            }

            return Policy
                   .BulkheadAsync(
                       options.MaxParallelization,
                       OnBulkheadRejectedAsync)
                   .WithPolicyKey(keyName);

            Task OnBulkheadRejectedAsync(Context context)
            {
                logger.LogOnBulkheadRejected(context, options);
                return Task.CompletedTask;
            }
        };
    }

    public static void CreateBulkhead<TOptions>(this PolicyBucketOptions<TOptions> policyProfile)
        where TOptions : BulkheadPolicyOptions
    {
        policyProfile.ConfigurePolicy = (options, logger) =>
        {
            if (options.MaxQueuedItems.HasValue)
            {
                return Policy
                       .Bulkhead(
                           options.MaxParallelization,
                           options.MaxQueuedItems.Value,
                           OnBulkheadRejected)
                       .WithPolicyKey(options.Name);
            }

            return Policy
                   .Bulkhead(
                       options.MaxParallelization,
                       OnBulkheadRejected)
                   .WithPolicyKey(options.Name);

            void OnBulkheadRejected(Context context)
            {
                logger.LogOnBulkheadRejected(context, options);
            }
        };
    }

    public static void CreateCircuitBreakerAsync<TOptions>(
        this PolicyBucketOptions<TOptions> policyProfile,
        bool addSyffix = false)
        where TOptions : CircuitBreakerPolicyOptions
    {
        policyProfile.ConfigurePolicy = (options, logger) =>
        {
            var keyName = addSyffix ? $"{options.Name}{PolicyNameSuffix}" : options.Name;
            return Policy
                  .Handle<Exception>()
                  .CircuitBreakerAsync(
                      options.ExceptionsAllowedBeforeBreaking,
                      options.DurationOfBreak,
                      OnBreak,
                      OnReset,
                      OnHalfOpen)
                  .WithPolicyKey(keyName);

            void OnBreak(Exception ex, CircuitState state, TimeSpan time, Context context)
            {
                logger.LogCircuitBreakerOnBreak(time, context, state, options, ex.GetExceptionMessages());
            }

            void OnReset(Context context)
            {
                logger.LogCircuitBreakerOnReset(context, options);
            }

            void OnHalfOpen()
            {
                logger.LogDebug("[{circuitBreakerPolicy}][OnHalfOpen]", options.Name);
            }
        };
    }

    public static void CreateCircuitBreaker<TOptions>(this PolicyBucketOptions<TOptions> policyProfile)
        where TOptions : CircuitBreakerPolicyOptions
    {
        policyProfile.ConfigurePolicy = (options, logger) =>
        {
            return Policy
                .Handle<Exception>()
                .CircuitBreaker(
                    options.ExceptionsAllowedBeforeBreaking,
                    options.DurationOfBreak,
                    OnBreak,
                    OnReset,
                    OnHalfOpen)
                .WithPolicyKey(options.Name);

            void OnBreak(Exception ex, CircuitState state, TimeSpan time, Context context)
            {
                logger.LogCircuitBreakerOnBreak(time, context, state, options, ex.GetExceptionMessages());
            }

            void OnReset(Context context)
            {
                logger.LogCircuitBreakerOnReset(context, options);
            }

            void OnHalfOpen()
            {
                logger.LogDebug("[{circuitBreakerPolicy}][OnHalfOpen]", options.Name);
            }
        };
    }

    public static void CreateRetryAsync<TOptions>(
        this PolicyBucketOptions<TOptions> policyProfile,
        bool addSyffix = false)
        where TOptions : RetryPolicyOptions
    {
        policyProfile.ConfigurePolicy = (options, logger) =>
        {
            var keyName = addSyffix ? $"{options.Name}{PolicyNameSuffix}" : options.Name;

            return Policy
                .Handle<Exception>()
                   .WaitAndRetryAsync(
                        options.Count,
                        OnDuration,
                        OnRetryAsync)
                   .WithPolicyKey(keyName);

            TimeSpan OnDuration(int attempt, Exception outcome, Context context)
            {
                logger.LogRetryOnDuration(attempt, context, options.Count, outcome.GetExceptionMessages());
                return TimeSpan.FromSeconds(Math.Pow(options.BackoffPower, attempt));
            }

            Task OnRetryAsync(Exception outcome, TimeSpan time, int attempt, Context context)
            {
                logger.LogRetryOnRetry(time, attempt, context, options.Count, outcome.GetExceptionMessages());
                return Task.CompletedTask;
            }
        };
    }

    public static void CreateRetry<TOptions>(this PolicyBucketOptions<TOptions> policyProfile)
        where TOptions : RetryPolicyOptions
    {
        policyProfile.ConfigurePolicy = (options, logger) =>
        {
            return Policy
                    .Handle<Exception>()
                       .WaitAndRetry(
                            options.Count,
                            OnDuration,
                            OnRetry)
                       .WithPolicyKey(options.Name);

            TimeSpan OnDuration(int attempt, Exception outcome, Context context)
            {
                logger.LogRetryOnDuration(attempt, context, options.Count, outcome.GetExceptionMessages());
                return TimeSpan.FromSeconds(Math.Pow(options.BackoffPower, attempt));
            }

            void OnRetry(Exception outcome, TimeSpan time, int attempt, Context context)
            {
                logger.LogRetryOnRetry(time, attempt, context, options.Count, outcome.GetExceptionMessages());
            }
        };
    }

    public static void CreateJitterRetryAsync<TOptions>(
        this PolicyBucketOptions<TOptions> policyProfile,
        bool addSyffix = false) where TOptions : RetryJitterPolicyOptions
    {
        policyProfile.ConfigurePolicy = (options, logger) =>
        {
            var keyName = addSyffix ? $"{options.Name}{PolicyNameSuffix}" : options.Name;

            var delay = Backoff.DecorrelatedJitter(options.MaxRetries, options.SeedDelay, options.MaxDelay);

            return Policy
                .Handle<Exception>()
                   .WaitAndRetryAsync(
                        delay,
                        OnRetryAsync)
                   .WithPolicyKey(keyName);

            Task OnRetryAsync(Exception outcome, TimeSpan time, int attempt, Context context)
            {
                logger.LogRetryOnRetry(time, attempt, context, options.MaxRetries, outcome.GetExceptionMessages());
                return Task.CompletedTask;
            }
        };
    }

    public static void CreateJitterRetry<TOptions>(this PolicyBucketOptions<TOptions> policyProfile)
        where TOptions : RetryJitterPolicyOptions
    {
        policyProfile.ConfigurePolicy = (options, logger) =>
        {
            var delay = Backoff.DecorrelatedJitter(options.MaxRetries, options.SeedDelay, options.MaxDelay);

            return Policy
                    .Handle<Exception>()
                       .WaitAndRetry(
                            delay,
                            OnRetry)
                       .WithPolicyKey(options.Name);
            void OnRetry(Exception outcome, TimeSpan time, int attempt, Context context)
            {
                logger.LogRetryOnRetry(time, attempt, context, options.MaxRetries, outcome.GetExceptionMessages());
            }
        };
    }
}
