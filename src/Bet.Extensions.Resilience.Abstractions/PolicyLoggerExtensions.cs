using Bet.Extensions.Resilience.Abstractions.Options;

using Microsoft.Extensions.Logging;

using Polly.CircuitBreaker;

namespace Polly;

public static class PolicyLoggerExtensions
{
    /// <summary>
    /// Logs message for <see cref="Timeout.TimeoutPolicy"/>.
    /// </summary>
    /// <typeparam name="T">The type of the logger.</typeparam>
    /// <param name="logger">The logger.</param>
    /// <param name="context">The polly context.</param>
    /// <param name="time">The timeout that expired.</param>
    /// <param name="message">The message to log.</param>
    public static void LogOnTimeout<T>(this ILogger<T> logger, Context context, TimeSpan time, string message)
    {
        logger.LogError(
            "[{timedoutPolicy}][Timed out] using " +
            "operation key: {operationKey}; " +
            "correlation id: {correlationId}; " +
            "timed out after: {totalMilliseconds}; " +
            "reason: {message}",
            context.PolicyKey,
            context.OperationKey,
            context.CorrelationId,
            time.TotalMilliseconds,
            message);
    }

    /// <summary>
    /// Logs message for <see cref="Timeout.TimeoutPolicy"/>.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="context">The polly context.</param>
    /// <param name="time">The timeout that expired.</param>
    /// <param name="message">The message to log.</param>
    public static void LogOnTimeout(this ILogger logger, Context context, TimeSpan time, string message)
    {
        logger.LogError(
            "[{timedoutPolicy}][Timed out] using " +
            "operation key: {operationKey}; " +
            "correlation id: {correlationId}; " +
            "timed out after: {totalMilliseconds}; " +
            "reason: {message}",
            context.PolicyKey,
            context.OperationKey,
            context.CorrelationId,
            time.TotalMilliseconds,
            message);
    }

    /// <summary>
    /// Logs message for <see cref="Fallback.FallbackPolicy"/>.
    /// </summary>
    /// <typeparam name="T">The type of the logger.</typeparam>
    /// <param name="logger">The logger.</param>
    /// <param name="context">The polly context.</param>
    /// <param name="message">The message to log.</param>
    public static void LogOnFallabck<T>(this ILogger<T> logger, Context context, string message)
    {
        logger.LogError(
            "[{fallbackPolicy}][Fallback] using " +
            "operation key: {operationKey}; " +
            "correlation id: {correlationId}; " +
            "reason: {message}; ",
            context.PolicyKey,
            context.OperationKey,
            context.CorrelationId,
            message);
    }

    /// <summary>
    /// Logs message for <see cref="Fallback.FallbackPolicy"/>.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="context">The polly context.</param>
    /// <param name="message">The message to log.</param>
    public static void LogOnFallabck(this ILogger logger, Context context, string message)
    {
        logger.LogError(
            "[{fallbackPolicy}][Fallback] using " +
            "operation key: {operationKey}; " +
            "correlation id: {correlationId}; " +
            "reason: {message}; ",
            context.PolicyKey,
            context.OperationKey,
            context.CorrelationId,
            message);
    }

    /// <summary>
    /// Logs message for <see cref="Bulkhead.BulkheadPolicy"/>.
    /// </summary>
    /// <typeparam name="T">The type of the logger.</typeparam>
    /// <param name="logger">The logger.</param>
    /// <param name="context">The polly context.</param>
    /// <param name="options">The options.</param>
    public static void LogOnBulkheadRejected<T>(this ILogger<T> logger, Context context, BulkheadPolicyOptions options)
    {
        logger.LogError(
            "[{bulkheadPolicy}][Rejected] using policy " +
            "operation key: {operationKey}; " +
            "correlation id: {correlationId} " +
            "MaxParallelization: {maxParallelization} " +
            "MaxQueuedItems: {maxQueuedItems}",
            context.PolicyKey,
            context.OperationKey,
            context.CorrelationId,
            options.MaxParallelization,
            options.MaxQueuedItems);
    }

    /// <summary>
    /// Logs message for <see cref="Bulkhead.BulkheadPolicy"/>.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="context">The polly context.</param>
    /// <param name="options">The options.</param>
    public static void LogOnBulkheadRejected(this ILogger logger, Context context, BulkheadPolicyOptions options)
    {
        logger.LogError(
            "[{bulkheadPolicy}][Rejected] using policy " +
            "operation key: {operationKey}; " +
            "correlation id: {correlationId} " +
            "MaxParallelization: {maxParallelization} " +
            "MaxQueuedItems: {maxQueuedItems}",
            context.PolicyKey,
            context.OperationKey,
            context.CorrelationId,
            options.MaxParallelization,
            options.MaxQueuedItems);
    }

    /// <summary>
    /// Logs message for <see cref="CircuitBreakerPolicy"/>.
    /// </summary>
    /// <typeparam name="T">The type of the logger.</typeparam>
    /// <param name="logger">The logger.</param>
    /// <param name="time">The time of the break.</param>
    /// <param name="context">The polly context.</param>
    /// <param name="circuitState">The circuit state.</param>
    /// <param name="options">The options for the policy.</param>
    /// <param name="message">The message to log.</param>
    public static void LogCircuitBreakerOnBreak<T>(
        this ILogger<T> logger,
        TimeSpan time,
        Context context,
        CircuitState circuitState,
        CircuitBreakerPolicyOptions options,
        string message)
    {
        logger.LogWarning(
            "[{circuitBreakerPolicy}][OnBreak] using " +
            "operation key: {operationKey}; " +
            "correlation id: {id}; " +
            "duration of the break: {time}; " +
            "exceptions allowed before breaking: {allowedExceptions} " +
            "circuit state: {circuitState}; " +
            "reason: {message}",
            context.PolicyKey,
            context.OperationKey,
            context.CorrelationId,
            time,
            options.ExceptionsAllowedBeforeBreaking,
            circuitState,
            message);
    }

    /// <summary>
    /// Logs message for <see cref="CircuitBreakerPolicy"/>.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="time">The time of the break.</param>
    /// <param name="context">The polly context.</param>
    /// <param name="circuitState">The circuit state.</param>
    /// <param name="options">The options for the policy.</param>
    /// <param name="message">The message to log.</param>
    public static void LogCircuitBreakerOnBreak(
        this ILogger logger,
        TimeSpan time,
        Context context,
        CircuitState circuitState,
        CircuitBreakerPolicyOptions options,
        string message)
    {
        logger.LogWarning(
            "[{circuitBreakerPolicy}][OnBreak] using " +
            "operation key: {operationKey}; " +
            "correlation id: {id}; " +
            "duration of the break: {time}; " +
            "exceptions allowed before breaking: {allowedExceptions} " +
            "circuit state: {circuitState}; " +
            "reason: {message}",
            context.PolicyKey,
            context.OperationKey,
            context.CorrelationId,
            time,
            options.ExceptionsAllowedBeforeBreaking,
            circuitState,
            message);
    }

    /// <summary>
    /// Logs message for <see cref="CircuitBreakerPolicy"/>.
    /// </summary>
    /// <typeparam name="T">The type of the logger.</typeparam>
    /// <param name="logger">The logger.</param>
    /// <param name="context">The polly context.</param>
    /// <param name="options">The policy options.</param>
    public static void LogCircuitBreakerOnReset<T>(this ILogger<T> logger, Context context, CircuitBreakerPolicyOptions options)
    {
        logger.LogInformation(
            "[{circuitBreakerPolicy}][OnReset] using " +
            "operation key: {operationKey}; " +
            "correlation id: {id}" +
            "exceptions allowed before breaking: {allowedExceptions} ",
            context.PolicyKey,
            context.OperationKey,
            context.CorrelationId,
            options.ExceptionsAllowedBeforeBreaking);
    }

    /// <summary>
    /// Logs message for <see cref="CircuitBreakerPolicy"/>.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="context">The polly context.</param>
    /// <param name="options">The policy options.</param>
    public static void LogCircuitBreakerOnReset(this ILogger logger, Context context, CircuitBreakerPolicyOptions options)
    {
        logger.LogInformation(
            "[{circuitBreakerPolicy}][OnReset] using " +
            "operation key: {operationKey}; " +
            "correlation id: {id}" +
            "exceptions allowed before breaking: {allowedExceptions} ",
            context.PolicyKey,
            context.OperationKey,
            context.CorrelationId,
            options.ExceptionsAllowedBeforeBreaking);
    }

    /// <summary>
    /// Logs message for <see cref="Retry.RetryPolicy"/>.
    /// </summary>
    /// <typeparam name="T">The type of the logger.</typeparam>
    /// <param name="logger">The logger.</param>
    /// <param name="retryAttempt">The current retry attempt count.</param>
    /// <param name="context">The polly context.</param>
    /// <param name="numberOfRetries">The total number of the retries possible.</param>
    /// <param name="message">The message to log.</param>
    public static void LogRetryOnDuration<T>(
        this ILogger<T> logger,
        int retryAttempt,
        Context context,
        int numberOfRetries,
        string message)
    {
        logger.LogInformation(
            "[{retryPolicy}][OnDuration] using " +
            "operation key: {operationKey}; " +
            "correlation id: {id}; " +
            "retry {retryNumber} of {totalRetries}; " +
            "reason: {message}",
            context.PolicyKey,
            context.OperationKey,
            context.CorrelationId,
            retryAttempt,
            numberOfRetries,
            message);
    }

    /// <summary>
    /// Logs message for <see cref="Retry.RetryPolicy"/>.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="retryAttempt">The current retry attempt count.</param>
    /// <param name="context">The polly context.</param>
    /// <param name="numberOfRetries">The total number of the retries possible.</param>
    /// <param name="message">The message to log.</param>
    public static void LogRetryOnDuration(
        this ILogger logger,
        int retryAttempt,
        Context context,
        int numberOfRetries,
        string message)
    {
        logger.LogInformation(
            "[{retryPolicy}][OnDuration] using " +
            "operation key: {operationKey}; " +
            "correlation id: {id}; " +
            "retry {retryNumber} of {totalRetries}; " +
            "reason: {message}",
            context.PolicyKey,
            context.OperationKey,
            context.CorrelationId,
            retryAttempt,
            numberOfRetries,
            message);
    }

    /// <summary>
    /// Logs message for <see cref="Retry.RetryPolicy"/>.
    /// </summary>
    /// <typeparam name="T">The type of the logger.</typeparam>
    /// <param name="logger">The logger.</param>
    /// <param name="time">The elapse time.</param>
    /// <param name="retryAttempt">The current retry attempt count.</param>
    /// <param name="context">The polly context.</param>
    /// <param name="numberOfRetries">The total number of the retries possible.</param>
    /// <param name="message">The message to log.</param>
    /// <returns></returns>
    public static Task LogRetryOnRetry<T>(
        this ILogger<T> logger,
        TimeSpan time,
        int retryAttempt,
        Context context,
        int numberOfRetries,
        string message)
    {
        logger.LogWarning(
            "[{retryPolicy}][OnRetry] using " +
            "operation key: {operationKey}; " +
            "correlation id: {id}; " +
            "time elapsed: {time}; " +
            "retry {retryAttempt} of {retryCount}; " +
            "reason: {message}",
            context.PolicyKey,
            context.OperationKey,
            context.CorrelationId,
            time,
            retryAttempt,
            numberOfRetries,
            message);

        return Task.CompletedTask;
    }

    /// <summary>
    /// Logs message for <see cref="Retry.RetryPolicy"/>.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="time">The elapse time.</param>
    /// <param name="retryAttempt">The current retry attempt count.</param>
    /// <param name="context">The polly context.</param>
    /// <param name="numberOfRetries">The total number of the retries possible.</param>
    /// <param name="message">The message to log.</param>
    /// <returns></returns>
    public static Task LogRetryOnRetry(
        this ILogger logger,
        TimeSpan time,
        int retryAttempt,
        Context context,
        int numberOfRetries,
        string message)
    {
        logger.LogWarning(
            "[{retryPolicy}][OnRetry] using " +
            "operation key: {operationKey}; " +
            "correlation id: {id}; " +
            "time elapsed: {time}; " +
            "retry {retryAttempt} of {retryCount}; " +
            "reason: {message}",
            context.PolicyKey,
            context.OperationKey,
            context.CorrelationId,
            time,
            retryAttempt,
            numberOfRetries,
            message);

        return Task.CompletedTask;
    }
}
