using Bet.Extensions.Resilience.Abstractions.Options;

namespace Bet.Extensions.Resilience.Abstractions.Policies
{
    public interface IFallbackPolicy<TOptions> : IPolicy<TOptions> where TOptions : PolicyOptions
    {
    }

    public interface IFallbackPolicy<TOptions, TResult> : IPolicy<TOptions, TResult> where TOptions : PolicyOptions
    {
    }
}
