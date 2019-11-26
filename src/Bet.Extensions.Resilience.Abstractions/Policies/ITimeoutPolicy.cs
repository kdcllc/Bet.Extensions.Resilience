using Bet.Extensions.Resilience.Abstractions.Options;

namespace Bet.Extensions.Resilience.Abstractions.Policies
{
    public interface ITimeoutPolicy<TOptions> : IPolicy<TOptions> where TOptions : PolicyOptions
    {
    }

    public interface ITimeoutPolicy<TOptions, TResult> : IPolicy<TOptions, TResult> where TOptions : PolicyOptions
    {
    }
}
