using System;
using System.Linq;

using Bet.Extensions.Resilience.Abstractions.Internal;
using Bet.Extensions.Resilience.Abstractions.Options;

using Microsoft.Extensions.DependencyInjection.Extensions;

using Polly;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ResilienceBuilderExtensions
    {
        private static readonly Func<IServiceCollection, PolicyOptionsRegistrant?> FindPolicyOptionsIntance = GetPolicyOptionsRegistrant;

        public static IResilienceBuilder<TPolicy, TOptions> ConfigurePolicy<TPolicy, TOptions>(
            this IResilienceBuilder<TPolicy, TOptions> builder,
            string sectionName,
            Action<PolicyBucketOptions<TOptions>> configurePolicy,
            string? policyName = null,
            Action<TOptions>? configure = null)
            where TPolicy : IsPolicy where TOptions : PolicyOptions, new()
        {
            if (string.IsNullOrWhiteSpace(policyName)
                && policyName == null)
            {
                policyName = builder.PolicyName;
            }

            ValidateOptionsRegistration<TOptions>(builder.Services, sectionName, policyName);

            if (!builder.PolicyNames.Contains(policyName))
            {
                builder.PolicyNames.Add(policyName);
            }

            builder.Services.AddOptions<PolicyBucketOptions<TOptions>>(policyName)
                .Configure<IServiceProvider>((options, provider) =>
                {
                    options.ServiceProvider = provider;
                    options.Name = policyName;
                    configurePolicy(options);
                });

            builder.Services.Configure<TOptions>(policyName, options =>
            {
                options.Name = policyName;
            });

            builder.Services.AddChangeTokenOptions(sectionName, policyName, configure);

            return builder;
        }

        private static void ValidateOptionsRegistration<TOptions>(
            this IServiceCollection services,
            string sectionName,
            string optionsName) where TOptions : PolicyOptions, new()
        {
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
    }
}
