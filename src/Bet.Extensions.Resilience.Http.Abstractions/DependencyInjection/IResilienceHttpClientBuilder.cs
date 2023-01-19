using Bet.Extensions.Resilience.Http.Abstractions.Options;

using Polly;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Provides builder pattern for <see cref="HttpClient"/> registration.
/// </summary>
public interface IResilienceHttpClientBuilder
{
    /// <summary>
    /// Gets the name of the client configured by this builder.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Name of the Options registered with the client.
    /// It can be the same as the Name or different.
    /// </summary>
    string OptionsName { get; }

    /// <summary>
    /// Gets the application service collection.
    /// </summary>
    IServiceCollection Services { get; }

    /// <summary>
    /// Configure <see cref="HttpClient"/> with options that have Root Section Name.
    /// </summary>
    /// <param name="rootSectionName">The name of the root section name for the options.</param>
    /// <param name="configureAction">The optional configuration that will be executed last. The default is null.</param>
    /// <returns></returns>
    IResilienceHttpClientBuilder ConfigureHttpClientOptions(string? rootSectionName = null, Action<HttpClientOptions>? configureAction = null);

    /// <summary>
    /// Configure <see cref="HttpClient"/> with options custom Root Section Name and Option Name.
    /// </summary>
    /// <param name="optionsSectionName">The options section name in the configurations.</param>
    /// <param name="rootSectionName">The name of the root section name for the options.</param>
    /// <param name="configureAction">The optional configuration that will be executed last. The default is null.</param>
    /// <returns></returns>
    IResilienceHttpClientBuilder ConfigureHttpClientOptions(string optionsSectionName, string? rootSectionName = null, Action<HttpClientOptions>? configureAction = null);

    /// <summary>
    /// Configure <see cref="HttpClient"/> with options custom Root Section Name and Option Name.
    /// </summary>
    /// <typeparam name="TOptions">The type of the options to be configured.</typeparam>
    /// <param name="rootSectionName">The name of the root section name for the options.</param>
    /// <param name="configureAction">The optional configuration that will be executed last. The default is null.</param>
    /// <returns></returns>
    IResilienceHttpClientBuilder ConfigureHttpClientOptions<TOptions>(string? rootSectionName = null, Action<TOptions>? configureAction = null) where TOptions : HttpClientOptions, new();

    /// <summary>
    /// Configure <see cref="HttpClient"/> with options custom instance of an object.
    /// </summary>
    /// <typeparam name="TOptions">The type of the options to be configured.</typeparam>
    /// <param name="optionsSectionName">The options section name in the configurations.</param>
    /// <param name="rootSectionName">The name of the root section name for the options.</param>
    /// <param name="configureAction">The optional configuration that will be executed last. The default is null.</param>
    /// <returns></returns>
    IResilienceHttpClientBuilder ConfigureHttpClientOptions<TOptions>(string optionsSectionName, string? rootSectionName = null, Action<TOptions>? configureAction = null) where TOptions : HttpClientOptions, new();

    /// <summary>
    /// Configures <see cref="HttpClient"/> Primary handler.
    /// </summary>
    /// <param name="handlerFactory">The delegate function for primary handler.</param>
    /// <returns></returns>
    IResilienceHttpClientBuilder ConfigurePrimaryHandler(Func<IServiceProvider, HttpMessageHandler>? handlerFactory = null);

    /// <summary>
    /// Configure Additional Delegating Handler.
    /// </summary>
    /// <param name="configureHandler"></param>
    /// <returns></returns>
    IResilienceHttpClientBuilder ConfigureHttpMessageHandler(Func<IServiceProvider, DelegatingHandler> configureHandler);

    /// <summary>
    /// Configure <see cref="IAsyncPolicy{TResult}"/> for <see cref="HttpClient"/>.
    /// </summary>
    /// <param name="policySelector"></param>
    /// <returns></returns>
    IResilienceHttpClientBuilder ConfigurePolicy(Func<IServiceProvider, HttpRequestMessage, IAsyncPolicy<HttpResponseMessage>> policySelector);

    /// <summary>
    /// Configure Default <see cref="Polly"/> policies.
    /// </summary>
    /// <returns></returns>
    IResilienceHttpClientBuilder ConfigureDefaultPolicies();

    /// <summary>
    /// Debugging the configurations.
    /// </summary>
    /// <returns></returns>
    (bool IsPrimaryHandlerSet, int OptionsCount, int DelegatingHandlerCount, int PolicyCount) Debug();
}
