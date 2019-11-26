using Bet.Extensions.Resilience.Abstractions.Options;

namespace Bet.Extensions.Resilience.Abstractions.Policies
{
    public interface IBulkheadPolicy<TOptions> : IPolicy<TOptions> where TOptions : PolicyOptions
    {
    }

    public interface IBulkheadPolicy<TOptions, TResult> : IPolicy<TOptions, TResult> where TOptions : PolicyOptions
    {
    }
}
