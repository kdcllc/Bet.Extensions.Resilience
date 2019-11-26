using Bet.Extensions.Resilience.Abstractions.Options;

using Polly;

namespace Bet.Extensions.Resilience.Abstractions.Policies
{
    /// <summary>
    /// The base interface for Policy Registration and retrieval.
    /// </summary>
    /// <typeparam name="TOptions"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    public interface IPolicyConfigurator<TOptions, TResult> where TOptions : PolicyOptions
    {
        /// <summary>
        /// Gets Async <see cref="Policy"/>.
        /// </summary>
        /// <returns></returns>
        IAsyncPolicy<TResult> GetAsyncPolicy();

        /// <summary>
        /// Gets Sync <see cref="Policy"/>.
        /// </summary>
        /// <returns></returns>
        ISyncPolicy<TResult> GetSyncPolicy();

        /// <summary>
        /// This method is used to register policies with <see cref="IPolicyRegistryConfigurator"/> implementation.
        /// </summary>
        void ConfigurePolicy();
    }
}
