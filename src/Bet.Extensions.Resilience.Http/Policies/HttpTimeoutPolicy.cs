using System;
using System.Net.Http;
using System.Threading.Tasks;

using Bet.Extensions.Resilience.Http.Options;

using Microsoft.Extensions.Logging;

using Polly;
using Polly.Timeout;

namespace Bet.Extensions.Resilience.Http.Policies
{
    public class HttpTimeoutPolicy<TOptions> : BasePolicy<TOptions> where TOptions : HttpPolicyOptions
    {
        public HttpTimeoutPolicy(
            string policyName,
            IHttpPolicyConfigurator<TOptions> policyConfigurator,
            ILogger<IHttpPolicy<TOptions>> logger) : base(policyName, policyConfigurator, logger)
        {
        }

        /// <inheritdoc/>
        public override IAsyncPolicy<HttpResponseMessage> CreateAsyncPolicy()
        {
            return Policy
                .TimeoutAsync(
                    Options.Timeout,
                    TimeoutStrategy.Pessimistic,
                    OnTimeoutAsync)
                .AsAsyncPolicy<HttpResponseMessage>();

            Task OnTimeoutAsync(Context context, TimeSpan span, Task task, Exception exception)
            {
                Logger.LogInformation(
                    "[Timeout Policy] OperationKey: {OperationKey}; CorrelationId: {CorrelationId}; Timed out after:: {TotalMilliseconds}",
                    context.OperationKey,
                    context.CorrelationId,
                    span.TotalMilliseconds);
                return Task.CompletedTask;
            }
        }

        /// <inheritdoc/>
        public override ISyncPolicy<HttpResponseMessage> CreateSyncPolicy()
        {
            return Policy
                .Timeout(
                    Options.Timeout,
                    TimeoutStrategy.Pessimistic,
                    OnTimeout)
                .AsPolicy<HttpResponseMessage>();

            void OnTimeout(Context context, TimeSpan span, Task task, Exception exception)
            {
                Logger.LogInformation(
                    "[Timeout Policy] OperationKey: {OperationKey}; CorrelationId: {CorrelationId}; Timed out after:: {TotalMilliseconds}",
                    context.OperationKey,
                    context.CorrelationId,
                    span.TotalMilliseconds);
            }
        }
    }
}
