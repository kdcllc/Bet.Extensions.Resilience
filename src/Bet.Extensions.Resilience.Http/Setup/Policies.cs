using System;
using System.Net;
using System.Net.Http;

using Bet.Extensions.Resilience.Http.Options;
using Bet.Extensions.Resilience.Http.Policies;
using Microsoft.Extensions.Logging;

using Polly;
using Polly.Extensions.Http;
using Polly.Registry;
using Polly.Timeout;

namespace Bet.Extensions.Resilience.Http.Setup
{
    internal sealed class Policies
    {
        public static void AddDefaultPolicies(IPolicyRegistry<string> policyRegistry, HttpPolicyOptions options)
        {
            if (policyRegistry.ContainsKey(HttpPoliciesKeys.HttpWaitAndRetryPolicyAsync)
                && policyRegistry.ContainsKey(HttpPoliciesKeys.HttpWaitAndRetryPolicy)
                && policyRegistry.ContainsKey(HttpPoliciesKeys.HttpCircuitBreakerPolicyAsync)
                && policyRegistry.ContainsKey(HttpPoliciesKeys.HttpCircuitBreakerPolicy)
                && policyRegistry.ContainsKey(HttpPoliciesKeys.HttpRequestTimeoutPolicyAsync)
                && policyRegistry.ContainsKey(HttpPoliciesKeys.HttpRequestTimeoutPolicy))
            {
                return;
            }

            // timeout async
            policyRegistry.Add(HttpPoliciesKeys.HttpRequestTimeoutPolicyAsync, GetTimeoutAsync(options.HttpRequestTimeout.Timeout));

            // timeout sync
            policyRegistry.Add(HttpPoliciesKeys.HttpRequestTimeoutPolicy, GetTimeout(options.HttpRequestTimeout.Timeout));

            // retry async
            policyRegistry.Add(
                HttpPoliciesKeys.HttpWaitAndRetryPolicyAsync,
                GetRetryAsync(
                    options.HttpRetry.Count,
                    options.HttpRetry.BackoffPower));

            // retry sync
            policyRegistry.Add(
                HttpPoliciesKeys.HttpWaitAndRetryPolicy,
                GetRetry(
                    options.HttpRetry.Count,
                    options.HttpRetry.BackoffPower));

            // circuit breaker async
            policyRegistry.Add(
                HttpPoliciesKeys.HttpCircuitBreakerPolicyAsync,
                GetCircuitBreakerAsync(
                    options.HttpCircuitBreaker.ExceptionsAllowedBeforeBreaking,
                    options.HttpCircuitBreaker.DurationOfBreak));

            // circuit breaker async
            policyRegistry.Add(
                    HttpPoliciesKeys.HttpCircuitBreakerPolicy,
                    GetCircuitBreaker(
                        options.HttpCircuitBreaker.ExceptionsAllowedBeforeBreaking,
                        options.HttpCircuitBreaker.DurationOfBreak));
        }

        public static ISyncPolicy<HttpResponseMessage> GetTimeout(TimeSpan span)
        {
            return Policy.Timeout<HttpResponseMessage>(span, TimeoutStrategy.Pessimistic);
        }

        public static IAsyncPolicy<HttpResponseMessage> GetTimeoutAsync(TimeSpan span)
        {
            return Policy.TimeoutAsync<HttpResponseMessage>(span, TimeoutStrategy.Pessimistic);
        }

        public static ISyncPolicy<HttpResponseMessage> GetCircuitBreaker(
            int numberOfExceptionsBeforeBreaking,
            TimeSpan durationOfBreak)
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .OrResult(response => response.StatusCode == HttpStatusCode.NotFound)
                .CircuitBreaker(
                    handledEventsAllowedBeforeBreaking: numberOfExceptionsBeforeBreaking,
                    durationOfBreak: durationOfBreak,
                    onBreak: (result, breakDelay, context) => OnBreakFunc(result, breakDelay, context, numberOfExceptionsBeforeBreaking),
                    onReset: (context) => OnResetFunc(context, durationOfBreak))
                .WithPolicyKey(HttpPoliciesKeys.HttpCircuitBreakerPolicy);
        }

        public static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerAsync(
            int numberOfExceptionsBeforeBreaking,
            TimeSpan durationOfBreak)
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .CircuitBreakerAsync(
                    handledEventsAllowedBeforeBreaking: numberOfExceptionsBeforeBreaking,
                    durationOfBreak: durationOfBreak,
                    onBreak: (result, breakDelay, context) => OnBreakFunc(result, breakDelay, context, numberOfExceptionsBeforeBreaking),
                    onReset: (context) => OnResetFunc(context, durationOfBreak))
                .WithPolicyKey(HttpPoliciesKeys.HttpCircuitBreakerPolicyAsync);
        }

        public static ISyncPolicy<HttpResponseMessage> GetRetry(
            int retryCount,
            int backOffPower)
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .WaitAndRetry(
                    retryCount: retryCount,
                    sleepDurationProvider: (retryAttempt, context) => OnDurationSetFunc(retryAttempt, context, backOffPower, retryCount),
                    onRetry: (result, timeSpan, retryAttempt, context) => OnRetryFunc(result, timeSpan, retryAttempt, context, retryCount))
                .WithPolicyKey(HttpPoliciesKeys.HttpWaitAndRetryPolicy);
        }

        public static IAsyncPolicy<HttpResponseMessage> GetRetryAsync(
            int retryCount,
            int backOffPower)
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .OrResult(response => response.StatusCode == HttpStatusCode.NotFound)
                .WaitAndRetryAsync(
                    retryCount: retryCount,
                    sleepDurationProvider: (retryAttempt, context) => OnDurationSetFunc(retryAttempt, context, backOffPower, retryCount),
                    onRetry: (result, timeSpan, retryAttempt, context) => OnRetryFunc(result, timeSpan, retryAttempt, context, retryCount))
                .WithPolicyKey(HttpPoliciesKeys.HttpWaitAndRetryPolicyAsync);
        }

        private static void OnBreakFunc(
            DelegateResult<HttpResponseMessage> result,
            TimeSpan breakDelay,
            Context context,
            int numberOfExceptionsBeforeBreaking)
        {
            context.TryGetLogger(out var logger);
            context.TryGetTypedHttpClientName(out var typedClientName);

            Logger.OnBreakError(
                logger,
                typedClientName,
                context.PolicyKey,
                result?.Result?.StatusCode == null ? 0 : (int)result?.Result?.StatusCode,
                numberOfExceptionsBeforeBreaking,
                result?.Exception);
        }

        private static TimeSpan OnDurationSetFunc(
            int retryAttempt,
            Context context,
            int backOffPower,
            int retryCount)
        {
            context.TryGetLogger(out var logger);
            context.TryGetTypedHttpClientName(out var typedClientName);

            var timeSpan = TimeSpan.FromSeconds(Math.Pow(backOffPower, retryAttempt));

            Logger.OnDurationSetInformation(
                logger,
                typedClientName,
                context.PolicyKey,
                timeSpan,
                retryAttempt,
                retryCount);

            return timeSpan;
        }

        private static void OnResetFunc(
            Context context,
            TimeSpan durationOfBreak)
        {
            context.TryGetLogger(out var logger);
            context.TryGetTypedHttpClientName(out var typedClientName);

            Logger.OnResetInformation(
                logger,
                typedClientName,
                context.PolicyKey,
                durationOfBreak);
        }

        private static void OnRetryFunc(
            DelegateResult<HttpResponseMessage> result,
            TimeSpan timeSpan,
            int retryAttempt,
            Context context,
            int retryCount)
        {
            context.TryGetLogger(out var logger);
            context.TryGetTypedHttpClientName(out var typedClientName);

            Logger.OnRetryInformation(
                logger,
                typedClientName,
                context.PolicyKey,
                timeSpan,
                retryAttempt,
                retryCount);

            if (retryAttempt != retryCount)
            {
                return;
            }

            Logger.OnRetryError(
                logger,
                typedClientName,
                context.PolicyKey,
                result?.Result?.StatusCode == null ? 0 : (int)result?.Result?.StatusCode,
                retryAttempt,
                retryCount,
                result?.Exception);
        }

        private static class Logger
        {
            private static readonly Action<ILogger, string, string, int, int, Exception> _onBreakError = LoggerMessage.Define<string, string, int, int>(
                LogLevel.Error,
                EventIds.OnBreak,
                "{TypedHttpClientName} executing OnBreak with Policy: {PolicyKey} returned StatustCode: {StatusCode} number of Exceptions before break {numberOfExceptionsBeforeBreaking}");

            private static readonly Action<ILogger, string, string, TimeSpan, int, int, Exception> _onDurationSet = LoggerMessage.Define<string, string, TimeSpan, int, int>(
                LogLevel.Information,
                EventIds.OnDurationSet,
                "{TypedHttpClientName} executing OnDurationSet with Policy: {PolicyKey} timespan: {timeSpan} Retries: {retryAttempt} of {retryCount}");

            private static readonly Action<ILogger, string, string, TimeSpan, Exception> _onResetInformation = LoggerMessage.Define<string, string, TimeSpan>(
                LogLevel.Information,
                EventIds.OnReset,
                "{TypedHttpClientName} executing OnReset with Policy: {PolicyKey} after break duration of {durationOfBreak}");

            private static readonly Action<ILogger, string, string, int, int, int, Exception> _onRetryError = LoggerMessage.Define<string, string, int, int, int>(
                LogLevel.Error,
                EventIds.OnRetry,
                "{TypedHttpClientName} executing OnRetry with Policy: {PolicyKey} returned StatustCode: {StatusCode} on Retries: {retryAttempt} of {retryCount}");

            private static readonly Action<ILogger, string, string, TimeSpan, int, int, Exception> _onRetryInformation = LoggerMessage.Define<string, string, TimeSpan, int, int>(
                LogLevel.Information,
                EventIds.OnRetry,
                "{TypedHttpClientName} executing OnRetry with Policy: {PolicyKey} timespan: {timeSpan} Retries: {retryAttempt} of {retryCount}");

            public static void OnBreakError(
                ILogger logger,
                string typedHttpClientName,
                string policyKey,
                int statusCode,
                int numberOfExceptionsBeforeBreaking,
                Exception exception)
            {
                if (logger != null)
                {
                    _onBreakError(
                        logger,
                        typedHttpClientName,
                        policyKey,
                        statusCode,
                        numberOfExceptionsBeforeBreaking,
                        exception);
                }
            }

            public static void OnDurationSetInformation(
                ILogger logger,
                string typedHttpClientName,
                string policyKey,
                TimeSpan timeSpan,
                int retryAttempt,
                int retryCount)
            {
                if (logger != null)
                {
                    _onDurationSet(
                        logger,
                        typedHttpClientName,
                        policyKey,
                        timeSpan,
                        retryAttempt,
                        retryCount,
                        null);
                }
            }

            public static void OnResetInformation(
                ILogger logger,
                string typedHttpClientName,
                string policyKey,
                TimeSpan timeSpan)
            {
                if (logger != null)
                {
                    _onResetInformation(
                        logger,
                        typedHttpClientName,
                        policyKey,
                        timeSpan,
                        null);
                }
            }

            public static void OnRetryError(
                ILogger logger,
                string typedHttpClientName,
                string policyKey,
                int statusCode,
                int retryAttempt,
                int retryCount,
                Exception exception)
            {
                if (logger != null)
                {
                    _onRetryError(
                        logger,
                        typedHttpClientName,
                        policyKey,
                        statusCode,
                        retryAttempt,
                        retryCount,
                        exception);
                }
            }

            public static void OnRetryInformation(
                ILogger logger,
                string typedHttpClientName,
                string policyKey,
                TimeSpan timeSpan,
                int retryAttempt,
                int retryCount)
            {
                if (logger != null)
                {
                    _onRetryInformation(logger, typedHttpClientName, policyKey, timeSpan, retryAttempt, retryCount, null);
                }
            }

            public static class EventIds
            {
                public static readonly EventId OnBreak = new EventId(102, nameof(OnBreak));
                public static readonly EventId OnDurationSet = new EventId(101, nameof(OnDurationSet));
                public static readonly EventId OnReset = new EventId(103, nameof(OnReset));
                public static readonly EventId OnRetry = new EventId(100, nameof(OnRetry));
            }
        }
    }
}
