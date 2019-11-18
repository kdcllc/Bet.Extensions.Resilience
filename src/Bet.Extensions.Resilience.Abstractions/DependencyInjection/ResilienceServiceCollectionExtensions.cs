using System;
using System.Linq;

using Bet.Extensions.Resilience.Abstractions;
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
        /// <typeparam name="TRegistrator"></typeparam>
        /// <param name="services"></param>
        /// <param name="policySectionName"></param>
        /// <param name="policyName"></param>
        /// <param name="configure"></param>
        /// <returns></returns>
        public static IServiceCollection AddResiliencePolicy<TRegistrator>(
            this IServiceCollection services,
            string policySectionName = PolicyName.DefaultPolicy,
            string policyName = PolicyName.DefaultPolicy,
            Action<PolicyOptions>? configure = null)
        {
            return services.AddResiliencePolicy<TRegistrator, PolicyOptions>(policySectionName, policyName, configure);
        }

        /// <summary>
        /// Adds Resilience Policies based on generic types provided.
        /// </summary>
        /// <typeparam name="TRegistrator"></typeparam>
        /// <typeparam name="TOptions"></typeparam>
        /// <param name="services"></param>
        /// <param name="policySectionName"></param>
        /// <param name="policyName"></param>
        /// <param name="configure"></param>
        /// <returns></returns>
        public static IServiceCollection AddResiliencePolicy<TRegistrator, TOptions>(
            this IServiceCollection services,
            string policySectionName = PolicyName.DefaultPolicy,
            string policyName = PolicyName.DefaultPolicy,
            Action<TOptions>? configure = null) where TOptions : PolicyOptions, new()
        {
            return services.AddResiliencePolicy<DefaultPolicyConfigurator<TRegistrator, TOptions>, TOptions>(policySectionName, policyName, configure: configure);
        }

        /// <summary>
        /// Adds Resilience Policies based on generic types provided.
        /// </summary>
        /// <typeparam name="T">The policy type.</typeparam>
        /// <typeparam name="TRegistrator">The type of the policy registrant.</typeparam>
        /// <typeparam name="TOptions">The type of the options.</typeparam>
        /// <param name="services">The DI services.</param>
        /// <param name="policySectionName">The policy section name. The default is null.</param>
        /// <param name="policyName">The policy name with <see cref="IReadOnlyPolicyRegistry{TKey}"/> collection, the default is null.</param>
        /// <param name="defaultPolicies">The list of default policies to be registered.</param>
        /// <param name="configure">The options configurations. The default is null.</param>
        /// <returns></returns>
        public static IServiceCollection AddResiliencePolicy<T, TRegistrator, TOptions>(
            this IServiceCollection services,
            string policySectionName = PolicyName.DefaultPolicy,
            string policyName = PolicyName.DefaultPolicy,
            string[] ? defaultPolicies = null,
            Action<TOptions>? configure = null) where T : class, IPolicyConfigurator<TRegistrator, TOptions> where TOptions : PolicyOptions, new()
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

            services.AddSingleton<IPolicyConfigurator<TRegistrator, TOptions>>((sp) =>
            {
                var provider = sp.GetRequiredService<IServiceProvider>();
                return new DefaultPolicyConfigurator<TRegistrator, TOptions>(provider, policyName, defaultPolicies);
            });

            // this service provides the initial policies registrations based on the type of the host.
            services.TryAddScoped<IPolicyRegistrator, DefaultPolicyRegistrator<TRegistrator>>();

            return services;
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
