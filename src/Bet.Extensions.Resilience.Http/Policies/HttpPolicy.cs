using System;
using System.Net.Http;

using Bet.Extensions.Resilience.Abstractions;
using Bet.Extensions.Resilience.Abstractions.Options;
using Bet.Extensions.Resilience.Abstractions.Policies;
using Bet.Extensions.Resilience.Http.Options;

using Microsoft.Extensions.Logging;

using Polly;

namespace Bet.Extensions.Resilience.Http.Policies
{
    public class HttpPolicy : BasePolicy<HttpPolicyOptions, HttpResponseMessage>, IHttpPolicy
    {
        public HttpPolicy(
            PolicyOptions policyOptions,
            IServiceProvider serviceProvider,
            IPolicyOptionsConfigurator<HttpPolicyOptions> policyOptionsConfigurator,
            IPolicyRegistryConfigurator registryConfigurator,
            ILogger<IPolicy<HttpPolicyOptions, HttpResponseMessage>> logger) : base(
                policyOptions,
                serviceProvider,
                policyOptionsConfigurator,
                registryConfigurator,
                logger)
        {
        }

        public override IAsyncPolicy<HttpResponseMessage> GetAsyncPolicy()
        {
            throw new NotImplementedException();
        }

        public override ISyncPolicy<HttpResponseMessage> GetSyncPolicy()
        {
            throw new NotImplementedException();
        }
    }
}
