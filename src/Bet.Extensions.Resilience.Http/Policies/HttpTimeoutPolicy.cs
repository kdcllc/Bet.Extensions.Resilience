using System;
using System.Net.Http;
using System.Threading.Tasks;

using Bet.Extensions.Resilience.Abstractions;
using Bet.Extensions.Resilience.Abstractions.Options;
using Bet.Extensions.Resilience.Abstractions.Policies;

using Microsoft.Extensions.Logging;

using Polly;
using Polly.Timeout;

namespace Bet.Extensions.Resilience.Http.Policies
{
    public class HttpTimeoutPolicy<TOptions, TResult> :
        BasePolicy<TOptions, HttpResponseMessage>,
        IHttpTimeoutPolicy<TOptions, HttpResponseMessage> where TOptions : TimeoutPolicyOptions
    {
        private readonly ILogger<IPolicy<TOptions>> _logger;

        public HttpTimeoutPolicy(
            PolicyOptions policyOptions,
            IPolicyOptionsConfigurator<TOptions> policyOptionsConfigurator,
            IPolicyRegistryConfigurator registryConfigurator,
            ILogger<IPolicy<TOptions>> logger) : base(policyOptions, policyOptionsConfigurator, registryConfigurator, logger)
        {
            _logger = logger;
        }

        /// <inheritdoc/>
        public override IAsyncPolicy<HttpResponseMessage> GetAsyncPolicy()
        {
            return Policy
                .TimeoutAsync(
                    Options.Timeout,
                    TimeoutStrategy.Pessimistic,
                    OnTimeoutAsync)
                .AsAsyncPolicy<HttpResponseMessage>();

            Task OnTimeoutAsync(Context context, TimeSpan span, Task task, Exception exception)
            {
                _logger.LogInformation(
                    "[Timeout Policy] OperationKey: {OperationKey}; CorrelationId: {CorrelationId}; Timed out after:: {TotalMilliseconds}",
                    context.OperationKey,
                    context.CorrelationId,
                    span.TotalMilliseconds);
                return Task.CompletedTask;
            }
        }

        /// <inheritdoc/>
        public override ISyncPolicy<HttpResponseMessage> GetSyncPolicy()
        {
            return Policy
                .Timeout(
                    Options.Timeout,
                    TimeoutStrategy.Pessimistic,
                    OnTimeout)
                .AsPolicy<HttpResponseMessage>();

            void OnTimeout(Context context, TimeSpan span, Task task, Exception exception)
            {
                _logger.LogInformation(
                    "[Timeout Policy] OperationKey: {OperationKey}; CorrelationId: {CorrelationId}; Timed out after:: {TotalMilliseconds}",
                    context.OperationKey,
                    context.CorrelationId,
                    span.TotalMilliseconds);
            }
        }
    }
}
