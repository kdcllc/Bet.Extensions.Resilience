using System;
using System.Collections.Generic;
using System.Net.Http;

using Bet.Extensions.Resilience.Abstractions.Options;

using Polly;

namespace Bet.Extensions.Resilience.Abstractions
{
    /// <summary>
    /// The resilient Polly policy builder.
    /// </summary>
    /// <typeparam name="T">The type of the policy to be configured.</typeparam>
    /// <typeparam name="TOptions"></typeparam>
    public interface IPolicyConfigurator<T, TOptions> where TOptions : PolicyOptions
    {
        /// <summary>
        /// The collection of the options for the specific configuration section.
        /// </summary>
        IReadOnlyDictionary<string, TOptions> OptionsCollection { get; }

        /// <summary>
        /// The collection o Async Policies.
        /// </summary>
        IReadOnlyDictionary<string, Func<IAsyncPolicy<T>>> AsyncPolicyCollection { get; }

        /// <summary>
        /// The collection of sync policies.
        /// </summary>
        IReadOnlyDictionary<string, Func<ISyncPolicy<T>>> SyncPolicyCollection { get; }

        /// <summary>
        /// Get the named policy option instance.
        /// </summary>
        /// <param name="settingsName"></param>
        /// <returns></returns>
        TOptions GetOptions(string settingsName);

        /// <summary>
        /// Register Async Policy.
        /// </summary>
        /// <param name="policyName">The name policy to be used in <see cref="Polly.Registry.IPolicyRegistry{TKey}"/>.</param>
        /// <param name="policyFunc">The configuration function.</param>
        /// <param name="replaceIfExists">The flag to override existing values.</param>
        /// <returns></returns>
        IPolicyConfigurator<T, TOptions> AddPolicy(string policyName, Func<IAsyncPolicy<T>> policyFunc, bool replaceIfExists = false);

        /// <summary>
        /// Register Sync <see cref="HttpRequestMessage"/> Policy.
        /// </summary>
        /// <param name="policyName">The name policy to be used in <see cref="Polly.Registry.IPolicyRegistry{TKey}"/>.</param>
        /// <param name="policyFunc">The configuration function.</param>
        /// <param name="replaceIfExists">The flag to override existing values.</param>
        /// <returns></returns>
        IPolicyConfigurator<T, TOptions> AddPolicy(string policyName, Func<ISyncPolicy<T>> policyFunc, bool replaceIfExists = false);

        /// <summary>
        /// Register the policies.
        /// </summary>
        void ConfigurePolicies();
    }
}
