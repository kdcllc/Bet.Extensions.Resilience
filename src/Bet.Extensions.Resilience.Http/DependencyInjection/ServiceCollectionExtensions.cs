using System;
using System.Net.Http;

using Bet.Extensions.Resilience.Abstractions;
using Bet.Extensions.Resilience.Abstractions.Options;
using Bet.Extensions.Resilience.Http.Policies;

using Microsoft.Extensions.Logging;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds Http Resilience policies.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="policySectionName"></param>
        /// <param name="policyName"></param>
        /// <param name="policyConfig"></param>
        /// <param name="defaultPolicies"></param>
        /// <param name="configure"></param>
        /// <returns></returns>
        public static IServiceCollection AddHttpResiliencePolicy(
            this IServiceCollection services,
            string policySectionName = PolicyName.DefaultPolicy,
            string policyName = PolicyName.DefaultPolicy,
            Func<IServiceProvider, IPolicyCreator<PolicyOptions, HttpResponseMessage>>? policyConfig = null,
            string[]? defaultPolicies = null,
            Action<PolicyOptions>? configure = null)
        {
            return services.AddHttpResiliencePolicy<PolicyOptions>(policySectionName, policyName, policyConfig, defaultPolicies, configure);
        }

        /// <summary>
        /// Adds Http Policies with custom options.
        /// </summary>
        /// <typeparam name="TOptions"></typeparam>
        /// <param name="services"></param>
        /// <param name="policySectionName"></param>
        /// <param name="policyName"></param>
        /// <param name="policyConfig"></param>
        /// <param name="defaultPolicies"></param>
        /// <param name="configure"></param>
        /// <returns></returns>
        public static IServiceCollection AddHttpResiliencePolicy<TOptions>(
            this IServiceCollection services,
            string policySectionName = PolicyName.DefaultPolicy,
            string policyName = PolicyName.DefaultPolicy,
            Func<IServiceProvider, IPolicyCreator<TOptions, HttpResponseMessage>>? policyConfig = null,
            string[]? defaultPolicies = null,
            Action<TOptions>? configure = null) where TOptions : PolicyOptions, new()
        {
            return services.AddHttpResiliencePolicy<DefaultPolicyConfigurator<TOptions, HttpResponseMessage>, TOptions>(
                policySectionName,
                policyName,
                policyConfig,
                defaultPolicies,
                configure);
        }

        /// <summary>
        /// Add Default Http Policies.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configure"></param>
        /// <param name="policySectionName"></param>
        /// <param name="policyName"></param>
        /// <returns></returns>
        public static IServiceCollection AddHttpDefaultResiliencePolicies(
            this IServiceCollection services,
            Action<PolicyOptions>? configure = null,
            string policySectionName = PolicyName.DefaultPolicy,
            string policyName = PolicyName.DefaultPolicy)
        {
            return services.AddHttpDefaultResiliencePolicies<PolicyOptions>(configure, policySectionName, policyName);
        }

        /// <summary>
        /// Adds Http Policies to the global Polly registry.
        /// </summary>
        /// <typeparam name="TOptions"></typeparam>
        /// <param name="services"></param>
        /// <param name="configure"></param>
        /// <param name="policySectionName"></param>
        /// <param name="policyName"></param>
        /// <returns></returns>
        public static IServiceCollection AddHttpDefaultResiliencePolicies<TOptions>(
            this IServiceCollection services,
            Action<TOptions>? configure = null,
            string policySectionName = PolicyName.DefaultPolicy,
            string policyName = PolicyName.DefaultPolicy) where TOptions : PolicyOptions, new()
        {
                if (!services.CanAddPolicy(policyName))
                {
                    return services;
                }

                var defaultPolicies = new string[]
                {
                    PolicyName.TimeoutPolicy,
                    PolicyName.RetryPolicy,
                    PolicyName.CircuitBreakerPolicy
                };

                services.AddHttpResiliencePolicy<DefaultPolicyConfigurator<TOptions, HttpResponseMessage>, TOptions>(
                    policySectionName,
                    PolicyName.TimeoutPolicy,
                    sp =>
                    {
                        var logger = sp.GetRequiredService<ILogger<HttpTimeoutPolicy<TOptions>>>();
                        var options = sp.GetRequiredService<IPolicyConfigurator<TOptions, HttpResponseMessage>>();

                        return new HttpTimeoutPolicy<TOptions>(PolicyName.TimeoutPolicy, options, logger);
                    },
                    null,
                    configure);

                services.AddHttpResiliencePolicy<DefaultPolicyConfigurator<TOptions, HttpResponseMessage>, TOptions>(
                        policySectionName,
                        PolicyName.RetryPolicy,
                        sp =>
                        {
                            var logger = sp.GetRequiredService<ILogger<HttpRetryPolicy<TOptions>>>();
                            var options = sp.GetRequiredService<IPolicyConfigurator<TOptions, HttpResponseMessage>>();

                            return new HttpRetryPolicy<TOptions>(PolicyName.RetryPolicy, options, logger);
                        },
                        null,
                        configure);

                services.AddHttpResiliencePolicy<DefaultPolicyConfigurator<TOptions, HttpResponseMessage>, TOptions>(
                        policySectionName,
                        PolicyName.CircuitBreakerPolicy,
                        sp =>
                        {
                            var logger = sp.GetRequiredService<ILogger<HttpCircuitBreakerPolicy<TOptions>>>();
                            var options = sp.GetRequiredService<IPolicyConfigurator<TOptions, HttpResponseMessage>>();

                            return new HttpCircuitBreakerPolicy<TOptions>(PolicyName.CircuitBreakerPolicy, options, logger);
                        },
                        null,
                        configure);

                // Default policy combines the rest of the policies.
                return services.AddHttpResiliencePolicy<DefaultPolicyConfigurator<TOptions, HttpResponseMessage>, TOptions>(
                policySectionName,
                policyName,
                null,
                defaultPolicies,
                configure);
        }

        public static IServiceCollection AddHttpResiliencePolicy<TPolicyConfigurator, TOptions>(
            this IServiceCollection services,
            string policySectionName = PolicyName.DefaultPolicy,
            string policyName = PolicyName.DefaultPolicy,
            Func<IServiceProvider, IPolicyCreator<TOptions, HttpResponseMessage>>? policyConfig = null,
            string[]? defaultPolicies = null,
            Action<TOptions>? configure = null) where TPolicyConfigurator : class, IPolicyConfigurator<TOptions, HttpResponseMessage>
                                                where TOptions : PolicyOptions, new()
        {
            return services.AddResiliencePolicy<TPolicyConfigurator, TOptions, HttpResponseMessage>(
                policySectionName,
                policyName,
                policyConfig,
                defaultPolicies,
                configure);
        }
    }
}
