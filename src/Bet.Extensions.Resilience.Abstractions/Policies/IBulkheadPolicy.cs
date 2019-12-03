using System;
using System.Threading.Tasks;

using Bet.Extensions.Resilience.Abstractions.Options;

using Microsoft.Extensions.Logging;

using Polly;

namespace Bet.Extensions.Resilience.Abstractions.Policies
{
    public interface IBulkheadPolicy<TOptions> : IPolicy<TOptions> where TOptions : BulkheadPolicyOptions
    {
        Func<ILogger<IPolicy<TOptions>>, TOptions, Action<Context>> OnBulkheadRejected { get; set; }

        Func<ILogger<IPolicy<TOptions>>, TOptions, Func<Context, Task>> OnBulkheadRejectedAsync { get; set; }
    }

    public interface IBulkheadPolicy<TOptions, TResult> : IPolicy<TOptions, TResult> where TOptions : BulkheadPolicyOptions
    {
        Func<ILogger<IPolicy<TOptions, TResult>>, TOptions, Action<Context>> OnBulkheadRejected { get; set; }

        Func<ILogger<IPolicy<TOptions, TResult>>, TOptions, Func<Context, Task>> OnBulkheadRejectedAsync { get; set; }
    }
}
