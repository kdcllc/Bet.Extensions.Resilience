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
    /// <typeparam name="TOptions">The configuration options type.</typeparam>
    /// <typeparam name="TResult">The return type from the policy.</typeparam>
    public interface IPolicyConfigurator<TOptions, TResult> where TOptions : PolicyOptions
    {
        string ParentPolicyName { get; }

        string[]? ChildrenPolicyNames { get; }

        /// <summary>
        /// The collection of the options for the specific configuration section.
        /// </summary>
        IReadOnlyDictionary<string, TOptions> OptionsCollection { get; }

        /// <summary>
        /// The collection o Async Policies.
        /// </summary>
        IReadOnlyDictionary<string, Func<IAsyncPolicy<TResult>>> AsyncPolicyCollection { get; }

        /// <summary>
        /// The collection of sync policies.
        /// </summary>
        IReadOnlyDictionary<string, Func<ISyncPolicy<TResult>>> SyncPolicyCollection { get; }

        /// <summary>
        /// Get the named policy option instance.
        /// </summary>
        /// <param name="optionsName"></param>
        /// <returns></returns>
        TOptions GetOptions(string optionsName);

        /// <summary>
        /// Register Async Policy.
        /// </summary>
        /// <param name="policyName">The name policy to be used in <see cref="Polly.Registry.IPolicyRegistry{TKey}"/>.</param>
        /// <param name="policyFunc">The configuration function.</param>
        /// <param name="replaceIfExists">The flag to override existing values.</param>
        /// <returns></returns>
        IPolicyConfigurator<TOptions, TResult> AddPolicy(string policyName, Func<IAsyncPolicy<TResult>> policyFunc, bool replaceIfExists = false);

        /// <summary>
        /// Register Sync <see cref="HttpRequestMessage"/> Policy.
        /// </summary>
        /// <param name="policyName">The name policy to be used in <see cref="Polly.Registry.IPolicyRegistry{TKey}"/>.</param>
        /// <param name="policyFunc">The configuration function.</param>
        /// <param name="replaceIfExists">The flag to override existing values.</param>
        /// <returns></returns>
        IPolicyConfigurator<TOptions, TResult> AddPolicy(string policyName, Func<ISyncPolicy<TResult>> policyFunc, bool replaceIfExists = false);

        /// <summary>
        /// Register the policies.
        /// </summary>
        void ConfigurePolicies();
    }
}
