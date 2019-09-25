﻿using System;
using System.Collections.Generic;
using System.Net.Http;

using Bet.Extensions.Resilience.Http.Options;

using Polly;

namespace Bet.Extensions.Resilience.Http.Policies
{
    /// <summary>
    /// The resilient Polly policy builder.
    /// </summary>
    /// <typeparam name="TOptions"></typeparam>
    public interface IHttpPolicyConfigurator<TOptions> where TOptions : HttpPolicyOptions
    {
        /// <summary>
        /// The collection of the options for the specific configuration section.
        /// </summary>
        IReadOnlyDictionary<string, TOptions> OptionsCollection { get; }

        /// <summary>
        /// The collection o Async Policies.
        /// </summary>
        IReadOnlyDictionary<string, Func<IAsyncPolicy<HttpResponseMessage>>> AsyncPolicyCollection { get; }

        /// <summary>
        /// The collection of sync policies.
        /// </summary>
        IReadOnlyDictionary<string, Func<ISyncPolicy<HttpResponseMessage>>> SyncPolicyCollection { get; }

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
        IHttpPolicyConfigurator<TOptions> AddPolicy(string policyName, Func<IAsyncPolicy<HttpResponseMessage>> policyFunc, bool replaceIfExists = false);

        /// <summary>
        /// Register Sync <see cref="HttpRequestMessage"/> Policy.
        /// </summary>
        /// <param name="policyName">The name policy to be used in <see cref="Polly.Registry.IPolicyRegistry{TKey}"/>.</param>
        /// <param name="policyFunc">The configuration function.</param>
        /// <param name="replaceIfExists">The flag to override existing values.</param>
        /// <returns></returns>
        IHttpPolicyConfigurator<TOptions> AddPolicy(string policyName, Func<ISyncPolicy<HttpResponseMessage>> policyFunc, bool replaceIfExists = false);

        /// <summary>
        /// Register the policies.
        /// </summary>
        void ConfigurePolicies();
    }
}