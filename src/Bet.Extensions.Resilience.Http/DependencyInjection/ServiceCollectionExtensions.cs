using System;
using System.Linq;

using Bet.Extensions.Resilience.Abstractions;
using Bet.Extensions.Resilience.Http.Options;
using Bet.Extensions.Resilience.Http.Policies;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        private static readonly Func<IServiceCollection, HttpPolicyRegistrant>
     _findPolicyBuilderIntance = (services) => services.SingleOrDefault(sd => sd.ServiceType == typeof(HttpPolicyRegistrant))?.ImplementationInstance as HttpPolicyRegistrant;

        public static IServiceCollection AddHttpResiliencePolicy(
            this IServiceCollection services,
            Action<HttpPolicyOptions> configure = null,
            string policySectionName = Constants.Policies,
            string policyName = HttpPoliciesKeys.DefaultPolicies)
        {
            return services.AddHttpResiliencePolicy<HttpPolicyOptions>(configure, policySectionName, policyName);
        }

        public static IServiceCollection AddHttpResiliencePolicy<TOptions>(
            this IServiceCollection services,
            Action<TOptions> configure = null,
            string policySectionName = Constants.Policies,
            string policyName = HttpPoliciesKeys.DefaultPolicies)
            where TOptions : HttpPolicyOptions, new()
        {
            return services.AddHttpResiliencePolicy<HttpPolicyConfigurator<TOptions>, TOptions>(configure, policySectionName, policyName);
        }

        public static IServiceCollection AddHttpResiliencePolicy<T, TOptions>(
            this IServiceCollection services,
            Action<TOptions> configure = null,
            string policySectionName = Constants.Policies,
            string policyName = HttpPoliciesKeys.DefaultPolicies,
            string[] defaultPolicies = null)
            where T : class, IHttpPolicyConfigurator<TOptions>
            where TOptions : HttpPolicyOptions, new()
        {
            // adds Polly Policy registration
            var registry = services.TryAddPolicyRegistry();

            // adds DI based marker object
            services.TryAddSingleton(new HttpPolicyRegistrant());

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
                options.PolicyName = policyName;
                options.SectionName = policySectionName;
            });

            services.AddChangeTokenOptions(policySectionName, policyName, configure);

            services.AddSingleton<IHttpPolicyConfigurator<TOptions>>((sp) =>
            {
                var provider = sp.GetRequiredService<IServiceProvider>();
                return new HttpPolicyConfigurator<TOptions>(provider, policyName, defaultPolicies);
            });

            // this service provides the initial policies registrations based on the type of the host.
            services.TryAddScoped<IHttpPolicyRegistrator, HttpPolicyRegistrator>();

            return services;
        }

        public static IServiceCollection AddHttpDefaultResiliencePolicies(
            this IServiceCollection services,
            Action<HttpPolicyOptions> configure = null,
            string policySectionName = Constants.Policies,
            string policyName = HttpPoliciesKeys.DefaultPolicies)
        {
            return services.AddHttpDefaultResiliencePolicies<HttpPolicyOptions>(configure, policySectionName, policyName);
        }

        public static IServiceCollection AddHttpDefaultResiliencePolicies<TOptions>(
            this IServiceCollection services,
            Action<TOptions> configure = null,
            string policySectionName = Constants.Policies,
            string policyName = HttpPoliciesKeys.DefaultPolicies) where TOptions : HttpPolicyOptions, new()
        {
                var defaultPolicies = new string[]
                {
                    HttpPoliciesKeys.HttpRequestTimeoutPolicy,
                    HttpPoliciesKeys.HttpWaitAndRetryPolicy,
                    HttpPoliciesKeys.HttpCircuitBreakerPolicy
                };

                services.AddSingleton<IHttpPolicy<TOptions>>(sp =>
                {
                    var logger = sp.GetRequiredService<ILogger<HttpTimeoutPolicy<TOptions>>>();
                    var options = sp.GetRequiredService<IHttpPolicyConfigurator<TOptions>>();

                    return new HttpTimeoutPolicy<TOptions>(HttpPoliciesKeys.HttpRequestTimeoutPolicy, options, logger);
                });

                services.AddSingleton<IHttpPolicy<TOptions>>(sp =>
                {
                    var logger = sp.GetRequiredService<ILogger<HttpRetryPolicy<TOptions>>>();
                    var options = sp.GetRequiredService<IHttpPolicyConfigurator<TOptions>>();

                    return new HttpRetryPolicy<TOptions>(HttpPoliciesKeys.HttpCircuitBreakerPolicy, options, logger);
                });

                services.AddSingleton<IHttpPolicy<TOptions>>(sp =>
                {
                    var logger = sp.GetRequiredService<ILogger<HttpCircuitBreakerPolicy<TOptions>>>();
                    var options = sp.GetRequiredService<IHttpPolicyConfigurator<TOptions>>();

                    return new HttpCircuitBreakerPolicy<TOptions>(HttpPoliciesKeys.HttpWaitAndRetryPolicy, options, logger);
                });

                return services.AddHttpResiliencePolicy<HttpPolicyConfigurator<TOptions>, TOptions>(configure, policySectionName, policyName, defaultPolicies);
        }
    }
}
