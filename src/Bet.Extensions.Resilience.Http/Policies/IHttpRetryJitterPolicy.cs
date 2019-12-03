using System.Net.Http;

using Bet.Extensions.Resilience.Abstractions.Policies;
using Bet.Extensions.Resilience.Http.Options;

namespace Bet.Extensions.Resilience.Http.Policies
{
    public interface IHttpRetryJitterPolicy : IPolicy<HttpRetryJitterPolicyOptions, HttpResponseMessage>
    {
    }
}
