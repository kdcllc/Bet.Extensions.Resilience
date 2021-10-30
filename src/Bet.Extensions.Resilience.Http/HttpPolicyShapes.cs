using System.Net;

using Bet.Extensions.Resilience.Abstractions;
using Bet.Extensions.Resilience.Abstractions.Options;
using Bet.Extensions.Resilience.Http.Options;

using Microsoft.Extensions.Logging;

using Polly.Bulkhead;
using Polly.CircuitBreaker;
using Polly.Extensions.Http;
using Polly.Timeout;

namespace Polly
{
    public static class HttpPolicyShapes
    {
        public static string PolicyNameSuffix => "Async";

        public static void HttpCreateFallbackAsync<TOptions>(
            this PolicyBucketOptions<TOptions> policyProfile,
            bool addSyffix = false)
            where TOptions : HttpFallbackPolicyOptions
        {
            policyProfile.ConfigurePolicy = (options, logger) =>
            {
                var keyName = addSyffix ? $"{options.Name}{PolicyNameSuffix}" : options.Name;

                var policy = Policy<HttpResponseMessage>

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

                Task<HttpResponseMessage> FallbackActionAsync(DelegateResult<HttpResponseMessage> outcome, Context context, CancellationToken cancellationToken)
                {
                    // can be adding anything else ...
                    logger.LogOnFallabck(context, outcome.GetMessage());
                    return Task.FromResult(new HttpResponseMessage((HttpStatusCode)options.StatusCode) { ReasonPhrase = options.Message });
                }

                Task OnFallbackAction(DelegateResult<HttpResponseMessage> outcome, Context context)
                {
                    logger.LogOnFallabck(context, outcome.GetMessage());
                    return Task.CompletedTask;
                }

                return policy;
            };
        }

        public static void HttpCreateFallback<TOptions>(this PolicyBucketOptions<TOptions> policyProfile)
            where TOptions : HttpFallbackPolicyOptions
        {
            policyProfile.ConfigurePolicy = (options, logger) =>
            {
                var policy = Policy<HttpResponseMessage>

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

                HttpResponseMessage Fallback(DelegateResult<HttpResponseMessage> outcome, Context context, CancellationToken cancellationToken)
                {
                    logger.LogOnFallabck(context, outcome.GetMessage());
                    return new HttpResponseMessage((HttpStatusCode)options.StatusCode) { ReasonPhrase = options.Message };
                }

                void OnFallbackAction(DelegateResult<HttpResponseMessage> outcome, Context context)
                {
                    logger.LogOnFallabck(context, outcome.GetMessage());
                }

                return policy;
            };
        }

        public static void HttpCreateCircuitBreakerAsync<TOptions>(
            this PolicyBucketOptions<TOptions> policyProfile,
            bool addSyffix = false)
            where TOptions : CircuitBreakerPolicyOptions
        {
            policyProfile.ConfigurePolicy = (options, logger) =>
            {
                var keyName = addSyffix ? $"{options.Name}{PolicyNameSuffix}" : options.Name;

                return HttpPolicyExtensions
                        .HandleTransientHttpError()
                        .CircuitBreakerAsync(
                            options.ExceptionsAllowedBeforeBreaking,
                            options.DurationOfBreak,
                            OnBreak,
                            OnReset,
                            OnHalfOpen)
                        .WithPolicyKey(keyName);

                void OnBreak(DelegateResult<HttpResponseMessage> outcome, CircuitState state, TimeSpan time, Context context)
                {
                    logger.LogCircuitBreakerOnBreak(time, context, state, options, outcome.GetMessage());
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

        public static void HttpCreateCircuitBreaker<TOptions>(this PolicyBucketOptions<TOptions> policyProfile)
            where TOptions : CircuitBreakerPolicyOptions
        {
            policyProfile.ConfigurePolicy = (options, logger) =>
            {
                return HttpPolicyExtensions
                        .HandleTransientHttpError()
                        .CircuitBreaker(
                            options.ExceptionsAllowedBeforeBreaking,
                            options.DurationOfBreak,
                            OnBreak,
                            OnReset,
                            OnHalfOpen)
                        .WithPolicyKey(options.Name);

                void OnBreak(DelegateResult<HttpResponseMessage> outcome, CircuitState state, TimeSpan time, Context context)
                {
                    logger.LogCircuitBreakerOnBreak(time, context, state, options, outcome.GetMessage());
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

        public static void HttpCreateRetryAsync<TOptions>(
            this PolicyBucketOptions<TOptions> policyProfile,
            bool addSyffix = false)
            where TOptions : RetryPolicyOptions
        {
            policyProfile.ConfigurePolicy = (options, logger) =>
            {
                var keyName = addSyffix ? $"{options.Name}{PolicyNameSuffix}" : options.Name;
                return HttpPolicyExtensions
                        .HandleTransientHttpError()

                         // HttpStatusCode.TooManyRequests if not specific logic is applied for the retries.
                         .OrResult(x => (int)x.StatusCode == 429)

                         .WaitAndRetryAsync(
                            options.Count,
                            OnDuration,
                            OnRetryAsync)
                         .WithPolicyKey(keyName);

                TimeSpan OnDuration(int attempt, DelegateResult<HttpResponseMessage> outcome, Context context)
                {
                    logger.LogRetryOnDuration(attempt, context, options.Count, outcome.GetMessage());
                    return TimeSpan.FromSeconds(Math.Pow(options.BackoffPower, attempt));
                }

                Task OnRetryAsync(DelegateResult<HttpResponseMessage> outcome, TimeSpan time, int attempt, Context context)
                {
                    logger.LogRetryOnRetry(time, attempt, context, options.Count, outcome.GetMessage());
                    return Task.CompletedTask;
                }
            };
        }

        public static void HttpCreateRetry<TOptions>(this PolicyBucketOptions<TOptions> policyProfile)
           where TOptions : RetryPolicyOptions
        {
            policyProfile.ConfigurePolicy = (options, logger) =>
            {
                return HttpPolicyExtensions
                        .HandleTransientHttpError()

                       // HttpStatusCode.TooManyRequests if not specific logic is applied for the retries.
                       .OrResult(x => (int)x.StatusCode == 429)

                       .WaitAndRetry(
                          options.Count,
                          OnDuration,
                          OnRetry)
                       .WithPolicyKey(options.Name);

                TimeSpan OnDuration(int attempt, DelegateResult<HttpResponseMessage> outcome, Context context)
                {
                    logger.LogRetryOnDuration(attempt, context, options.Count, outcome.GetMessage());
                    return TimeSpan.FromSeconds(Math.Pow(options.BackoffPower, attempt));
                }

                void OnRetry(DelegateResult<HttpResponseMessage> outcome, TimeSpan time, int attempt, Context context)
                {
                    logger.LogRetryOnRetry(time, attempt, context, options.Count, outcome.GetMessage());
                }
            };
        }

        public static void HttpCreateRetryJitterAsync<TOptions>(
            PolicyBucketOptions<TOptions> policyProfile,
            bool addSyffix = false) where TOptions : RetryJitterPolicyOptions
        {
            policyProfile.ConfigurePolicy = (options, logger) =>
            {
                var keyName = addSyffix ? $"{options.Name}{PolicyNameSuffix}" : options.Name;

                var delay = Backoff.DecorrelatedJitter(options.MaxRetries, options.SeedDelay, options.MaxDelay);

                return HttpPolicyExtensions
                        .HandleTransientHttpError()

                         // HttpStatusCode.TooManyRequests if not specific logic is applied for the retries.
                         .OrResult(x => (int)x.StatusCode == 429)

                         .WaitAndRetryAsync(
                            delay,
                            OnRetryAsync)
                         .WithPolicyKey(keyName);

                Task OnRetryAsync(DelegateResult<HttpResponseMessage> outcome, TimeSpan time, int attempt, Context context)
                {
                    logger.LogRetryOnRetry(time, attempt, context, options.MaxRetries, outcome.GetMessage());
                    return Task.CompletedTask;
                }
            };
        }

        public static void HttpCreateRetryJitter<TOptions>(this PolicyBucketOptions<TOptions> policyProfile)
            where TOptions : RetryJitterPolicyOptions
        {
            policyProfile.ConfigurePolicy = (options, logger) =>
            {
                var delay = Backoff.DecorrelatedJitter(options.MaxRetries, options.SeedDelay, options.MaxDelay);

                return HttpPolicyExtensions
                        .HandleTransientHttpError()

                         // HttpStatusCode.TooManyRequests if not specific logic is applied for the retries.
                         .OrResult(x => (int)x.StatusCode == 429)

                       .WaitAndRetry(
                         delay,
                         OnRetry)
                       .WithPolicyKey(options.Name);

                void OnRetry(DelegateResult<HttpResponseMessage> outcome, TimeSpan time, int attempt, Context context)
                {
                    logger.LogRetryOnRetry(time, attempt, context, options.MaxRetries, outcome.GetMessage());
                }
            };
        }
    }
}
