using Bet.Extensions.Http.MessageHandlers.HttpTimeout;

using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class HttpTimeoutHandlerServiceCollectionExtensions
    {
        public static IServiceCollection AddHttpTimeoutHandler(
            this IServiceCollection services,
            Action<HttpTimeoutHandlerOptions>? configure = null)
        {
            services.Configure<HttpTimeoutHandlerOptions>(opt => configure?.Invoke(opt));
            services.TryAddTransient<HttpTimeoutHandler>();
            return services;
        }

        public static IHttpClientBuilder AddHttpTimeoutHandler(this IHttpClientBuilder builder, TimeSpan timeout)
        {
            builder.Services.Configure<HttpTimeoutHandlerOptions>(opt => opt.DefaultTimeout = timeout);
            return builder.AddHttpTimeoutHandler();
        }

        public static IHttpClientBuilder AddHttpTimeoutHandler(
            this IHttpClientBuilder builder,
            Action<HttpTimeoutHandlerOptions> configure)
        {
            builder.Services.Configure<HttpTimeoutHandlerOptions>(opt => configure(opt));
            return builder.AddHttpTimeoutHandler();
        }

        public static IHttpClientBuilder AddHttpTimeoutHandler(this IHttpClientBuilder builder)
        {
            builder.Services.TryAddTransient<HttpTimeoutHandler>();

            // When you use TimeoutDelegatingHandler we don't want the individual http client
            // timeout to interfere so set it to infinite.
            builder.ConfigureHttpClient((_, httpClient) => httpClient.Timeout = Timeout.InfiniteTimeSpan);

            return builder.AddHttpMessageHandler(sp => sp.GetRequiredService<HttpTimeoutHandler>());
        }
    }
}
