using System;
using System.Linq;

using Bet.Extensions.Resilience.Abstractions;
using Bet.Extensions.Resilience.Abstractions.Internal;
using Bet.Extensions.Resilience.Abstractions.Options;

using Microsoft.Extensions.DependencyInjection.Extensions;

using Polly.Registry;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ResilienceServiceCollectionExtensions
    {
        private static readonly Func<IServiceCollection, PolicyRegistrant?> _findPolicyBuilderIntance = GetPolicyRegistrant;

        /// <summary>
        /// Adds Resilience Policies based on default <see cref="PolicyOptions"/> type.
        /// </summary>
        /// <typeparam name="TResult">The type of the policy result.</typeparam>
        /// <param name="services">The DI services.</param>
        /// <param name="policySectionName">The policy section name for named options.</param>
        /// <param name="policyName">The policy name registered with <see cref="IPolicyRegistry{TKey}"/>.</param>
        /// <param name="policyConfig"></param>
        /// <param name="defaultPolicies"></param>
        /// <param name="configure">The options configuration. The default is null.</param>
        /// <returns></returns>
        public static IServiceCollection AddResiliencePolicy<TResult>(
            this IServiceCollection services,
            string policySectionName = PolicyName.DefaultPolicy,
            string policyName = PolicyName.DefaultPolicy,
            Func<IServiceProvider, IPolicyCreator<PolicyOptions, TResult>>? policyConfig = null,
            string[]? defaultPolicies = null,
            Action<PolicyOptions>? configure = null)
        {
            return services.AddResiliencePolicy<PolicyOptions, TResult>(policySectionName, policyName, policyConfig, defaultPolicies, configure);
        }

        /// <summary>
        /// Adds Resilience Policies based on generic types provided.
        /// </summary>
        /// <typeparam name="TOptions"></typeparam>
        /// <typeparam name="TResult">The type of the policy result.</typeparam>
        /// <param name="services"></param>
        /// <param name="policySectionName"></param>
        /// <param name="policyName"></param>
        /// <param name="policyConfig"></param>
        /// <param name="defaultPolicies"></param>
        /// <param name="configure"></param>
        /// <returns></returns>
        public static IServiceCollection AddResiliencePolicy<TOptions, TResult>(
            this IServiceCollection services,
            string policySectionName = PolicyName.DefaultPolicy,
            string policyName = PolicyName.DefaultPolicy,
            Func<IServiceProvider, IPolicyCreator<TOptions, TResult>>? policyConfig = null,
            string[]? defaultPolicies = null,
            Action<TOptions>? configure = null) where TOptions : PolicyOptions, new()
        {
            return services.AddResiliencePolicy<DefaultPolicyConfigurator<TOptions, TResult>, TOptions, TResult>(
                policySectionName,
                policyName,
                policyConfig,
                defaultPolicies,
                configure);
        }

        /// <summary>
        /// Adds Resilience Policies based on generic types provided.
        /// </summary>
        /// <typeparam name="T">The policy type.</typeparam>
        /// <typeparam name="TOptions">The type of the options.</typeparam>
        /// <typeparam name="TResult">The type of the policy registrant.</typeparam>
        /// <param name="services">The DI services.</param>
        /// <param name="policySectionName">The policy section name. The default is null.</param>
        /// <param name="policyName">The policy name with <see cref="IReadOnlyPolicyRegistry{TKey}"/> collection, the default is null.</param>
        /// <param name="policyConfig"></param>
        /// <param name="childrenPolicyNames"></param>
        /// <param name="configure">The options configurations. The default is null.</param>
        /// <returns></returns>
        public static IServiceCollection AddResiliencePolicy<T, TOptions, TResult>(
            this IServiceCollection services,
            string policySectionName = PolicyName.DefaultPolicy,
            string policyName = PolicyName.DefaultPolicy,
            Func<IServiceProvider, IPolicyCreator<TOptions, TResult>>? policyConfig = null,
            string[]? childrenPolicyNames = null,
            Action<TOptions>? configure = null) where T : class, IPolicyConfigurator<TOptions, TResult> where TOptions : PolicyOptions, new()
        {
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

            // adds Polly Policy registration
            var registry = services.TryAddPolicyRegistry();

            // configure policy creator.
            if (policyConfig != null)
            {
                services.AddScoped(sp => policyConfig(sp));
            }

            services.Configure<TOptions>(policyName, options =>
            {
                options.Name = policyName;
                options.OptionsName = policySectionName;
            });

            services.AddChangeTokenOptions(policySectionName, policyName, configure);

            services.AddSingleton<IPolicyConfigurator<TOptions, TResult>>((sp) =>
            {
                var provider = sp.GetRequiredService<IServiceProvider>();
                return new DefaultPolicyConfigurator<TOptions, TResult>(provider, policyName, childrenPolicyNames);
            });

            // this service provides the initial policies registrations based on the type of the host.
            services.TryAddScoped<IPolicyRegistrator, DefaultPolicyRegistrator<TResult>>();

            return services;
        }

        /// <summary>
        /// Verifies if the Policy can be added.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="policyName"></param>
        /// <returns></returns>
        public static bool CanAddPolicy(this IServiceCollection services, string policyName)
        {
            // return an instant of ResilienceHttpPolicyRegistrant
            var registrant = _findPolicyBuilderIntance(services);

            if (registrant != null)
            {
                // only unique policy names are allowed.
                if (registrant.RegisteredPolicies.TryGetValue(policyName, out var type))
                {
                    return false;
                }

                return true;
            }

            services.TryAddSingleton(new PolicyRegistrant());

            return true;
        }

        /// <summary>
        /// Registers the provided <see cref="IPolicyRegistry{String}"/> in the service collection with service types
        /// <see cref="IPolicyRegistry{String}"/>, and <see cref="IReadOnlyPolicyRegistry{String}"/> and returns
        /// the provided registry.
        /// </summary>
        /// <param name="services">The DI <see cref="IServiceCollection"/>.</param>
        /// <param name="registry">The <see cref="IPolicyRegistry{String}"/>. The default value is null.</param>
        /// <returns>The provided <see cref="IPolicyRegistry{String}"/>.</returns>
        public static IPolicyRegistry<string>? TryAddPolicyRegistry(
            this IServiceCollection services,
            IPolicyRegistry<string>? registry = null)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (!services.Any(d => d.ServiceType == typeof(IReadOnlyPolicyRegistry<string>))
                || !services.Any(d => d.ServiceType == typeof(IPolicyRegistry<string>)))
            {
                if (registry == null)
                {
                    registry = new PolicyRegistry();
                }

                services.AddSingleton(registry);
                services.AddSingleton<IReadOnlyPolicyRegistry<string>>(registry);
            }

            return registry;
        }

        private static PolicyRegistrant? GetPolicyRegistrant(IServiceCollection services)
        {
            return services.SingleOrDefault(sd => sd.ServiceType == typeof(PolicyRegistrant))?.ImplementationInstance as PolicyRegistrant;
        }
    }
}
