using System.Net.Http;

using Bet.Extensions.Resilience.Abstractions.Options;
using Bet.Extensions.Resilience.Abstractions.Policies;

namespace Bet.Extensions.Resilience.Http.Policies
{
    public interface IHttpTimeoutPolicy<TOptions, TResult> :
        IPolicy<TOptions, TResult> where TOptions : PolicyOptions where TResult : HttpResponseMessage
    {
    }
}
