using System;
using System.Threading.Tasks;

using Bet.Extensions.Resilience.Abstractions.Options;

using Microsoft.Extensions.Logging;

using Polly;

namespace Bet.Extensions.Resilience.Abstractions.Policies
{
    /// <summary>
    /// Timeout Policy definition.
    /// </summary>
    /// <typeparam name="TOptions"></typeparam>
    public interface ITimeoutPolicy<TOptions> : IPolicy<TOptions> where TOptions : PolicyOptions
    {
        /// <summary>
        /// The timeout async action.
        /// Th captured abandoned <see cref="Task"/> as timed-out action.
        /// The Task parameter will be null if the executed
        /// action responded co-operatively to cancellation before the policy timed it out.
        /// </summary>
        Func<ILogger<IPolicy<TOptions>>, TOptions, Func<Context, TimeSpan, Task, Exception, Task>> OnTimeoutAsync { get; set; }

        /// <summary>
        /// The time out sync action.
        /// Th captured abandoned <see cref="Task"/> as timed-out action.
        /// The Task parameter will be null if the executed
        /// action responded co-operatively to cancellation before the policy timed it out.
        /// The captured <see cref="Exception"/> of the canceled timed-out action.
        /// </summary>
        Func<ILogger<IPolicy<TOptions>>, TOptions, Action<Context, TimeSpan, Task, Exception>> OnTimeout { get; set; }
    }

    /// <summary>
    /// Timeout Policy definition.
    /// </summary>
    /// <typeparam name="TOptions"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    public interface ITimeoutPolicy<TOptions, TResult> : IPolicy<TOptions, TResult> where TOptions : PolicyOptions
    {
        /// <summary>
        /// The timeout async action.
        /// Th captured abandoned <see cref="Task"/> as timed-out action.
        /// The Task parameter will be null if the executed
        /// action responded co-operatively to cancellation before the policy timed it out.
        /// </summary>
        Func<ILogger<IPolicy<TOptions>>, TOptions, Func<Context, TimeSpan, Task, Exception, Task>> OnTimeoutAsync { get; set; }

        /// <summary>
        /// The time out sync action.
        /// Th captured abandoned <see cref="Task"/> as timed-out action.
        /// The Task parameter will be null if the executed
        /// action responded co-operatively to cancellation before the policy timed it out.
        /// The captured <see cref="Exception"/> of the canceled timed-out action.
        /// </summary>
        Func<ILogger<IPolicy<TOptions>>, TOptions, Action<Context, TimeSpan, Task, Exception>> OnTimeout { get; set; }
    }
}
