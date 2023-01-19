using Bet.Extensions.Http.MessageHandlers.CorrelationId;

using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extensions on the <see cref="IServiceCollection"/>.
/// </summary>
public static class CorrelationIdServiceCollectionExtensions
{
    /// <summary>
    /// Adds required services to support the Correlation ID functionality.
    /// </summary>
    /// <param name="serviceCollection"></param>
    public static IServiceCollection AddCorrelationId(this IServiceCollection serviceCollection)
    {
        serviceCollection.TryAddSingleton<ICorrelationContextAccessor, CorrelationContextAccessor>();
        serviceCollection.TryAddTransient<ICorrelationContextFactory, CorrelationContextFactory>();

        return serviceCollection;
    }

    public static IResilienceHttpClientBuilder ConfigureCorrelationIdHandler(this IResilienceHttpClientBuilder builder)
    {
        return builder;
    }
}
