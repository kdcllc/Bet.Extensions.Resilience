using System;

using Bet.Extensions.Resilience.Abstractions.Options;

using Microsoft.Extensions.Logging;

using Polly;
using Polly.CircuitBreaker;

namespace Bet.Extensions.Resilience.Abstractions.Policies
{
    public interface ICircuitBreakerPolicy<TOptions> : IPolicy<TOptions> where TOptions : CircuitBreakerPolicyOptions
    {
        Func<ILogger<IPolicy<TOptions>>, TOptions, Action<Exception, CircuitState, TimeSpan, Context>> OnBreak { get; set; }

        Func<ILogger<IPolicy<TOptions>>, TOptions, Action<Context>> OnReset { get; set; }

        Func<ILogger<IPolicy<TOptions>>, TOptions, Action> OnHalfOpen { get; set; }
    }

    public interface ICircuitBreakerPolicy<TOptions, TResult> : IPolicy<TOptions, TResult> where TOptions : CircuitBreakerPolicyOptions
    {
        Func<ILogger<IPolicy<TOptions, TResult>>, TOptions, Action<DelegateResult<TResult>, CircuitState, TimeSpan, Context>> OnBreak { get; set; }

        Func<ILogger<IPolicy<TOptions, TResult>>, TOptions, Action<Context>> OnReset { get; set; }

        Func<ILogger<IPolicy<TOptions, TResult>>, TOptions, Action> OnHalfOpen { get; set; }
    }
}
