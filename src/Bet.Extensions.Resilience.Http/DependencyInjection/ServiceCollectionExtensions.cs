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
        public static IServiceCollection AddHttpResiliencePolicy(
            this IServiceCollection services,
            string policySectionName = PolicyName.DefaultPolicy,
            string policyName = PolicyName.DefaultPolicy,
            Action<PolicyOptions>? configure = null)
        {
            return services.AddHttpResiliencePolicy<PolicyOptions>(policySectionName, policyName, configure);
        }

        public static IServiceCollection AddHttpResiliencePolicy<TOptions>(
            this IServiceCollection services,
            string policySectionName = PolicyName.DefaultPolicy,
            string policyName = PolicyName.DefaultPolicy,
            Action<TOptions>? configure = null)
            where TOptions : PolicyOptions, new()
        {
            return services.AddHttpResiliencePolicy<DefaultPolicyConfigurator<HttpResponseMessage, TOptions>, TOptions>(policySectionName, policyName, configure: configure);
        }

        public static IServiceCollection AddHttpResiliencePolicy<T, TOptions>(
            this IServiceCollection services,
            string policySectionName = PolicyName.DefaultPolicy,
            string policyName = PolicyName.DefaultPolicy,
            string[] ? defaultPolicies = null,
            Action<TOptions>? configure = null)
            where T : class, IPolicyConfigurator<HttpResponseMessage, TOptions>
            where TOptions : PolicyOptions, new()
        {
            return services.AddResiliencePolicy<T, HttpResponseMessage, TOptions>(policySectionName, policyName, defaultPolicies, configure);
        }

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
                var defaultPolicies = new string[]
                {
                    PolicyName.TimeoutPolicy,
                    PolicyName.RetryPolicy,
                    PolicyName.CircuitBreakerPolicy
                };

                services.AddScoped<IPolicyCreator<HttpResponseMessage, TOptions>, HttpTimeoutPolicy<TOptions>>(sp =>
                {
                    var logger = sp.GetRequiredService<ILogger<HttpTimeoutPolicy<TOptions>>>();
                    var options = sp.GetRequiredService<IPolicyConfigurator<HttpResponseMessage, TOptions>>();

                    return new HttpTimeoutPolicy<TOptions>(PolicyName.TimeoutPolicy, options, logger);
                });

                services.AddScoped<IPolicyCreator<HttpResponseMessage, TOptions>, HttpRetryPolicy<TOptions>>(sp =>
                {
                    var logger = sp.GetRequiredService<ILogger<HttpRetryPolicy<TOptions>>>();
                    var options = sp.GetRequiredService<IPolicyConfigurator<HttpResponseMessage, TOptions>>();

                    return new HttpRetryPolicy<TOptions>(PolicyName.RetryPolicy, options, logger);
                });

                services.AddScoped<IPolicyCreator<HttpResponseMessage, TOptions>, HttpCircuitBreakerPolicy<TOptions>>(sp =>
                {
                    var logger = sp.GetRequiredService<ILogger<HttpCircuitBreakerPolicy<TOptions>>>();
                    var options = sp.GetRequiredService<IPolicyConfigurator<HttpResponseMessage, TOptions>>();

                    return new HttpCircuitBreakerPolicy<TOptions>(PolicyName.CircuitBreakerPolicy, options, logger);
                });

                return services.AddHttpResiliencePolicy<DefaultPolicyConfigurator<HttpResponseMessage, TOptions>, TOptions>(
                policySectionName,
                policyName,
                defaultPolicies,
                configure);
        }
    }
}
