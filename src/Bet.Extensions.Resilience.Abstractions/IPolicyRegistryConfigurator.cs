using System;

using Polly;

namespace Bet.Extensions.Resilience.Abstractions
{
    /// <summary>
    /// The extension of <see cref="Polly.Registry.IPolicyRegistry{TKey}"/>.
    /// This allow for Dynamic reconfiguration during applications running.
    /// https://github.com/App-vNext/Polly/wiki/Dynamic-reconfiguration-during-running.
    /// </summary>
    public interface IPolicyRegistryConfigurator
    {
        /// <summary>
        /// Validates if the policy has been configured already inside <see cref="Polly.Registry.IPolicyRegistry{TKey}"/>.
        /// </summary>
        /// <param name="policyName">The unique policy name.</param>
        /// <returns></returns>
        bool IsPolicyConfigured(string policyName);

        /// <summary>
        /// Adds policy with  specific name.
        /// </summary>
        /// <param name="policyName">The unique policy name.</param>
        /// <param name="policyFunc">The delegate for the policy function.</param>
        /// <param name="replaceIfExists">The switch to replace the existing policy. The default is false.</param>
        /// <returns></returns>
        IPolicyRegistryConfigurator AddPolicy(string policyName, Func<IsPolicy> policyFunc, bool replaceIfExists = false);
    }
}
