using System;

using Microsoft.Extensions.Logging;

using Polly;

namespace Bet.Extensions.Resilience.Abstractions.Policies
{
    public static class PolicieBuilder
    {
        /// <summary>
        /// Gets <see cref="IAsyncPolicy"/> type of WaitAndRetryAsync.
        /// </summary>
        /// <typeparam name="TException">The type of the exception to be used within the condition.</typeparam>
        /// <param name="condition">The condition for the policy handling the request.</param>
        /// <param name="retryCount">The retry count for the policy.</param>
        /// <param name="backOffPower">The back off power to be used to calculate the next timeout.</param>
        /// <param name="policyName">The name of the policy.</param>
        /// <returns></returns>
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

        public static ISyncPolicy GetWaitAndRetry<TException>(Func<TException,bool> condition, int retryCount, int backOffPower, string policyName)
              where TException : Exception
        {
            return Policy.Handle(condition)
                .WaitAndRetry(
                    retryCount: retryCount,
                    sleepDurationProvider: (retryAttempt, contex) => OnDurationSetFunc(retryAttempt, contex, backOffPower, retryCount),
                    onRetry: (result, timeSpan, retryAttempt, context) => OnRetryFunc(result, timeSpan, retryAttempt, context, retryCount))
                .WithPolicyKey(policyName);
        }

        private static TimeSpan OnDurationSetFunc(int retryAttempt, Context context, int backOffPower, int retryCount)
        {
            context.TryGetLogger(out var logger);

            var timeSpan = TimeSpan.FromSeconds(Math.Pow(backOffPower, retryAttempt));

            Logger.OnDurationSetInformation(logger, context.PolicyKey, timeSpan, retryAttempt, retryCount);

            return timeSpan;
        }

        private static void OnRetryFunc(Exception result, TimeSpan timeSpan, int retryAttempt, Context context, int retryCount)
        {
            context.TryGetLogger(out var logger);
            Logger.OnRetryInformation(logger, context.PolicyKey, timeSpan, retryAttempt, retryCount, result?.GetBaseException());

            if (retryAttempt != retryCount)
            {
                return;
            }

            Logger.OnRetryError(logger, context.PolicyKey, retryAttempt, retryCount, result?.GetBaseException());
        }

        public static class EventIds
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

            private static readonly Action<ILogger, string, TimeSpan, int,int, Exception> OnRetryInformationInternal =
               LoggerMessage.Define<string, TimeSpan, int, int>(
               LogLevel.Information,
               EventIds.OnRetry,
               "#Polly #WaitAndRetryAsync executing OnRetry with Policy: {PolicyKey} and Wait Timespan: {timeSpan} Retries: {retryAttempt} of {retryCount}.");

            private static readonly Action<ILogger, string, TimeSpan, int, int, Exception> OnDurationSetInformationInternal =
                LoggerMessage.Define<string, TimeSpan, int, int>(
                    LogLevel.Information,
                    EventIds.OnDurationSet,
                    "#Polly #WaitAndRetryAsync executing OnDurationSet with Policy: {PolicyKey} timespan: {timeSpan} Retries: {retryAttempt} of {retryCount}");

            internal static void OnDurationSetInformation(ILogger logger, string policyKey, TimeSpan timeSpan, int retryAttempt, int retryCount)
            {
               if (logger != null)
               {
                    OnDurationSetInformationInternal(logger, policyKey, timeSpan, retryAttempt, retryCount, null);
               }
            }

            internal static void OnRetryError(ILogger logger, string policyKey, int retryAttempt, int retryCount, Exception exception)
            {
                if (logger != null)
                {
                    OnRetryErrorInternal(logger, policyKey, retryAttempt, retryCount, exception);
                }
            }

            internal static void OnRetryInformation(ILogger logger, string policyKey, TimeSpan timeSpan, int retryAttempt, int retryCount, Exception exception)
            {
                if (logger != null)
                {
                    OnRetryInformationInternal(logger, policyKey, timeSpan, retryAttempt, retryCount, exception);
                }
            }
        }
    }

}
