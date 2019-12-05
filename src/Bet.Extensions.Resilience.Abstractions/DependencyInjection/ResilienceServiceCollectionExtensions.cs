using System;
using System.Linq;

using Bet.Extensions.Resilience.Abstractions;
using Bet.Extensions.Resilience.Abstractions.Internal;
using Bet.Extensions.Resilience.Abstractions.Options;
using Bet.Extensions.Resilience.Abstractions.Policies;

using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

using Polly.Registry;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ResilienceServiceCollectionExtensions
    {
        private static readonly Func<IServiceCollection, PolicyOptionsRegistrant?> FindPolicyOptionsIntance = GetPolicyOptionsRegistrant;
        private static readonly Func<IServiceCollection, PolicyRegistrant?> FindPolicyIntance = GetPolicyRegistrant;

        /// <summary>
        /// Adds Resilience <see cref="Polly"/>:
        /// 1. Policy to <see cref="IServiceProvider"/> as specify implementation type.
        /// 2. <see cref="IPolicyRegistry{TKey}"/>.
        /// In addition configures required options based on the section configuration that has been provided.
        /// </summary>
        /// <typeparam name="TImplementation">The implementation type of the policy.</typeparam>
        /// <typeparam name="TOptions">The type of the options to be used.</typeparam>
        /// <typeparam name="TResult">The type for policy result.</typeparam>
        /// <param name="services">The DI services instance.</param>
        /// <param name="policyName">The name of the Policy that must be unique.</param>
        /// <param name="sectionName">The section name for the Policy options configurations.</param>
        /// <param name="policyOptionsName">The policy options name, if not provided is the same as the 'PolicyName'. The default is null.</param>
        /// <param name="configure">The configurations for the policy options.</param>
        /// <param name="serviceLifetime">The <see cref="ServiceLifetime"/> for DI registration.</param>
        public static IServiceCollection AddResiliencePolicy<TImplementation, TOptions, TResult>(
            this IServiceCollection services,
            string policyName,
            string sectionName,
            string? policyOptionsName = default,
            Action<TOptions>? configure = default,
            ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
                where TImplementation : BasePolicy<TOptions, TResult>, IPolicy<TOptions, TResult>
                where TOptions : PolicyOptions, new()
        {
            // validates and registers Policy with DI marker instance, only unique policies are allowed.
            RegisterPolicy<TOptions, TResult>(services, policyName);

            // adds needed DI dependencies across registrations.
            var policyOptions = ConfigureGenericPolicy(services, policyName, sectionName, configure, policyOptionsName);

            // adds policy to DI with IPolicy Interface which allows for on start policy registrations.
            services.Add(ServiceDescriptor.Describe(
                typeof(IPolicy<TOptions, TResult>),
                sp => sp.CreatePolicyInsance<TImplementation, TOptions, TResult>(policyOptions),
                serviceLifetime));

            // adds policy to DI per implementation type specified.
            services.Add(ServiceDescriptor.Describe(
                typeof(TImplementation),
                sp => sp.CreatePolicyInsance<TImplementation, TOptions, TResult>(policyOptions),
                serviceLifetime));

            return services;
        }

        /// <summary>
        /// Adds Resilience <see cref="Polly"/> to <see cref="IServiceProvider"/> as specify implementation type.
        /// In addition configures required options based on the section configuration that has been provided.
        /// </summary>
        /// <typeparam name="TType">The policy type interface.</typeparam>
        /// <typeparam name="TImplementation">The policy implementation.</typeparam>
        /// <typeparam name="TOptions">The type of the options to be used.</typeparam>
        /// <typeparam name="TResult">The type for policy result.</typeparam>
        /// <param name="services">The DI services instance.</param>
        /// <param name="policyName">The name of the Policy that must be unique.</param>
        /// <param name="sectionName">The section name for the Policy options configurations.</param>
        /// <param name="policyOptionsName">The policy options name, if not provided is the same as the 'PolicyName'. The default is null.</param>
        /// <param name="configure">The configurations for the policy options.</param>
        /// <param name="serviceLifetime">The <see cref="ServiceLifetime"/> for DI registration.</param>
        /// <returns></returns>
        public static IServiceCollection AddResiliencePolicy<TType, TImplementation, TOptions, TResult>(
            this IServiceCollection services,
            string policyName,
            string sectionName,
            string? policyOptionsName = default,
            Action<TOptions>? configure = default,
            ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
                where TType : IPolicy<TOptions, TResult>
                where TImplementation : BasePolicy<TOptions, TResult>, IPolicy<TOptions, TResult>
                where TOptions : PolicyOptions, new()
        {
            // validates and registers Policy with DI marker instance, only unique policies are allowed.
            RegisterPolicy<TOptions, TResult>(services, policyName);

            // adds needed DI dependencies across registrations.
            var policyOptions = ConfigureGenericPolicy(services, policyName, sectionName, configure, policyOptionsName);

            // adds policy to DI with IPolicy Interface which allows for on start policy registrations.
            services.Add(ServiceDescriptor.Describe(
                typeof(IPolicy<TOptions, TResult>),
                sp => sp.CreatePolicyInsance<TImplementation, TOptions, TResult>(policyOptions),
                serviceLifetime));

            // adds IPolicy{TPolicyType}<TOptions> instance.
            services.Add(ServiceDescriptor.Describe(
                typeof(TType),
                sp => sp.CreatePolicyInsance<TImplementation, TOptions, TResult>(policyOptions),
                serviceLifetime));

            return services;
        }

        /// <summary>
        /// Adds Resilience <see cref="Polly"/>:
        /// 1. Policy to <see cref="IServiceProvider"/> as specify implementation type.
        /// 2. <see cref="IPolicyRegistry{TKey}"/>.
        /// In addition configures required options based on the section configuration that has been provided.
        /// NOTE: this method allows for multiple instances of the Policy to be registered as along as the Policy Name is unique.
        /// </summary>
        /// <typeparam name="TImplementation">The implementation type of the policy.</typeparam>
        /// <typeparam name="TOptions">The type of the options to be used.</typeparam>
        /// <param name="services">The DI services instance.</param>
        /// <param name="policyName">The name of the Policy that must be unique.</param>
        /// <param name="sectionName">The section name for the Policy options configurations.</param>
        /// <param name="policyOptionsName">The policy options name, if not provided is the same as the 'PolicyName'. The default is null.</param>
        /// <param name="configure">The configurations for the policy options.</param>
        /// <param name="serviceLifetime">The <see cref="ServiceLifetime"/> for DI registration.</param>
        /// <returns></returns>
        public static IServiceCollection AddResiliencePolicy<TImplementation, TOptions>(
            this IServiceCollection services,
            string policyName,
            string sectionName,
            string? policyOptionsName = default,
            Action<TOptions>? configure = default,
            ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
             where TImplementation : BasePolicy<TOptions>, IPolicy<TOptions>
             where TOptions : PolicyOptions, new()
        {
            // validates and registers Policy with DI marker instance, only unique policies are allowed.
            RegisterPolicy<TOptions, object>(services, policyName);

            var policyOptions = ConfigureGenericPolicy(services, policyName, sectionName, configure, policyOptionsName);

            // adds policy to DI with IPolicy Interface which allows for on start policy registrations.
            services.Add(ServiceDescriptor.Describe(
                typeof(IPolicy<TOptions>),
                sp => sp.CreatePolicyInsance<TImplementation, TOptions>(policyOptions),
                serviceLifetime));

            // adds policy to DI per implementation type specified.
            services.Add(ServiceDescriptor.Describe(
                typeof(TImplementation),
                sp => sp.CreatePolicyInsance<TImplementation, TOptions>(policyOptions),
                serviceLifetime));

            return services;
        }

        /// <summary>
        /// Adds Resilience <see cref="Polly"/>to <see cref="IServiceProvider"/> as specify implementation type.
        /// In addition configures required options based on the section configuration that has been provided.
        /// </summary>
        /// <typeparam name="TType">The policy type interface.</typeparam>
        /// <typeparam name="TImplementation">The policy implementation.</typeparam>
        /// <typeparam name="TOptions">The type of the options to be used.</typeparam>
        /// <param name="services">The DI services instance.</param>
        /// <param name="policyName">The name of the Policy that must be unique.</param>
        /// <param name="sectionName">The section name for the Policy options configurations.</param>
        /// <param name="policyOptionsName">The policy options name, if not provided is the same as the 'PolicyName'. The default is null.</param>
        /// <param name="configure">The configurations for the policy options.</param>
        /// <param name="serviceLifetime">The <see cref="ServiceLifetime"/> for DI registration.</param>
        /// <returns></returns>
        public static IServiceCollection AddResiliencePolicy<TType, TImplementation, TOptions>(
           this IServiceCollection services,
           string policyName,
           string sectionName,
           string? policyOptionsName = default,
           Action<TOptions>? configure = default,
           ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
                where TType : IPolicy<TOptions>
                where TImplementation : BasePolicy<TOptions>, IPolicy<TOptions>
                where TOptions : PolicyOptions, new()
        {
            // Validates and registers with DI marker instance.
            RegisterPolicy<TOptions, object>(services, policyName);

            // Adds needed DI references.
            var policyOptions = ConfigureGenericPolicy(services, policyName, sectionName, configure, policyOptionsName);

            // adds policy to DI with IPolicy Interface which allows for on start policy registrations.
            services.Add(ServiceDescriptor.Describe(
                typeof(IPolicy<TOptions>),
                sp => sp.CreatePolicyInsance<TImplementation, TOptions>(policyOptions),
                serviceLifetime));

            // adds IPolicy{TPolicyType}<TOptions> instance.
            services.Add(ServiceDescriptor.Describe(
                typeof(TType),
                sp => sp.CreatePolicyInsance<TImplementation, TOptions>(policyOptions),
                serviceLifetime));

            return services;
        }

        /// <summary>
        /// Add Resilience Options to configurations.
        /// This method can be used independently from the rest of the framework.
        /// If the desired functionality is to only configure the options.
        /// </summary>
        /// <typeparam name="TOptions">The type of options.</typeparam>
        /// <param name="services">The DI services instance.</param>
        /// <param name="sectionName">The options section name.</param>
        /// <param name="optionsName">The options name within named options.</param>
        /// <param name="configure">The configurations of options.</param>
        /// <returns></returns>
        public static IServiceCollection ConfigureResilienceOptions<TOptions>(
            this IServiceCollection services,
            string sectionName,
            string optionsName,
            Action<TOptions>? configure = null) where TOptions : PolicyOptions, new()
        {
            services.ValidateOptionsRegistration<TOptions>(sectionName, optionsName);

            services.Configure<TOptions>(optionsName, options =>
            {
                options.OptionsName = optionsName;
            });

            services.AddChangeTokenOptions(sectionName, optionsName, configure);

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

        private static PolicyOptions ConfigureGenericPolicy<TOptions>(
            IServiceCollection services,
            string policyName,
            string sectionName,
            Action<TOptions>? configure = default,
            string? policyOptionsName = default)
                        where TOptions : PolicyOptions, new()
        {
            policyOptionsName ??= policyName;

            var policyOptions = new PolicyOptions { Name = policyName, OptionsName = policyOptionsName };

            // add Polly Policy registration
            services.TryAddPolicyRegistry();

            // policy registration interface
            services.TryAddSingleton<IPolicyRegistryConfigurator, DefaultPolicyRegistryConfigurator>();

            // responsible for registering all of the policies.
            services.TryAddSingleton<IPolicyRegistrator, DefaultPolicyRegistrator>();

            services.ConfigureResilienceOptions(sectionName, policyOptionsName, configure);
            services.Configure<TOptions>(policyOptionsName, options =>
            {
                options.Name = policyName;
                options.OptionsName = policyOptionsName;
            });

            return policyOptions;
        }

        private static void RegisterPolicy<TOptions, TResult>(IServiceCollection services, string policyName) where TOptions : PolicyOptions, new()
        {
            var registrant = FindPolicyIntance(services);

            if (registrant != null)
            {
                // only unique policy names are allowed.
                if (registrant.RegisteredPolicies.TryGetValue(policyName, out var type))
                {
                    throw new ArgumentException($"{policyName} already exists");
                }

                if (typeof(TResult) == typeof(object))
                {
                    registrant.RegisteredPolicies.Add(policyName, (typeof(TOptions), null));
                }
                else
                {
                    registrant.RegisteredPolicies.Add(policyName, (typeof(TOptions), typeof(TResult)));
                }
            }
        }

        private static TImplementation CreatePolicyInsance<TImplementation, TOptions, TResult>(
            this IServiceProvider sp,
            PolicyOptions policyOptions)
            where TImplementation : BasePolicy<TOptions, TResult>, IPolicy<TOptions, TResult>
            where TOptions : PolicyOptions, new()
        {
            var serviceProvider = sp.GetRequiredService<IServiceProvider>();
            var policyOptionsConfigurator = sp.GetRequiredService<IPolicyOptionsConfigurator<TOptions>>();
            var registryConfigurator = sp.GetRequiredService<IPolicyRegistryConfigurator>();
            var logger = sp.GetRequiredService<ILogger<IPolicy<TOptions, TResult>>>();

            return ActivatorUtilities.CreateInstance<TImplementation>(
                sp,
                policyOptions,
                serviceProvider,
                policyOptionsConfigurator,
                registryConfigurator,
                logger);
        }

        private static TImplementation CreatePolicyInsance<TImplementation, TOptions>(
            this IServiceProvider sp,
            PolicyOptions policyOptions)
            where TImplementation : BasePolicy<TOptions>, IPolicy<TOptions>
            where TOptions : PolicyOptions, new()
        {
            var serviceProvider = sp.GetRequiredService<IServiceProvider>();

            var policyOptionsConfigurator = sp.GetRequiredService<IPolicyOptionsConfigurator<TOptions>>();
            var registryConfigurator = sp.GetRequiredService<IPolicyRegistryConfigurator>();
            var logger = sp.GetRequiredService<ILogger<IPolicy<TOptions>>>();

            var x = ActivatorUtilities.CreateInstance<TImplementation>(
                sp,
                policyOptions,
                serviceProvider,
                policyOptionsConfigurator,
                registryConfigurator,
                logger) { P};
        }

        private static void ValidateOptionsRegistration<TOptions>(
            this IServiceCollection services,
            string sectionName,
            string optionsName) where TOptions : PolicyOptions, new()
        {
            services.TryAddSingleton<IPolicyOptionsConfigurator<TOptions>, DefaultPolicyOptionsConfigurator<TOptions>>();

            var registrant = FindPolicyOptionsIntance(services);

            if (registrant != null)
            {
                // only unique policy names are allowed.
                if (registrant.RegisteredPolicyOptions.TryGetValue(optionsName, out var type))
                {
                    throw new ArgumentException($"Resilience Options named: {optionsName} already exists");
                }

                registrant.RegisteredPolicyOptions.Add(optionsName, (sectionName, typeof(TOptions)));
            }
        }

        private static PolicyOptionsRegistrant? GetPolicyOptionsRegistrant(IServiceCollection services)
        {
            services.TryAddSingleton(new PolicyOptionsRegistrant());
            return services.SingleOrDefault(sd => sd.ServiceType == typeof(PolicyOptionsRegistrant))?.ImplementationInstance as PolicyOptionsRegistrant;
        }

        private static PolicyRegistrant? GetPolicyRegistrant(IServiceCollection services)
        {
            services.TryAddSingleton(new PolicyRegistrant());
            return services.SingleOrDefault(sd => sd.ServiceType == typeof(PolicyRegistrant))?.ImplementationInstance as PolicyRegistrant;
        }
    }
}
