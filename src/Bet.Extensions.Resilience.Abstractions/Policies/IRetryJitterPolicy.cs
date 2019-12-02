using Bet.Extensions.Resilience.Abstractions.Options;

namespace Bet.Extensions.Resilience.Abstractions.Policies
{
    public interface IRetryJitterPolicy : IPolicy<RetryJitterPolicyOptions>
    {
    }

    public interface IRetryJitterPolicy<TResult> : IPolicy<RetryJitterPolicyOptions, TResult>
    {
    }
}
