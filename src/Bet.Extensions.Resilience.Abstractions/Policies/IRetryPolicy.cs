using System;
using System.Threading.Tasks;

using Bet.Extensions.Resilience.Abstractions.Options;

using Microsoft.Extensions.Logging;

using Polly;

namespace Bet.Extensions.Resilience.Abstractions.Policies
{
    public interface IRetryPolicy<TOptions> : IPolicy<TOptions> where TOptions : PolicyOptions
    {
        Func<ILogger<IPolicy<TOptions>>, TOptions, Func<int, Exception, Context, TimeSpan>> OnDuration { get; set; }

        Func<ILogger<IPolicy<TOptions>>, TOptions, Func<Exception, TimeSpan, int, Context, Task>> OnRetryAsync { get; set; }

        Func<ILogger<IPolicy<TOptions>>, TOptions, Action<Exception, TimeSpan, int, Context>> OnRetry { get; set; }
    }

    public interface IRetryPolicy<TOptions, TResult> : IPolicy<TOptions, TResult> where TOptions : PolicyOptions
    {
        Func<ILogger<IPolicy<TOptions, TResult>>, TOptions, Func<int, DelegateResult<TResult>, Context, TimeSpan>> OnDuration { get; set; }

        Func<ILogger<IPolicy<TOptions, TResult>>, TOptions, Func<DelegateResult<TResult>, TimeSpan, int, Context, Task>> OnRetryAsync { get; set; }

        Func<ILogger<IPolicy<TOptions, TResult>>, TOptions, Action<DelegateResult<TResult>, TimeSpan, int, Context>> OnRetry { get; set; }
    }
}
