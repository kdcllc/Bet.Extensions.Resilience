using Bet.Extensions.Resilience.Abstractions.Options;

namespace Bet.Extensions.Resilience.Abstractions.Policies
{
    public interface ICircuitBreakerPolicy<TOptions> : IPolicy<TOptions> where TOptions : PolicyOptions
    {
    }

    public interface ICircuitBreakerPolicy<TOptions, TResult> : IPolicy<TOptions, TResult> where TOptions : PolicyOptions
    {
    }
}
