using System;
using System.Threading.Tasks;

using Bet.Extensions.Resilience.Abstractions.Options;

using Microsoft.Extensions.Logging;

using Polly;

namespace Bet.Extensions.Resilience.Abstractions.Policies
{
    public interface IRetryJitterPolicy<TOptions> : IPolicy<TOptions> where TOptions : RetryJitterPolicyOptions
    {
        Func<ILogger<IPolicy<TOptions>>, TOptions, Func<Exception, TimeSpan, int, Context, Task>> OnRetryAsync { get; set; }

        Func<ILogger<IPolicy<TOptions>>, TOptions, Action<Exception, TimeSpan, int, Context>> OnRetry { get; set; }
    }

    public interface IRetryJitterPolicy<TOptions, TResult> : IPolicy<TOptions, TResult> where TOptions : RetryJitterPolicyOptions
    {
        Func<ILogger<IPolicy<TOptions, TResult>>, TOptions, Func<DelegateResult<TResult>, TimeSpan, int, Context, Task>> OnRetryAsync { get; set; }

        Func<ILogger<IPolicy<TOptions, TResult>>, TOptions, Action<DelegateResult<TResult>, TimeSpan, int, Context>> OnRetry { get; set; }
    }
}
