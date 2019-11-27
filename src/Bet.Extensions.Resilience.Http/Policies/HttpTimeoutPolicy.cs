using System.Net.Http;

using Bet.Extensions.Resilience.Abstractions;
using Bet.Extensions.Resilience.Abstractions.Options;
using Bet.Extensions.Resilience.Abstractions.Policies;

using Microsoft.Extensions.Logging;

namespace Bet.Extensions.Resilience.Http.Policies
{
    public class HttpTimeoutPolicy<TOptions, TResult> :
        TimeoutPolicy<TOptions, HttpResponseMessage>,
        IHttpTimeoutPolicy<TOptions, HttpResponseMessage> where TOptions : TimeoutPolicyOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HttpTimeoutPolicy{TOptions, TResult}"/> class.
        /// </summary>
        /// <param name="policyOptions"></param>
        /// <param name="policyOptionsConfigurator"></param>
        /// <param name="registryConfigurator"></param>
        /// <param name="logger"></param>
        public HttpTimeoutPolicy(
            PolicyOptions policyOptions,
            IPolicyOptionsConfigurator<TOptions> policyOptionsConfigurator,
            IPolicyRegistryConfigurator registryConfigurator,
            ILogger<IPolicy<TOptions>> logger) : base(policyOptions, policyOptionsConfigurator, registryConfigurator, logger)
        {
        }
    }
}
