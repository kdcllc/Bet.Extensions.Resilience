using System;

using Microsoft.Extensions.Logging;

using Polly;

namespace Bet.Extensions.Resilience.Abstractions.Policies
{
    public static class RetryPolicyBuilder
    {
        /// <summary>
        /// Gets <see cref="IAsyncPolicy"/> type of WaitAndRetryAsync.
        /// </summary>
        /// <typeparam name="TException">The type of the exception to be used within the condition.</typeparam>
        /// <param name="condition">The condition for the policy handling the request.</param>
        /// <param name="retryCount">The retry count for the policy.</param>
        /// <param name="backOffPower">The back off power to be used to calculate the next timeout.</param>
        /// <param name="policyName">The name of the policy.</param>
        /// <returns><see cref="IAsyncPolicy"/>.</returns>
        public static IAsyncPolicy GetWaitAndRetryAsync<TException>(Func<TException, bool> condition, int retryCount, int backOffPower, string policyName)
            where TException : Exception
        {
            return Policy.Handle(condition)
                .WaitAndRetryAsync(
                    retryCount: retryCount,
                    sleepDurationProvider: (retryAttempt, context) => OnDurationSetFunc(retryAttempt, context, backOffPower, retryCount),
                    onRetry: (result, timeSpan, retryAttempt, context) => OnRetryFunc(result, timeSpan, retryAttempt, context, retryCount))
                .WithPolicyKey(policyName);
        }

        /// <summary>
        /// Gets <see cref="ISyncPolicy"/> type of WaitAndRetry.
        /// </summary>
        /// <typeparam name="TException">The type of the exception to be used within the condition.</typeparam>
        /// <param name="condition">The condition for the policy handling the request.</param>
        /// <param name="retryCount">The retry count for the policy.</param>
        /// <param name="backOffPower">The back off power to be used to calculate the next timeout.</param>
        /// <param name="policyName">The name of the policy.</param>
        /// <returns><see cref="ISyncPolicy"/>.</returns>
        public static ISyncPolicy GetWaitAndRetry<TException>(Func<TException, bool> condition, int retryCount, int backOffPower, string policyName)
              where TException : Exception
        {
            return Policy.Handle(condition)
                .WaitAndRetry(
                    retryCount: retryCount,
                    sleepDurationProvider: (retryAttempt, contex) => OnDurationSetFunc(retryAttempt, contex, backOffPower, retryCount),
                    onRetry: (result, timeSpan, retryAttempt, context) => OnRetryFunc(result, timeSpan, retryAttempt, context, retryCount))
                .WithPolicyKey(policyName);
        }

        /// <summary>
        /// Gets <see cref="IAsyncPolicy"/> type of RetryAsync.
        /// </summary>
        /// <typeparam name="TException">The type of the exception to be used within the condition.</typeparam>
        /// <param name="condition">The condition for the policy handling the request.</param>
        /// <param name="retryCount">The retry count for the policy.</param>
        /// <param name="policyName">The name of the policy.</param>
        /// <returns><see cref="IAsyncPolicy"/>.</returns>
        public static IAsyncPolicy GetRetryAsync<TException>(Func<TException, bool> condition, int retryCount, string policyName)
            where TException : Exception
        {
            return Policy.Handle(condition).RetryAsync(
                 retryCount: retryCount,
                 onRetry: (result, retryAttempt, context) => OnRetryFunc(result, retryAttempt, context, retryCount))
                 .WithPolicyKey(policyName);
        }

        /// <summary>
        /// Gets <see cref="ISyncPolicy"/> type of Retry.
        /// </summary>
        /// <typeparam name="TException">The type of the exception to be used within the condition.</typeparam>
        /// <param name="condition">The condition for the policy handling the request.</param>
        /// <param name="retryCount">The retry count for the policy.</param>
        /// <param name="policyName">The name of the policy.</param>
        /// <returns><see cref="ISyncPolicy"/>.</returns>
        public static ISyncPolicy GetRetry<TException>(Func<TException, bool> condition, int retryCount, string policyName)
            where TException : Exception
        {
            return Policy.Handle(condition)
                .Retry(
                    retryCount: retryCount,
                    onRetry: (result, retryAttempt, context) => OnRetryFunc(result, retryAttempt, context, retryCount))
                .WithPolicyKey(policyName);
        }

        public static IAsyncPolicy GetWaitAndRetryForeverAsync<TException>(Func<TException, bool> condition, TimeSpan attemptTimeout, string policyName)
            where TException : Exception
        {
            return Policy.Handle(condition)
                .WaitAndRetryForeverAsync(
                    sleepDurationProvider: (retryAttempt, context) =>
                        {
                            context.TryGetLogger(out var logger);
                            context.TryGetActionName(out var actionName);

                            Logger.OnDurationSetInformation(logger, actionName, context.PolicyKey, attemptTimeout, retryAttempt, -1);
                            return attemptTimeout;
                        },
                    onRetry: (exception, attempt, delay, context) => { })
                .WithPolicyKey(policyName);
        }

        private static void OnRetryFunc(Exception result, int retryAttempt, Context context, int retryCount)
        {
            context.TryGetLogger(out var logger);

            Logger.OnRetryError(logger, context.PolicyKey, retryAttempt, retryCount, result?.GetBaseException());
        }

        private static TimeSpan OnDurationSetFunc(int retryAttempt, Context context, int backOffPower, int retryCount)
        {
            context.TryGetLogger(out var logger);
            context.TryGetActionName(out var actionName);

            var timeSpan = TimeSpan.FromSeconds(Math.Pow(backOffPower, retryAttempt));

            Logger.OnDurationSetInformation(logger, actionName, context.PolicyKey, timeSpan, retryAttempt, retryCount);

            return timeSpan;
        }

        private static void OnRetryFunc(Exception result, TimeSpan timeSpan, int retryAttempt, Context context, int retryCount)
        {
            context.TryGetLogger(out var logger);
            context.TryGetActionName(out var actionName);

            Logger.OnRetryInformation(logger, actionName, context.PolicyKey, timeSpan, retryAttempt, retryCount, result?.GetBaseException());

            if (retryAttempt != retryCount)
            {
                return;
            }

            Logger.OnRetryError(logger, context.PolicyKey, retryAttempt, retryCount, result?.GetBaseException());
        }

        private static class EventIds
        {
            public static readonly EventId OnRetry = new EventId(100, nameof(OnRetry));
            public static readonly EventId OnDurationSet = new EventId(101, nameof(OnDurationSet));
        }

        private static class Logger
        {
            private static readonly Action<ILogger, string, int, int, Exception> OnRetryErrorInternal =
                LoggerMessage.Define<string, int, int>(
                LogLevel.Error,
                EventIds.OnRetry,
                "#Polly #WaitAndRetryAsync executing OnRetry with Policy: {PolicyKey} and Retries: {retryAttempt} of {retryCount}");

            private static readonly Action<ILogger, string, string, TimeSpan, int, int, Exception> OnRetryInformationInternal =
               LoggerMessage.Define<string, string, TimeSpan, int, int>(
               LogLevel.Information,
               EventIds.OnRetry,
               "#Polly #WaitAndRetryAsync executing {ActionName} OnRetry with Policy: {PolicyKey} and Wait Timespan: {timeSpan} Retries: {retryAttempt} of {retryCount}.");

            private static readonly Action<ILogger, string, string, TimeSpan, int, int, Exception> OnDurationSetInformationInternal =
                LoggerMessage.Define<string, string, TimeSpan, int, int>(
                    LogLevel.Information,
                    EventIds.OnDurationSet,
                    "#Polly #WaitAndRetryAsync executing {ActionName} OnDurationSet with Policy: {PolicyKey} timespan: {timeSpan} Retries: {retryAttempt} of {retryCount}");

            internal static void OnDurationSetInformation(ILogger logger, string actionName, string policyKey, TimeSpan timeSpan, int retryAttempt, int retryCount)
            {
                if (logger != null)
                {
                    OnDurationSetInformationInternal(logger, actionName, policyKey, timeSpan, retryAttempt, retryCount, null);
                }
            }

            internal static void OnRetryError(ILogger logger, string policyKey, int retryAttempt, int retryCount, Exception exception)
            {
                if (logger != null)
                {
                    OnRetryErrorInternal(logger, policyKey, retryAttempt, retryCount, exception);
                }
            }

            internal static void OnRetryInformation(ILogger logger, string actionName, string policyKey, TimeSpan timeSpan, int retryAttempt, int retryCount, Exception exception)
            {
                if (logger != null)
                {
                    OnRetryInformationInternal(logger, actionName, policyKey, timeSpan, retryAttempt, retryCount, exception);
                }
            }
        }
    }
}
