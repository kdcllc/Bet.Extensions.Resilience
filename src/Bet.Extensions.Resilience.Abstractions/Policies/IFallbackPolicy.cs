using System;
using System.Threading;
using System.Threading.Tasks;

using Bet.Extensions.Resilience.Abstractions.Options;

using Microsoft.Extensions.Logging;

using Polly;

namespace Bet.Extensions.Resilience.Abstractions.Policies
{
    public interface IFallbackPolicy<TOptions> : IPolicy<TOptions> where TOptions : PolicyOptions
    {
        Func<ILogger<IPolicy<TOptions>>, TOptions, Action<Exception, Context, CancellationToken>> FallBackAction { get; set; }

        Func<ILogger<IPolicy<TOptions>>, TOptions, Action<Exception, Context>> OnFallback { get; set; }

        Func<ILogger<IPolicy<TOptions>>, TOptions, Func<Exception, Context, CancellationToken, Task>> FallbackActionAsync { get; set; }

        Func<ILogger<IPolicy<TOptions>>, TOptions, Func<Exception, Context, Task>> OnFallbackAsync { get; set; }
    }

    public interface IFallbackPolicy<TOptions, TResult> : IPolicy<TOptions, TResult> where TOptions : PolicyOptions
    {
        Func<ILogger<IPolicy<TOptions>>, TOptions, Func<DelegateResult<TResult>, Context, CancellationToken, Task<TResult>>> FallbackActionAsync { get; set; }

        Func<ILogger<IPolicy<TOptions>>, TOptions, Func<DelegateResult<TResult>, Context, Task>> OnFallbackAsync { get; set; }

        Func<ILogger<IPolicy<TOptions>>, TOptions, Func<DelegateResult<TResult>, Context, CancellationToken, TResult>> FallbackAction { get; set; }

        Func<ILogger<IPolicy<TOptions>>, TOptions, Action<DelegateResult<TResult>, Context>> OnFallback { get; set; }
    }
}
