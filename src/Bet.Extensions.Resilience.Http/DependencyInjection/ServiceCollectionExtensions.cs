using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;

using Bet.Extensions.Resilience.Abstractions;
using Bet.Extensions.Resilience.Http.MessageHandlers.PollyHttp;
using Bet.Extensions.Resilience.Http.Options;
using Bet.Extensions.Resilience.Http.Setup;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Registry;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        private static readonly Func<IResilienceHttpClientBuilder, ResilienceHttpClientOptions>
          _findIntance = (builder) => (ResilienceHttpClientOptions)builder.Services.Single(sd => sd.ServiceType == typeof(ResilienceHttpClientOptions)).ImplementationInstance;

        private static readonly Func<IResilienceHttpClientBuilder, IConfiguration>
            _configuration = (builder) => builder.Services.SingleOrDefault(sd => sd.ServiceType == typeof(IConfiguration))?.ImplementationInstance as IConfiguration;

        /// <summary>
        /// Adds Resilience <see cref="HttpClient"/> with custom options that can be used to inject inside of the TypedClient.
        /// </summary>
        /// <typeparam name="TClient">The interface for the typed client.</typeparam>
        /// <typeparam name="TImplementation">The implementation of the typed client./</typeparam>
        /// <typeparam name="TOptions">The options type to be used to register with DI.</typeparam>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        /// <param name="sectionName">The configuration section name if provided overrides the TImplementation type for the configuration.
        ///                           The default is null; thus TOptions type is used for the
        /// </param>
        /// <returns>IResilienceHttpClientBuilder</returns>
        public static IResilienceHttpClientBuilder AddResilienceTypedClient<TClient, TImplementation, TOptions>(
            this IServiceCollection services,
            string sectionName = null)
            where TClient : class
            where TImplementation : class, TClient
            where TOptions: HttpClientOptions, new()
        {
            var builder = AddResilienceHttpClientBuilder<TClient>(services);

            sectionName = sectionName ?? typeof(TOptions).Name;

            // create options instance from configuration
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

        /// <summary>
        /// Adds Resilience <see cref="HttpClient"/>.
        /// </summary>
        /// <typeparam name="TClient">The interface for the typed client.</typeparam>
        /// <typeparam name="TImplementation">The implementation of the typed client./</typeparam>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        /// <param name="sectionName">The configuration section name if provided overrides the TImplementation type name.
        ///                           The default is null.
        /// </param>
        /// <returns></returns>
        public static IResilienceHttpClientBuilder AddResilienceTypedClient<TClient, TImplementation>(
            this IServiceCollection services,
            string sectionName = null)
            where TClient : class
            where TImplementation : class, TClient
        {
            var builder = AddResilienceHttpClientBuilder<TClient>(services);

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

        /// <summary>
        /// Adds Default Http Policies to be executed within the context of the request.
        /// WaitAndRetry and CircuitBreaker
        /// </summary>
        /// <param name="builder">The <see cref="IResilienceHttpClientBuilder"/> instance to configure.</param>
        /// <param name="enableLogging">The configuration of the default policies to log the output. The default is false.</param>
        /// <returns></returns>
        public static IResilienceHttpClientBuilder AddDefaultPolicies(
            this IResilienceHttpClientBuilder builder,
            bool enableLogging = false)
        {
            builder.Services.AddTransient<IConfigureOptions<HttpPolicyOptions>>(sp =>
            {
                return new ConfigureOptions<HttpPolicyOptions>((options) =>
                {
                    var configuration = sp.GetRequiredService<IConfiguration>();
                    configuration.Bind(Constants.Policies, options);
                });
            });

            var instance = _findIntance(builder);

            if (instance.RegisteredBuilders.TryGetValue(builder.Name, out var clientOptions))
            {
                clientOptions.EnableLogging = enableLogging;

                IAsyncPolicy<HttpResponseMessage> retryPolicy(IServiceProvider sp, HttpRequestMessage request)
                {
                    var httpPolicyOptions = sp.GetRequiredService<IOptions<HttpPolicyOptions>>().Value;
                    return Policies.GetRetryAsync(httpPolicyOptions.HttpRetry.Count, httpPolicyOptions.HttpRetry.BackoffPower);
                }

                AddPollyPolicy(clientOptions, retryPolicy);

                IAsyncPolicy<HttpResponseMessage> circuitBreakerPolicy(IServiceProvider sp, HttpRequestMessage request)
                {
                    var httpPolicyOptions = sp.GetRequiredService<IOptions<HttpPolicyOptions>>().Value;
                    return Policies.GetCircuitBreakerAsync(
                        httpPolicyOptions.HttpCircuitBreaker.ExceptionsAllowedBeforeBreaking,
                        httpPolicyOptions.HttpCircuitBreaker.DurationOfBreak);
                }

                AddPollyPolicy(clientOptions, circuitBreakerPolicy);
            }

            return builder;
        }

        /// <summary>
        /// Adds <see cref="HttpClient"/> primary message handler.
        /// </summary>
        /// <param name="builder">The <see cref="IResilienceHttpClientBuilder"/> instance to configure.</param>
        /// <param name="configure">The delegate to configure Primary Http MessageHandler for the <see cref="HttpClient"/> client.</param>
        /// <returns></returns>
        public static IResilienceHttpClientBuilder AddPrimaryHttpMessageHandler(
            this IResilienceHttpClientBuilder builder,
            Func<IServiceProvider, HttpMessageHandler> configure)
        {
            var instance = _findIntance(builder);

            if (instance.RegisteredBuilders.TryGetValue(builder.Name, out var options))
            {
                if (!options.IsPrimaryHanlderAdded)
                {
                    options.PrimaryHandler = (sp) => configure(sp);
                    options.HttpClientBuilder.ConfigurePrimaryHttpMessageHandler(options.PrimaryHandler);
                    options.IsPrimaryHanlderAdded = true;
                }
                else
                {
                    throw new InvalidOperationException("Primary Handler was already added");
                }
            }

            return builder;
        }

        /// <summary>
        /// Adds <see cref="HttpClient"/> configuration for the client.
        /// </summary>
        /// <param name="builder">The <see cref="IResilienceHttpClientBuilder"/> instance to configure.</param>
        /// <param name="configureClient">The delegate to configure <see cref="HttpClient"/> properties.</param>
        /// <returns></returns>
        public static IResilienceHttpClientBuilder AddHttpClientConfiguration(
            this IResilienceHttpClientBuilder builder,
            Action<IServiceProvider, HttpClient> configureClient)
        {
            var instance = _findIntance(builder);

            if (instance.RegisteredBuilders.TryGetValue(builder.Name, out var options))
            {
                options.HttpClientActions.Add(configureClient);
                options.HttpClientBuilder.ConfigureHttpClient(configureClient);
            }

            return builder;
        }

        /// <summary>
        /// Adds Http Message Handler, will be executed in the order that added.
        /// </summary>
        /// <param name="builder">The <see cref="IResilienceHttpClientBuilder"/> instance to configure.</param>
        /// <param name="configureHandler">The delegate to configure additional http message handler.</param>
        /// <returns></returns>
        public static IResilienceHttpClientBuilder AddHttpMessageHandler(
            this IResilienceHttpClientBuilder builder,
            Func<IServiceProvider, DelegatingHandler> configureHandler)
        {
            var instance = _findIntance(builder);

            if (instance.RegisteredBuilders.TryGetValue(builder.Name, out var options))
            {
                options.AdditionalHandlers.Add(configureHandler);
                options.HttpClientBuilder.ConfigurePrimaryHttpMessageHandler(configureHandler);
            }
            return builder;
        }

        /// <summary>
        /// Adds Polly policy to be executed.
        /// </summary>
        /// <param name="builder">The <see cref="IResilienceHttpClientBuilder"/> instance to configure.</param>
        /// <param name="policySelector">The delegate to configure Polly policy for the handlers.</param>
        /// <returns></returns>
        public static IResilienceHttpClientBuilder AddPolicy(
            this IResilienceHttpClientBuilder builder,
            Func<IServiceProvider, HttpRequestMessage, IAsyncPolicy<HttpResponseMessage>> policySelector)
        {
                    var instance = _findIntance(builder);

                    if (instance.RegisteredBuilders.TryGetValue(builder.Name, out var options))
                    {
                        options.Policies.Add(policySelector);
                        options.HttpClientBuilder.AddPolicyHandler(policySelector);
            }
            return builder;
        }

        /// <summary>
        /// Adds all of the <see cref="HttpClient"/> configurations at once.
        /// </summary>
        /// <param name="builder">The <see cref="IResilienceHttpClientBuilder"/> instance to configure.</param>
        /// <param name="configure">The delegate to configure <see cref="HttpClient"/></param>
        /// <returns></returns>
        public static IResilienceHttpClientBuilder AddAllConfigurations(
            this IResilienceHttpClientBuilder builder,
            Action<HttpClientOptionsBuilder> configure)
        {
            var instance = _findIntance(builder);

            if (instance.RegisteredBuilders.TryGetValue(builder.Name, out var options))
            {
                configure?.Invoke(options);

                var httpBuilder = options.HttpClientBuilder;

                // the default is 2 min.
                httpBuilder.SetHandlerLifetime(options.HttpClientOptions.Timeout);

                // allows to register new primary handler.
                httpBuilder.ConfigurePrimaryHttpMessageHandler(options.PrimaryHandler);
                options.IsPrimaryHanlderAdded = true;

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
                    AddPollyPolicy(options, policy);
                }

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

            if (!services.Any(d=> d.ServiceType == typeof(IReadOnlyPolicyRegistry<string>))
                || !services.Any(d=> d.ServiceType == typeof(IPolicyRegistry<string>)))
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

        private static void AddPollyPolicy(
            HttpClientOptionsBuilder options,
            Func<IServiceProvider, HttpRequestMessage, Polly.IAsyncPolicy<HttpResponseMessage>> policy)
        {
            if (options.EnableLogging)
            {
                options.HttpClientBuilder.AddHttpMessageHandler((sp) =>
                {
                    var logger = sp.GetRequiredService<ILoggerFactory>().CreateLogger($"System.Net.Http.HttpClient.{options.Name}");
                    return new PolicyWithLoggingHttpMessageHandler((request) => policy(sp, request), logger,options.Name);
                });
            }
            else
            {
                options.HttpClientBuilder.AddPolicyHandler(policy);
            }
        }

        /// <summary>
        /// Creates <see cref="ResilienceHttpClientBuilder"/>.
        /// </summary>
        /// <typeparam name="TClient"></typeparam>
        /// <param name="services"></param>
        /// <returns></returns>
        private static IResilienceHttpClientBuilder AddResilienceHttpClientBuilder<TClient>(IServiceCollection services) where TClient : class
        {
            // register default services
            services.TryAddSingleton(new ResilienceHttpClientOptions());

            // create builder based on the name of the type
            // this matches the HttpFactoryClient logic https://github.com/aspnet/Extensions/blob/11cf90103841c35cbefe9afb8e5bf9fee696dd17/src/HttpClientFactory/Http/src/DependencyInjection/HttpClientFactoryServiceCollectionExtensions.cs#L517
            var name = TypeNameHelper.GetTypeDisplayName(typeof(TClient), fullName: false);
            return new ResilienceHttpClientBuilder(services, name);
        }

        private static void Configure<TClient, TImplementation>(
            IResilienceHttpClientBuilder builder,
            string sectionName = null)
            where TClient : class
            where TImplementation : class, TClient
        {
            var instance = _findIntance(builder);

            // the instance of the resilience client wasn't added yet, so add it only once.
            if (!instance.RegisteredBuilders.TryGetValue(builder.Name, out var options))
            {
                // Create Typed Client HttpFactoryClient
                var httpClientBuilder = builder.Services.AddHttpClient<TClient, TImplementation>();

                var newOptions = new HttpClientOptionsBuilder(builder.Name, httpClientBuilder);

                // configures default values from configuration provider if present.
                if (_configuration(builder) is IConfiguration configuration)
                {
                    sectionName = string.IsNullOrWhiteSpace(sectionName) ? typeof(TImplementation).Name : sectionName;
                    newOptions.SectionName = sectionName;
                    configuration.Bind(sectionName, newOptions.HttpClientOptions);

                    newOptions.HttpClientActions.Add((sp, client) =>
                    {
                        if (newOptions.HttpClientOptions?.BaseAddress != null)
                        {
                            client.BaseAddress = newOptions.HttpClientOptions.BaseAddress;
                        }

                        if (newOptions.HttpClientOptions?.Timeout != null)
                        {
                            client.Timeout = newOptions.HttpClientOptions.Timeout;
                        }

                        if (newOptions.HttpClientOptions?.ContentType != null)
                        {
                            client.DefaultRequestHeaders.Add("accept", newOptions.HttpClientOptions.ContentType);
                            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(newOptions.HttpClientOptions.ContentType));
                        }
                    });

                    // configure default actions
                    foreach (var httpClientAction in newOptions.HttpClientActions)
                    {
                        httpClientBuilder.ConfigureHttpClient(httpClientAction);
                    }
                }

                instance.RegisteredBuilders[builder.Name] = newOptions;

                return;
            }

            var message = $"The HttpClient factory already has a registered client with the name '{builder.Name}'";
            throw new InvalidOperationException(message);
        }
    }
}
