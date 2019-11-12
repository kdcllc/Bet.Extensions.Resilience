using System;
using System.Linq;

using Microsoft.Extensions.Internal;

using Polly.Registry;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ResilienceHttpClientBuilderExtensions
    {
        /// <summary>
        /// Adds <see cref="ResilienceHttpClientBuilder"/>.
        /// </summary>
        /// <typeparam name="TClient"></typeparam>
        /// <typeparam name="TImplementation"></typeparam>
        /// <param name="services"></param>
        /// <param name="name"></param>
        /// <param name="optionsName"></param>
        /// <returns></returns>
        public static IResilienceHttpClientBuilder AddResilienceHttpClient<TClient, TImplementation>(
            this IServiceCollection services,
            string? name = null,
            string? optionsName = null) where TClient : class where TImplementation : class, TClient
        {
            var builderName = name ?? TypeNameHelper.GetTypeDisplayName(typeof(TClient), fullName: false);
            var options = optionsName ?? TypeNameHelper.GetTypeDisplayName(typeof(TImplementation), fullName: false);

            return new ResilienceHttpTypedClientBuilder<TClient, TImplementation>(services, builderName, options);
        }

        /// <summary>
        /// Registers the provided <see cref="IPolicyRegistry{String}"/> in the service collection with service types
        /// <see cref="IPolicyRegistry{String}"/>, and <see cref="IReadOnlyPolicyRegistry{String}"/> and returns
        /// the provided registry.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
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
    }
}
