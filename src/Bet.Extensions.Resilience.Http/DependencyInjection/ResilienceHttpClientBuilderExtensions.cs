
using Microsoft.Extensions.Internal;

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
    }
}
