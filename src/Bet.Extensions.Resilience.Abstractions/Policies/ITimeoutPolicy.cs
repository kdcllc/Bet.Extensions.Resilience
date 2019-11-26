using System;
using System.Threading.Tasks;

using Bet.Extensions.Resilience.Abstractions.Options;

using Polly;

namespace Bet.Extensions.Resilience.Abstractions.Policies
{
    /// <summary>
    /// Timeout policy definition.
    /// </summary>
    /// <typeparam name="TOptions"></typeparam>
    public interface ITimeoutPolicy<TOptions> : IPolicy<TOptions> where TOptions : PolicyOptions
    {
        /// <summary>
        /// The timeout async action.
        /// </summary>
        /// <param name="context">The polly context.</param>
        /// <param name="timeout">The timeout value.</param>
        /// <param name="abandonedTask">
        /// Th captured abandoned <see cref="Task"/> as timed-out action.
        /// The Task parameter will be null if the executed
        /// action responded co-operatively to cancellation before the policy timed it out.
        /// </param>
        /// <param name="ex">The captured <see cref="Exception"/> of the canceled timed-out action.</param>
        /// <returns>The <see cref="Task"/>.</returns>
        Task OnTimeoutAsync(Context context, TimeSpan timeout, Task abandonedTask, Exception ex);

        /// <summary>
        /// The time out sync action.
        /// </summary>
        /// <param name="context">The polly context.</param>
        /// <param name="timeout">The timeout value.</param>
        /// <param name="abandonedTask">
        /// Th captured abandoned <see cref="Task"/> as timed-out action.
        /// The Task parameter will be null if the executed
        /// action responded co-operatively to cancellation before the policy timed it out.
        /// </param>
        /// <param name="ex">The captured <see cref="Exception"/> of the canceled timed-out action.</param>
        void OnTimeout(Context context, TimeSpan timeout, Task abandonedTask, Exception ex);
    }

    public interface ITimeoutPolicy<TOptions, TResult> : IPolicy<TOptions, TResult> where TOptions : PolicyOptions
    {
        /// <summary>
        /// The timeout action.
        /// </summary>
        /// <param name="context">The polly context.</param>
        /// <param name="timeout">The timeout value.</param>
        /// <param name="abandonedTask">
        /// Th captured abandoned <see cref="Task"/> as timed-out action.
        /// The Task parameter will be null if the executed
        /// action responded co-operatively to cancellation before the policy timed it out.
        /// </param>
        /// <param name="ex">The captured <see cref="Exception"/> of the canceled timed-out action.</param>
        /// <returns>The <see cref="Task"/>.</returns>
        Task OnTimeoutAsync(Context context, TimeSpan timeout, Task abandonedTask, Exception ex);

        /// <summary>
        /// The time out sync action.
        /// </summary>
        /// <param name="context">The polly context.</param>
        /// <param name="timeout">The timeout value.</param>
        /// <param name="abandonedTask">
        /// Th captured abandoned <see cref="Task"/> as timed-out action.
        /// The Task parameter will be null if the executed
        /// action responded co-operatively to cancellation before the policy timed it out.
        /// </param>
        /// <param name="ex">The captured <see cref="Exception"/> of the canceled timed-out action.</param>
        void OnTimeout(Context context, TimeSpan timeout, Task abandonedTask, Exception ex);
    }
}
