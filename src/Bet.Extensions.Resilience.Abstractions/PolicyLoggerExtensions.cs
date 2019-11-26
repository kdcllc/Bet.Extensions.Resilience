using System;
using System.Threading.Tasks;

using Bet.Extensions.Resilience.Abstractions.Options;

using Microsoft.Extensions.Logging;

namespace Polly
{
    public static class PolicyLoggerExtensions
    {
        public static void LogCircuitBreakerOnBreak<T>(this ILogger<T> logger, string message, TimeSpan time, Context context)
        {
            logger.LogWarning(
                "[CircuitBreak Policy][OnBreak] using operation key: {operationKey}; correlation ID: {id}; duration of the break: {time}; reason: {outcome}",
                context.OperationKey,
                context.CorrelationId,
                time,
                message);
        }

        public static void LogCircuitBreakerOnReset<T>(this ILogger<T> logger, Context context)
        {
            logger.LogInformation(
                "[CircuitBreak Policy][OnReset] using operation key: {operationKey}; correlation ID: {id}",
                context.OperationKey,
                context.CorrelationId);
        }

        public static void LogRetryOnDuration<T>(this ILogger<T> logger, int retryAttempt, string message, Context context, RetryPolicyOptions options)
        {
            logger.LogInformation(
                "[Retry Policy][OnDuration] using operation key: {operationKey}; correlation ID: {id}; retry {retryNumber} of {totalRetries}; exception: {exception}",
                context.OperationKey,
                context.CorrelationId,
                retryAttempt,
                options.Count,
                message);
        }

        public static Task LogRetryOnRetry<T>(this ILogger<T> logger, string message, TimeSpan time, int retryAttempt, Context context, int retryCount)
        {
            logger.LogWarning(
                "[Retry Policy][OnRetry] using operation key: {operationKey}; correlation ID: {id}; time elapsed: {time}; retry {retryAttempt} of {retryCount}; exception: {exception}",
                context.OperationKey,
                context.CorrelationId,
                time,
                retryAttempt,
                retryCount,
                message);

            return Task.CompletedTask;
        }

        public static void LogOnTimeout<T>(this ILogger<T> logger, Context context, TimeSpan time)
        {
            logger.LogError(
                "[Timeout Policy][Timed out] using operation key: {operationKey}; correlation ID: {correlationId}; timed out after: {totalMilliseconds}",
                context.OperationKey,
                context.CorrelationId,
                time.TotalMilliseconds);
        }

        public static void LogOnBulkheadRejected<T>(this ILogger<T> logger, Context context)
        {
            logger.LogError(
                "[Bulkhead Policy][Rejected] using policy key: {PolicyKey}; operation key: {OperationKey}; correlation id: {CorrelationId}",
                context.PolicyKey,
                context.OperationKey,
                context.CorrelationId);
        }
    }
}
