using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;

using Bet.Extensions.Resilience.Http.MessageHandlers.PollyHttp;
using Bet.Extensions.Resilience.Http.Options;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Polly.Registry;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        private static readonly Func<IResilienceHttpClientBuilder, ResilienceHttpClientOptions>
          _findIntance = (builder) => (ResilienceHttpClientOptions)builder.Services.Single(sd => sd.ServiceType == typeof(ResilienceHttpClientOptions)).ImplementationInstance;

        private static readonly Func<IResilienceHttpClientBuilder, IConfiguration>
            _configuration = (builder) => (IConfiguration) builder.Services.Single(sd => sd.ServiceType == typeof(IConfiguration)).ImplementationInstance;

        public static IResilienceHttpClientBuilder AddResilienceTypedClient<TClient, TImplementation, TOptions>(
            this IServiceCollection services)
            where TClient : class
            where TImplementation : class, TClient
            where TOptions: HttpClientOptions, new()
        {
            var builder = AddDefaults<TClient>(services);

            var sectionName = typeof(TOptions).Name;

            services.AddTransient<IConfigureOptions<TOptions>>(sp =>
            {
                return new ConfigureOptions<TOptions>((options) =>
                {
                    var configuration = sp.GetRequiredService<IConfiguration>();
                    configuration.Bind(sectionName, options);
                });
            });

            Configure<TClient, TImplementation>(builder, sectionName);

            return builder;
        }

        public static IResilienceHttpClientBuilder AddResilienceTypedClient<TClient, TImplementation>(
            this IServiceCollection services,
            string sectionName = null)
            where TClient : class
            where TImplementation : class, TClient
        {
            var builder = AddDefaults<TClient>(services);

            builder.Services.AddTransient<IConfigureOptions<HttpClientOptions>>(sp =>
            {
                var implName = TypeNameHelper.GetTypeDisplayName(typeof(TImplementation), fullName: false);

                return new ConfigureNamedOptions<HttpClientOptions>(implName, (options) =>
                {
                    var configuration = sp.GetRequiredService<IConfiguration>();
                    configuration.Bind(sectionName ?? implName, options);
                });
            });

            Configure<TClient, TImplementation>(builder, sectionName);

            return builder;
        }

        private static IResilienceHttpClientBuilder AddDefaults<TClient>(IServiceCollection services)
            where TClient : class
        {
            // register default services
            services.TryAddSingleton(new ResilienceHttpClientOptions());
            services.TryAddPolicyRegistry();

            // create builder based on the name of the type
            var name = TypeNameHelper.GetTypeDisplayName(typeof(TClient), fullName: false);
            return new ResilienceHttpClientBuilder(services, name);
        }

        public static IResilienceHttpClientBuilder AddPrimaryHandler(
            this IResilienceHttpClientBuilder builder,
            Func<IServiceProvider, HttpMessageHandler> configure)
        {
            var instance = _findIntance(builder);

            if (instance.ClientOptions.TryGetValue(builder.Name, out var options))
            {
                options.PrimaryHandler = (sp) => configure(sp);
            }

            return builder;
        }

        public static IResilienceHttpClientBuilder ConfigureAll(
            this IResilienceHttpClientBuilder builder,
            Action<HttpClientOptionsBuilder> configure)
        {
            var instance = _findIntance(builder);

            if (instance.ClientOptions.TryGetValue(builder.Name, out var options))
            {
                configure?.Invoke(options);

                return builder;
            }

            var message = $"The HttpClient factory with the name '{builder.Name}' is not registered.";
            throw new InvalidOperationException(message);
        }

        /// <summary>
        /// Registers the provided <see cref="IPolicyRegistry{String}"/> in the service collection with service types
        /// <see cref="IPolicyRegistry{String}"/>, and <see cref="IReadOnlyPolicyRegistry{String}"/> and returns
        /// the provided registry.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        /// <param name="registry">The <see cref="IPolicyRegistry{String}"/>. The default value is null.</param>
        /// <returns>The provided <see cref="IPolicyRegistry{String}"/>.</returns>
        public static IPolicyRegistry<string> TryAddPolicyRegistry(
            this IServiceCollection services,
            IPolicyRegistry<string> registry = null)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (!services.Any(d=> d.ServiceType == typeof(IReadOnlyPolicyRegistry<string>)))
            {
                if (registry == null)
                {
                    registry = new PolicyRegistry();
                }

                services.AddSingleton<IPolicyRegistry<string>>(registry);
                services.AddSingleton<IReadOnlyPolicyRegistry<string>>(registry);
            }

            return registry;
        }

        public static IResilienceHttpClientBuilder Build(this IResilienceHttpClientBuilder builder)
        {
            var registry = _findIntance(builder);

            if (registry.ClientOptions.TryGetValue(builder.Name, out var options))
            {
                var httpBuilder = options.HttpClientBuilder;

                httpBuilder.SetHandlerLifetime(options.ClientOptions.Timeout);

                httpBuilder.ConfigurePrimaryHttpMessageHandler(options.PrimaryHandler);

                // allows for multiple configurations to be registered.
                foreach (var httpClientAction in options.HttpClientActions)
                {
                    httpBuilder.ConfigureHttpClient(httpClientAction);
                }

                foreach (var messageHander in options.AdditionalHandlers)
                {
                    httpBuilder.AddHttpMessageHandler((sp) => messageHander(sp));
                }

                foreach (var policy in options.Policies)
                {
                    if (options.EnableLogging)
                    {
                        httpBuilder.AddHttpMessageHandler((sp) =>
                        {
                            var logger = sp.GetRequiredService<ILoggerFactory>().CreateLogger($"System.Net.Http.HttpClient.{builder.Name}");
                            return new PolicyWithLoggingHttpMessageHandler((request) => policy(sp, request), logger);
                        });
                    }
                    else
                    {
                        httpBuilder.AddPolicyHandler(policy);
                    }
                }

                return builder;
            }

            var message = $"The HttpClient factory with the name '{builder.Name}' is not registered.";
            throw new InvalidOperationException(message);
        }

        private static void Configure<TClient, TImplementation>(
            IResilienceHttpClientBuilder builder,
            string sectionName = null)
            where TClient : class
            where TImplementation : class, TClient
        {
            var instance = _findIntance(builder);

            // the instance of the resilience client wasn't added yet.
            if (!instance.ClientOptions.TryGetValue(builder.Name, out var options))
            {
                var httpClientBuilder = builder.Services.AddHttpClient<TClient, TImplementation>();

                var newOptions = new HttpClientOptionsBuilder(builder.Name, httpClientBuilder);

                var configuration = (IConfiguration)builder.Services.Single(sd => sd.ServiceType == typeof(IConfiguration)).ImplementationInstance;

                // configures default values from configuration provider.
                if (configuration != null)
                {
                    sectionName = string.IsNullOrWhiteSpace(sectionName) ? typeof(TImplementation).Name : sectionName;
                    newOptions.SectionName = sectionName;
                    configuration.Bind(sectionName, newOptions.ClientOptions);

                    newOptions.HttpClientActions.Add((sp, client) =>
                    {
                        if (newOptions.ClientOptions?.BaseAddress != null)
                        {
                            client.BaseAddress = newOptions.ClientOptions.BaseAddress;
                        }

                        if (newOptions.ClientOptions?.Timeout != null)
                        {
                            client.Timeout = newOptions.ClientOptions.Timeout;
                        }

                        if (newOptions.ClientOptions?.ContentType != null)
                        {
                            client.DefaultRequestHeaders.Add("accept", newOptions.ClientOptions.ContentType);
                            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(newOptions.ClientOptions.ContentType));
                        }
                    });
                }

                instance.ClientOptions[builder.Name] = newOptions;

                return;
            }

            var message = $"The HttpClient factory already has a registered client with the name '{builder.Name}'";
            throw new InvalidOperationException(message);
        }
    }
}
