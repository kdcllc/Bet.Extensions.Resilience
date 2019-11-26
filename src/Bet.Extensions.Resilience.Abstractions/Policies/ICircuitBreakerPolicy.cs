using System;
using Bet.Extensions.Resilience.Abstractions.Options;
using Polly;

namespace Bet.Extensions.Resilience.Abstractions.Policies
{
    public interface ICircuitBreakerPolicy<TOptions> : IPolicy<TOptions> where TOptions : PolicyOptions
    {
    }

    public interface ICircuitBreakerPolicy<TOptions, TResult> : IPolicy<TOptions, TResult> where TOptions : PolicyOptions
    {
        void OnBreak(DelegateResult<TResult> delegateResult, TimeSpan breakSpan, Context context);

        void OnReset(Context context);
    }
}
