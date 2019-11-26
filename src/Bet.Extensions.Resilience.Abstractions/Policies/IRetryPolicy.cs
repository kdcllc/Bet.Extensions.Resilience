using Bet.Extensions.Resilience.Abstractions.Options;

namespace Bet.Extensions.Resilience.Abstractions.Policies
{
    public interface IRetryPolicy<TOptions> : IPolicy<TOptions> where TOptions : PolicyOptions
    {
    }

    public interface IRetryPolicy<TOptions, TResult> : IPolicy<TOptions, TResult> where TOptions : PolicyOptions
    {
    }
}
