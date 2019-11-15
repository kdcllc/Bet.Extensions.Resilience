using System;

namespace Bet.Extensions.Resilience.Abstractions.Executor
{
    /// <summary>
    /// The policy sync executor for wrapped Polly policies.
    /// </summary>
    public interface IPolicySyncExecutor
    {
        /// <summary>
        /// Execute Sync function with policies.
        /// </summary>
        /// <typeparam name="T">The type pf the task to be executed.</typeparam>
        /// <param name="action">The function to be executed.</param>
        /// <returns>type of the result.</returns>
        T Execute<T>(Func<T> action);

        /// <summary>
        /// Execute Sync Action with policies.
        /// </summary>
        /// <param name="action">The action to be executed.</param>
        void Execute(Action action);
    }
}
