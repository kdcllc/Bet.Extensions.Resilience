
using Bet.Extensions.Resilience.Abstractions.Options;

using Polly;

namespace Bet.Extensions.Resilience.Abstractions
{
    /// <summary>
    /// The Default interface that allows for Policy Configurations framework to configure policies.
    /// </summary>
    /// <typeparam name="TOptions">The type of the Configuration Options.</typeparam>
    /// <typeparam name="TResult">The return type from the policy.</typeparam>
    public interface IPolicyCreator<TOptions, TResult> where TOptions : PolicyOptions
    {
        /// <summary>
        /// The name of the Policy to be configured.
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
        IAsyncPolicy<TResult> CreateAsyncPolicy();

        /// <summary>
        /// Create Sync Polly Policy.
        /// </summary>
        /// <returns></returns>
        ISyncPolicy<TResult> CreateSyncPolicy();
    }
}
