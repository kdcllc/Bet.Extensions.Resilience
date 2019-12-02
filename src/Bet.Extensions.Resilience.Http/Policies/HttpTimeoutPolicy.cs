using System.Net.Http;

using Bet.Extensions.Resilience.Abstractions;
using Bet.Extensions.Resilience.Abstractions.Options;
using Bet.Extensions.Resilience.Abstractions.Policies;
using Bet.Extensions.Resilience.Http.Options;

using Microsoft.Extensions.Logging;

namespace Bet.Extensions.Resilience.Http.Policies
{
    public class HttpTimeoutPolicy :
        TimeoutPolicy<HttpTimeoutPolicyOptions, HttpResponseMessage>,
        IHttpTimeoutPolicy
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HttpTimeoutPolicy"/> class.
        /// </summary>
        /// <param name="policyOptions"></param>
        /// <param name="policyOptionsConfigurator"></param>
        /// <param name="registryConfigurator"></param>
        /// <param name="logger"></param>
        public HttpTimeoutPolicy(
            PolicyOptions policyOptions,
            IPolicyOptionsConfigurator<HttpTimeoutPolicyOptions> policyOptionsConfigurator,
            IPolicyRegistryConfigurator registryConfigurator,
            ILogger<IPolicy<HttpTimeoutPolicyOptions, HttpResponseMessage>> logger) : base(policyOptions, policyOptionsConfigurator, registryConfigurator, logger)
        {
        }
    }
}
