using System;
using System.Linq;
using System.Net.Http;

using Bet.Extensions.Resilience.Abstractions;
using Bet.Extensions.Resilience.Abstractions.Options;
using Bet.Extensions.Resilience.Http.Policies;

using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        private static readonly Func<IServiceCollection, PolicyRegistrant>
     _findPolicyBuilderIntance = (services) => services.SingleOrDefault(sd => sd.ServiceType == typeof(PolicyRegistrant))?.ImplementationInstance as PolicyRegistrant;

        public static IServiceCollection AddHttpResiliencePolicy(
            this IServiceCollection services,
            Action<PolicyOptions>? configure = null,
            string policySectionName = PolicyName.DefaultPolicy,
            string policyName = PolicyName.DefaultPolicy)
        {
            return services.AddHttpResiliencePolicy<PolicyOptions>(configure, policySectionName, policyName);
        }

        public static IServiceCollection AddHttpResiliencePolicy<TOptions>(
            this IServiceCollection services,
            Action<TOptions>? configure = null,
            string policySectionName = PolicyName.DefaultPolicy,
            string policyName = PolicyName.DefaultPolicy)
            where TOptions : PolicyOptions, new()
        {
            return services.AddHttpResiliencePolicy<DefaultPolicyConfigurator<HttpResponseMessage, TOptions>, TOptions>(configure, policySectionName, policyName);
        }

        public static IServiceCollection AddHttpResiliencePolicy<T, TOptions>(
            this IServiceCollection services,
            Action<TOptions>? configure = null,
            string policySectionName = PolicyName.DefaultPolicy,
            string policyName = PolicyName.DefaultPolicy,
            string[] ? defaultPolicies = null)
            where T : class, IPolicyConfigurator<HttpResponseMessage, TOptions>
            where TOptions : PolicyOptions, new()
        {
            // adds Polly Policy registration
            var registry = services.TryAddPolicyRegistry();

            // adds DI based marker object
            services.TryAddSingleton(new PolicyRegistrant());

            // return an instant of ResilienceHttpPolicyRegistrant
            var registrant = _findPolicyBuilderIntance(services);

            if (registrant != null)
            {
                // only unique policy names are allowed.
                if (registrant.RegisteredPolicies.TryGetValue(policyName, out var type))
                {
                    throw new ArgumentException($"{policyName} already exists");
                }

                registrant.RegisteredPolicies.Add(policyName, typeof(TOptions));
            }

            services.Configure<TOptions>(policyName, options =>
            {
                options.Name = policyName;
                options.OptionsName = policySectionName;
            });

            services.AddChangeTokenOptions(policySectionName, policyName, configure);

            services.AddSingleton<IPolicyConfigurator<HttpResponseMessage, TOptions>>((sp) =>
            {
                var provider = sp.GetRequiredService<IServiceProvider>();
                return new DefaultPolicyConfigurator<HttpResponseMessage, TOptions>(provider, policyName, defaultPolicies);
            });

            // this service provides the initial policies registrations based on the type of the host.
            services.TryAddScoped<IPolicyRegistrator, DefaultPolicyRegistrator<HttpResponseMessage, TOptions>>();

            return services;
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

                services.AddScoped<IPolicyCreator<HttpResponseMessage, TOptions>, HttpWaitAndRetryPolicy<TOptions>>(sp =>
                {
                    var logger = sp.GetRequiredService<ILogger<HttpWaitAndRetryPolicy<TOptions>>>();
                    var options = sp.GetRequiredService<IPolicyConfigurator<HttpResponseMessage, TOptions>>();

                    return new HttpWaitAndRetryPolicy<TOptions>(PolicyName.RetryPolicy, options, logger);
                });

                services.AddScoped<IPolicyCreator<HttpResponseMessage, TOptions>, HttpCircuitBreakerPolicy<TOptions>>(sp =>
                {
                    var logger = sp.GetRequiredService<ILogger<HttpCircuitBreakerPolicy<TOptions>>>();
                    var options = sp.GetRequiredService<IPolicyConfigurator<HttpResponseMessage, TOptions>>();

                    return new HttpCircuitBreakerPolicy<TOptions>(PolicyName.CircuitBreakerPolicy, options, logger);
                });

                return services.AddHttpResiliencePolicy<DefaultPolicyConfigurator<HttpResponseMessage, TOptions>, TOptions>(
                configure,
                policySectionName,
                policyName,
                defaultPolicies);
        }
    }
}
