using System;
using System.Net.Http;

using Bet.Extensions.Resilience.Http.Options;

using Polly;

namespace Bet.Extensions.Resilience.Http.Policies
{
    /// <summary>
    /// The resilient Polly policy builder.
    /// </summary>
    /// <typeparam name="TOptions"></typeparam>
    public interface IResilienceHttpPolicyBuilder<TOptions> where TOptions : HttpPolicyOptions
    {
        /// <summary>
        /// Get the named policy option instance.
        /// </summary>
        /// <param name="settingsName"></param>
        /// <returns></returns>
        TOptions GetOptions(string settingsName);

        /// <summary>
        /// Register Async <see cref="HttpRequestMessage"/> Policy.
        /// </summary>
        /// <param name="policyName">The name policy to be used in <see cref="Polly.Registry.IPolicyRegistry{TKey}"/>.</param>
        /// <param name="policyFunc">The configuration function.</param>
        /// <param name="replaceIfExists">The flag to override existing values.</param>
        /// <returns></returns>
        IResilienceHttpPolicyBuilder<TOptions> AddPolicy(string policyName, Func<IAsyncPolicy<HttpResponseMessage>> policyFunc, bool replaceIfExists = false);

        /// <summary>
        /// Register Sync <see cref="HttpRequestMessage"/> Policy.
        /// </summary>
        /// <param name="policyName">The name policy to be used in <see cref="Polly.Registry.IPolicyRegistry{TKey}"/>.</param>
        /// <param name="policyFunc">The configuration function.</param>
        /// <param name="replaceIfExists">The flag to override existing values.</param>
        /// <returns></returns>
        IResilienceHttpPolicyBuilder<TOptions> AddPolicy(string policyName, Func<ISyncPolicy<HttpResponseMessage>> policyFunc, bool replaceIfExists = false);

        /// <summary>
        /// Register the policies.
        /// </summary>
        /// <returns></returns>
        void RegisterPolicies();
    }
}
