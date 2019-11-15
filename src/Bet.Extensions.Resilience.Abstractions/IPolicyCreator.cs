
using Bet.Extensions.Resilience.Abstractions.Options;

using Polly;

namespace Bet.Extensions.Resilience.Abstractions
{
    /// <summary>
    /// The default interface that allows for Policy Configurations framework to configure it.
    /// </summary>
    /// <typeparam name="T">The type for the policy.</typeparam>
    /// <typeparam name="TOptions"></typeparam>
    public interface IPolicyCreator<T, TOptions> where TOptions : PolicyOptions
    {
        /// <summary>
        /// The name of the Http Policy to be configured.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Policies options.
        /// </summary>
        TOptions Options { get; }

        /// <summary>
        /// The method to register the policies with the.
        /// This method is called inside <see cref="DefaultPolicyConfigurator{T, TOptions}"/> class.
        /// </summary>
        void RegisterPolicy();

        /// <summary>
        /// Create Async Polly Policy.
        /// </summary>
        /// <returns></returns>
        IAsyncPolicy<T> CreateAsyncPolicy();

        /// <summary>
        /// Create Sync Polly Policy.
        /// </summary>
        /// <returns></returns>
        ISyncPolicy<T> CreateSyncPolicy();
    }
}
