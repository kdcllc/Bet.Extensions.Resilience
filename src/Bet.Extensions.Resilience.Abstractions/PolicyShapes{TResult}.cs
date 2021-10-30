using Bet.Extensions.Resilience.Abstractions;
using Bet.Extensions.Resilience.Abstractions.Options;

using Microsoft.Extensions.Logging;

using Polly.Bulkhead;
using Polly.CircuitBreaker;
using Polly.Timeout;

namespace Polly
{
    public static partial class PolicyShapes
    {
        public static void CreateTimeoutAsync<TOptions, TResult>(
            PolicyBucketOptions<TOptions> policyProfile,
            TimeoutStrategy timeoutStrategy = TimeoutStrategy.Pessimistic,
            bool addSyffix = false) where TOptions : TimeoutPolicyOptions
        {
            policyProfile.ConfigurePolicy = (options, logger) =>
            {
                var keyName = addSyffix ? $"{options.Name}{PolicyNameSuffix}" : options.Name;

                var policy = Policy
                        .TimeoutAsync<TResult>(options.Timeout, timeoutStrategy, OnTimeoutAsync)
                        .WithPolicyKey(keyName);

                Task OnTimeoutAsync(Context context, TimeSpan timeout, Task abandonedTask, Exception ex)
                {
                    logger.LogOnTimeout(context, timeout, ex.GetExceptionMessages());
                    return Task.CompletedTask;
                }

                return policy;
            };
        }

        public static void CreateTimeout<TOptions, TResult>(
            this PolicyBucketOptions<TOptions> policyProfile,
            TimeoutStrategy timeoutStrategy = TimeoutStrategy.Pessimistic)
            where TOptions : TimeoutPolicyOptions
        {
            policyProfile.ConfigurePolicy = (options, logger) =>
            {
                var policy = Policy
                        .Timeout<TResult>(options.Timeout, timeoutStrategy, OnTimeout)
                        .WithPolicyKey(options.Name);

                void OnTimeout(Context context, TimeSpan timeout, Task abandonedTask, Exception ex)
                {
                    logger.LogOnTimeout(context, timeout, ex.GetExceptionMessages());
                }

                return policy;
            };
        }

        /// <summary>
        /// Create Fallback profile.
        /// </summary>
        /// <typeparam name="TOptions"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="policyProfile"></param>
        /// <param name="func">'outcome.GetExceptionMessages()'.</param>
        /// <param name="addSyffix"></param>
        public static void CreateFallbackAsync<TOptions, TResult>(
            PolicyBucketOptions<TOptions> policyProfile,
            Func<DelegateResult<TResult>, string> func,
            bool addSyffix = false)
            where TOptions : FallbackPolicyOptions
        {
            policyProfile.ConfigurePolicy = (options, logger) =>
            {
                var keyName = addSyffix ? $"{options.Name}{PolicyNameSuffix}" : options.Name;

                var policy = Policy<TResult>

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

                .FallbackAsync(fallbackAction: FallbackActionAsync, onFallbackAsync: OnFallbackAction)

                .WithPolicyKey(keyName);

                Task<TResult> FallbackActionAsync(DelegateResult<TResult> outcome, Context context, CancellationToken cancellationToken)
                {
                    logger.LogOnFallabck(context, func(outcome));
                    return Task.FromResult(outcome.Result);
                }

                Task OnFallbackAction(DelegateResult<TResult> outcome, Context context)
                {
                    return Task.CompletedTask;
                }

                return policy;
            };
        }

        public static void CreateFallback<TOptions, TResult>(
            PolicyBucketOptions<TOptions> policyProfile,
            Func<DelegateResult<TResult>, string> func)
            where TOptions : FallbackPolicyOptions
        {
            policyProfile.ConfigurePolicy = (options, logger) =>
            {
                var policy = Policy<TResult>

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

                .Fallback(fallbackAction: Fallback, onFallback: OnFallbackAction)

                .WithPolicyKey(options.Name);

                TResult Fallback(DelegateResult<TResult> outcome, Context context, CancellationToken cancellationToken)
                {
                    logger.LogOnFallabck(context, func(outcome));
                    return outcome.Result;
                }

                void OnFallbackAction(DelegateResult<TResult> outcome, Context context)
                {
                }

                return policy;
            };
        }

        public static void CreateBulkheadAsync<TOptions, TResult>(
            PolicyBucketOptions<TOptions> policyProfile,
            bool addSyffix = false)
            where TOptions : BulkheadPolicyOptions
        {
            policyProfile.ConfigurePolicy = (options, logger) =>
            {
                var keyName = addSyffix ? $"{options.Name}{PolicyNameSuffix}" : options.Name;

                if (options.MaxQueuedItems.HasValue)
                {
                    return Policy
                        .BulkheadAsync<TResult>(
                            options.MaxParallelization,
                            options.MaxQueuedItems.Value,
                            OnBulkheadRejectedAsync)
                        .WithPolicyKey(keyName);
                }

                return Policy
                    .BulkheadAsync<TResult>(
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

        public static void CreateBulkhead<TOptions, TResult>(PolicyBucketOptions<TOptions> policyProfile)
            where TOptions : BulkheadPolicyOptions
        {
            policyProfile.ConfigurePolicy = (options, logger) =>
            {
                if (options.MaxQueuedItems.HasValue)
                {
                    return Policy
                        .Bulkhead<TResult>(
                            options.MaxParallelization,
                            options.MaxQueuedItems.Value,
                            OnBulkheadRejected)
                        .WithPolicyKey(options.Name);
                }

                return Policy
                    .Bulkhead<TResult>(
                        options.MaxParallelization,
                        OnBulkheadRejected)
                    .WithPolicyKey(options.Name);

                void OnBulkheadRejected(Context context)
                {
                    logger.LogOnBulkheadRejected(context, options);
                }
            };
        }

        public static void CreateCircuitBreakerAsync<TOptions, TResult>(
            PolicyBucketOptions<TOptions> policyProfile,
            Func<DelegateResult<TResult>, string> func,
            bool addSyffix = false)
            where TOptions : CircuitBreakerPolicyOptions
        {
            policyProfile.ConfigurePolicy = (options, logger) =>
            {
                var keyName = addSyffix ? $"{options.Name}{PolicyNameSuffix}" : options.Name;
                return Policy<TResult>
                      .Handle<Exception>()
                      .CircuitBreakerAsync(
                          options.ExceptionsAllowedBeforeBreaking,
                          options.DurationOfBreak,
                          OnBreak,
                          OnReset,
                          OnHalfOpen)
                      .WithPolicyKey(keyName);
                void OnBreak(DelegateResult<TResult> outcome, CircuitState state, TimeSpan time, Context context)
                {
                    logger.LogCircuitBreakerOnBreak(time, context, state, options, func(outcome));
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

        public static void CreateCircuitBreaker<TOptions, TResult>(
            PolicyBucketOptions<TOptions> policyProfile,
            Func<DelegateResult<TResult>, string> func) where TOptions : CircuitBreakerPolicyOptions
        {
            policyProfile.ConfigurePolicy = (options, logger) =>
            {
                return Policy<TResult>
                     .Handle<Exception>()
                     .CircuitBreaker(
                         options.ExceptionsAllowedBeforeBreaking,
                         options.DurationOfBreak,
                         OnBreak,
                         OnReset,
                         OnHalfOpen)
                     .WithPolicyKey(options.Name);

                void OnBreak(DelegateResult<TResult> outcome, CircuitState state, TimeSpan time, Context context)
                {
                    logger.LogCircuitBreakerOnBreak(time, context, state, options, func(outcome));
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

        public static void CreateRetryAsync<TOptions, TResult>(
            PolicyBucketOptions<TOptions> policyProfile,
            Func<DelegateResult<TResult>, string> func,
            bool addSyffix = false)
            where TOptions : RetryPolicyOptions
        {
            policyProfile.ConfigurePolicy = (options, logger) =>
            {
                var keyName = addSyffix ? $"{options.Name}{PolicyNameSuffix}" : options.Name;
                return Policy<TResult>
                         .Handle<Exception>()
                         .WaitAndRetryAsync(
                            options.Count,
                            OnDuration,
                            OnRetryAsync)
                         .WithPolicyKey(keyName);

                TimeSpan OnDuration(int attempt, DelegateResult<TResult> outcome, Context context)
                {
                    logger.LogRetryOnDuration(attempt, context, options.Count, func(outcome));
                    return TimeSpan.FromSeconds(Math.Pow(options.BackoffPower, attempt));
                }

                Task OnRetryAsync(DelegateResult<TResult> outcome, TimeSpan time, int attempt, Context context)
                {
                    logger.LogRetryOnRetry(time, attempt, context, options.Count, func(outcome));
                    return Task.CompletedTask;
                }
            };
        }

        public static void CreateRetry<TOptions, TResult>(
           PolicyBucketOptions<TOptions> policyProfile,
           Func<DelegateResult<TResult>, string> func)
           where TOptions : RetryPolicyOptions
        {
            policyProfile.ConfigurePolicy = (options, logger) =>
            {
                return Policy<TResult>
                       .Handle<Exception>()
                       .WaitAndRetry(
                          options.Count,
                          OnDuration,
                          OnRetry)
                       .WithPolicyKey(options.Name);

                TimeSpan OnDuration(int attempt, DelegateResult<TResult> outcome, Context context)
                {
                    logger.LogRetryOnDuration(attempt, context, options.Count, func(outcome));
                    return TimeSpan.FromSeconds(Math.Pow(options.BackoffPower, attempt));
                }

                void OnRetry(DelegateResult<TResult> outcome, TimeSpan time, int attempt, Context context)
                {
                    logger.LogRetryOnRetry(time, attempt, context, options.Count, func(outcome));
                }
            };
        }

        public static void CreateRetryJitterAsync<TOptions, TResult>(
            PolicyBucketOptions<TOptions> policyProfile,
            Func<DelegateResult<TResult>, string> func,
            bool addSyffix = false) where TOptions : RetryJitterPolicyOptions
        {
            policyProfile.ConfigurePolicy = (options, logger) =>
            {
                var keyName = addSyffix ? $"{options.Name}{PolicyNameSuffix}" : options.Name;

                var delay = Backoff.DecorrelatedJitter(options.MaxRetries, options.SeedDelay, options.MaxDelay);

                return Policy<TResult>
                         .Handle<Exception>()
                         .WaitAndRetryAsync(
                            delay,
                            OnRetryAsync)
                         .WithPolicyKey(keyName);

                Task OnRetryAsync(DelegateResult<TResult> outcome, TimeSpan time, int attempt, Context context)
                {
                    logger.LogRetryOnRetry(time, attempt, context, options.MaxRetries, func(outcome));
                    return Task.CompletedTask;
                }
            };
        }

        public static void CreateRetryJitter<TOptions, TResult>(
            PolicyBucketOptions<TOptions> policyProfile,
            Func<DelegateResult<TResult>, string> func) where TOptions : RetryJitterPolicyOptions
        {
            policyProfile.ConfigurePolicy = (options, logger) =>
            {
                var delay = Backoff.DecorrelatedJitter(options.MaxRetries, options.SeedDelay, options.MaxDelay);

                return Policy<TResult>
                       .Handle<Exception>()
                       .WaitAndRetry(
                         delay,
                         OnRetry)
                       .WithPolicyKey(options.Name);

                void OnRetry(DelegateResult<TResult> outcome, TimeSpan time, int attempt, Context context)
                {
                    logger.LogRetryOnRetry(time, attempt, context, options.MaxRetries, func(outcome));
                }
            };
        }
    }
}
