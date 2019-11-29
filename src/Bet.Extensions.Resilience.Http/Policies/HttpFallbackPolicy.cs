using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

using Bet.Extensions.Resilience.Abstractions;
using Bet.Extensions.Resilience.Abstractions.Options;
using Bet.Extensions.Resilience.Abstractions.Policies;
using Bet.Extensions.Resilience.Http.Options;

using Microsoft.Extensions.Logging;

using Polly;

namespace Bet.Extensions.Resilience.Http.Policies
{
    public class HttpFallbackPolicy<TOptions>
        : FallbackPolicy<TOptions, HttpResponseMessage>,
        IHttpFallbackPolicy<TOptions> where TOptions : HttpFallbackPolicyOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HttpFallbackPolicy{TOptions}"/> class.
        /// </summary>
        /// <param name="policyOptions"></param>
        /// <param name="policyOptionsConfigurator"></param>
        /// <param name="registryConfigurator"></param>
        /// <param name="logger"></param>
        public HttpFallbackPolicy(
            PolicyOptions policyOptions,
            IPolicyOptionsConfigurator<TOptions> policyOptionsConfigurator,
            IPolicyRegistryConfigurator registryConfigurator,
            ILogger<IPolicy<TOptions>> logger) : base(policyOptions, policyOptionsConfigurator, registryConfigurator, logger)
        {
        }

        public override IAsyncPolicy<HttpResponseMessage> GetAsyncPolicy()
        {
            FallbackActionAsync = (logger, options) =>
            {
                return (ex, context, token) =>
                {
                    // can be adding anything else ...
                    return Task.FromResult(new HttpResponseMessage((HttpStatusCode)options.StatusCode) { ReasonPhrase = options.Message });
                };
            };

            OnFallbackAsync = (logger, options) =>
            {
                return (result, context) =>
                {
                    // refactor logger and options
                    var x = options.Message;

                    logger.LogOnFallabck(context, result.GetMessage());

                    return Task.CompletedTask;
                };
            };

            // execute policy.
            return base.GetAsyncPolicy();
        }

        public override ISyncPolicy<HttpResponseMessage> GetSyncPolicy()
        {
            FallbackAction = (logger, options) =>
            {
                return (result, context, token) =>
                {
                    // can be adding anything else ...
                    return new HttpResponseMessage((HttpStatusCode)options.StatusCode) { ReasonPhrase = options.Message };
                };
            };

            OnFallback = (logger, options) =>
            {
                return (result, context) =>
                {
                    logger.LogOnFallabck(context, result.GetMessage());
                };
            };

            return base.GetSyncPolicy();
        }
    }
}
