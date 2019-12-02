using System.Net.Http;

using Bet.Extensions.Resilience.Abstractions.Policies;
using Bet.Extensions.Resilience.Http.Options;

namespace Bet.Extensions.Resilience.Http.Policies
{
    public interface IHttpCircuitBreakerPolicy :
        IPolicy<HttpCircuitBreakerPolicyOptions, HttpResponseMessage>
    {
    }
}
