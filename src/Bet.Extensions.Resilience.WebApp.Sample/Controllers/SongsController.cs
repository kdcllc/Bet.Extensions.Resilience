using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

using Bet.Extensions.Resilience.Abstractions;
using Bet.Extensions.Resilience.Abstractions.Options;
using Bet.Extensions.Resilience.Http.Options;
using Bet.Extensions.Resilience.WebApp.Sample.Clients;
using Bet.Extensions.Resilience.WebApp.Sample.Clients.Models;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

using Polly.CircuitBreaker;

namespace Bet.Extensions.Resilience.WebApp.Sample.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SongsController : ControllerBase
    {
        private readonly IChavahClient _chavahClient;
        private readonly IServiceProvider _provider;
        private readonly PolicyBucket<AsyncCircuitBreakerPolicy<HttpResponseMessage>, CircuitBreakerPolicyOptions> _policyBucket;

        public SongsController(
            IChavahClient chavahClient,
            IServiceProvider provider,
            PolicyBucket<AsyncCircuitBreakerPolicy<HttpResponseMessage>, CircuitBreakerPolicyOptions> policyBucket)
        {
            _chavahClient = chavahClient ?? throw new ArgumentNullException(nameof(chavahClient));
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
            _policyBucket = policyBucket ?? throw new ArgumentNullException(nameof(policyBucket));
        }

        [HttpGet]
        public async Task<IEnumerable<Song>> Get(int count = 3)
        {
            // testing the configuration of policies
            var policy = _policyBucket.GetPolicy(HttpPolicyOptionsKeys.HttpCircuitBreakerPolicy);

            var services = _provider.GetServices<PolicyBucket<AsyncCircuitBreakerPolicy<HttpResponseMessage>, CircuitBreakerPolicyOptions>>();

            var result = await _chavahClient.GetPopular(count);

            return result;
        }
    }
}
