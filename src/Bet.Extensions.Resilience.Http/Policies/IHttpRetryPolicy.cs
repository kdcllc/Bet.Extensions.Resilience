using System.Net.Http;

using Bet.Extensions.Resilience.Abstractions.Options;
using Bet.Extensions.Resilience.Abstractions.Policies;

namespace Bet.Extensions.Resilience.Http.Policies
{
    public interface IHttpRetryPolicy<TOptions> :
        IPolicy<TOptions, HttpResponseMessage> where TOptions : PolicyOptions
    {
    }
}
