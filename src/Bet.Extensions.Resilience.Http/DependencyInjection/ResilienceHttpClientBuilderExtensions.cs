using System.Net.Http.Headers;

using Bet.Extensions.Resilience.Http.Abstractions.Options;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Internal;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ResilienceHttpClientBuilderExtensions
    {
        /// <summary>
        /// Adds <see cref="IResilienceHttpClientBuilder"/>.
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
        /// Configures <see cref="HttpClient"/> options.
        /// </summary>
        /// <typeparam name="TOptions">The type of the options.</typeparam>
        /// <param name="builder">The <see cref="IHttpClientBuilder"/>.</param>
        /// <param name="optionsSectionName">
        /// The options name section.
        /// The default is null.
        /// The result is 'builder.Name' value which is usually equals interface name.
        /// </param>
        /// <param name="optionsName">
        /// The options name to be registered within the Options system.
        /// The default is null.
        /// The result is 'builder.Name' value which is usually equals interface name.
        /// </param>
        /// <param name="rootSectionName">The root section for the options.
        /// It is possible to group multiple options configurations.
        /// The default is null.
        /// </param>
        /// <param name="configureAction">
        /// The configuration action to be used last in the configure pipeline.
        /// The default is null.
        /// </param>
        /// <returns></returns>
        public static IHttpClientBuilder ConfigureOptions<TOptions>(
           this IHttpClientBuilder builder,
           string? optionsSectionName = null,
           string? optionsName = null,
           string? rootSectionName = null,
           Action<TOptions>? configureAction = null) where TOptions : HttpClientOptions, new()
        {
            optionsSectionName ??= builder.Name;

            optionsName ??= builder.Name;

            builder.Services.AddSingleton((Func<IServiceProvider, IOptionsChangeTokenSource<TOptions>>)((sp) =>
            {
                var configuration = sp.GetRequiredService<IConfiguration>();

                if (rootSectionName == null)
                {
                    return new ConfigurationChangeTokenSource<TOptions>(optionsName, configuration);
                }
                else
                {
                    var section = configuration.GetSection(rootSectionName);
                    return new ConfigurationChangeTokenSource<TOptions>(optionsName, section);
                }
            }));

            builder.Services.AddSingleton<IConfigureOptions<TOptions>>(sp =>
            {
                // if optionName = string.Empty then default value exist
                return new ConfigureNamedOptions<TOptions>(optionsName, (options) =>
                {
                    var configuration = sp.GetRequiredService<IConfiguration>();
                    if (rootSectionName == null)
                    {
                        configuration.Bind(optionsSectionName, options);
                    }
                    else
                    {
                        var section = configuration.GetSection(rootSectionName);
                        section.Bind(optionsSectionName, options);
                    }

                    configureAction?.Invoke(options);
                });
            });

            // makes this option accessible without IOptions interface.
            builder.Services.AddSingleton(resolver => resolver.GetRequiredService<IOptions<TOptions>>().Value);

            // Registers an IConfigureOptions<TOptions> action configurator.
            // Being last it will bind from configuration source first
            // and run the customization afterwards
            builder.Services.Configure<TOptions>(optionsName, options => configureAction?.Invoke(options));

            builder.ConfigureHttpClient((sp, client) =>
            {
                var config = sp.GetRequiredService<IOptionsMonitor<TOptions>>().Get(optionsName);

                if (config?.BaseAddress != null)
                {
                    client.BaseAddress = config.BaseAddress;
                }

                if (config?.Timeout != null)
                {
                    client.Timeout = config.Timeout;
                }

                if (config?.ContentType != null)
                {
                    client.DefaultRequestHeaders.Add("accept", config.ContentType);
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(config.ContentType));
                }
            });

            return builder;
        }
    }
}
