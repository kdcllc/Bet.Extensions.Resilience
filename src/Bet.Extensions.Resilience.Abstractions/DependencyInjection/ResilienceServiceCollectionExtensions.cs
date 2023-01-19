using Bet.Extensions.Resilience.Abstractions;
using Bet.Extensions.Resilience.Abstractions.DependencyInjection;
using Bet.Extensions.Resilience.Abstractions.Internal;
using Bet.Extensions.Resilience.Abstractions.Options;

using Microsoft.Extensions.DependencyInjection.Extensions;

using Polly;
using Polly.Registry;

namespace Microsoft.Extensions.DependencyInjection;

public static class ResilienceServiceCollectionExtensions
{
    private static readonly Func<IServiceCollection, PolicyRegistrant?> FindPolicyIntance = GetPolicyRegistrant;

    public static IResilienceBuilder<TPolicy, TOptions> AddPollyPolicy<TPolicy, TOptions>(
        this IServiceCollection services,
        string policyName,
        ServiceLifetime serviceLifetime = ServiceLifetime.Singleton)
        where TPolicy : IsPolicy
        where TOptions : PolicyOptions, new()
    {
        // validates and registers Policy with DI marker instance, only unique policies are allowed.
        RegisterPolicy<TPolicy, TOptions>(services, policyName);

        services.AddPolicyServices<TPolicy, TOptions>(serviceLifetime);

        return new DefaultResilienceBuilder<TPolicy, TOptions>(services, policyName);
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

    internal static IServiceCollection AddPolicyServices<TPolicy, TOptions>(this IServiceCollection services, ServiceLifetime serviceLifetime)
        where TPolicy : IsPolicy where TOptions : PolicyOptions
    {
        services.AddLogging();
        services.AddOptions();

        services.TryAddPolicyRegistry();

        services.TryAddSingleton<PolicyBucketConfigurator, PolicyBucketConfigurator>();

        services.Add(ServiceDescriptor.Describe(
            typeof(PolicyBucket<TPolicy, TOptions>),
            typeof(PolicyBucket<TPolicy, TOptions>),
            serviceLifetime));

        return services;
    }

    private static void RegisterPolicy<TPolicy, TOptions>(IServiceCollection services, string policyName)
        where TPolicy : IsPolicy
        where TOptions : PolicyOptions, new()
    {
        var registrant = FindPolicyIntance(services);

        if (registrant != null)
        {
            // only unique policy names are allowed.
            if (registrant.RegisteredPolicies.TryGetValue(policyName, out var type))
            {
                throw new ArgumentException($"{policyName} already exists");
            }

            registrant.RegisteredPolicies.Add(policyName, typeof(PolicyBucket<TPolicy, TOptions>));
        }
    }

    private static PolicyRegistrant? GetPolicyRegistrant(IServiceCollection services)
    {
        services.TryAddSingleton(new PolicyRegistrant());
        return services.SingleOrDefault(sd => sd.ServiceType == typeof(PolicyRegistrant))?.ImplementationInstance as PolicyRegistrant;
    }
}
