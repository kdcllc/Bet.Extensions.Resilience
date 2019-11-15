using System;
using System.Threading;
using System.Threading.Tasks;

namespace Bet.Extensions.Resilience.Abstractions.Executor
{
    /// <summary>
    /// <para>
    /// The policy async executor for wrapped Polly policies.</para>
    /// <para>
    /// Polly doesn't support async void methods. So you can't pass an Action.
    /// http://www.thepollyproject.org/2017/06/09/polly-and-synchronous-versus-asynchronous-policies/
    /// Returns a Task in order to avoid execution of void methods asynchronously, which causes unexpected out-of-sequence execution of policy hooks and continuing policy actions, and a risk of unobserved exceptions.
    /// https://msdn.microsoft.com/en-us/magazine/jj991977.aspx
    /// https://github.com/App-vNext/Polly/issues/107#issuecomment-218835218.
    /// </para>
    /// </summary>
    public interface IPolicyAsyncExecutor
    {
        /// <summary>
        /// Executes Async function delegate with specified Polly policies.
        /// </summary>
        /// <typeparam name="T">The type of the task to be executed.</typeparam>
        /// <param name="action">The function to be executed.</param>
        /// <returns>task of type.</returns>
        Task<T> ExecuteAsync<T>(Func<Task<T>> action);

        /// <summary>
        /// Executes Async the function delegate with specified Polly policies.
        /// </summary>
        /// <param name="action">The function to be executed.</param>
        /// <returns>task.</returns>
        Task ExecuteAsync(Func<Task> action);

        /// <summary>
        /// Executes Async the function delegate with cancellation token.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="action"></param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        Task<T> ExecuteAsync<T>(Func<CancellationToken, Task<T>> action, CancellationToken cancellationToken);
    }
}
