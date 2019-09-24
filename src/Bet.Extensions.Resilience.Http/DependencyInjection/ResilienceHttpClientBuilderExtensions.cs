using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;

using Bet.Extensions.Http.MessageHandlers.Abstractions.Options;
using Bet.Extensions.Resilience.Abstractions;
using Bet.Extensions.Resilience.Http.Options;
using Bet.Extensions.Resilience.Http.Policies;
using Bet.Extensions.Resilience.Http.Setup;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Http.MessageHandlers;
using Microsoft.Extensions.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Polly;
using Polly.Registry;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ResilienceHttpClientBuilderExtensions
    {
        private const string _message = "The HttpClient factory with the name '{0}' is not registered.";

        private static readonly Func<IResilienceHttpClientBuilder, HttpClientOptionsBuilderRegistrant>
          _findHttpBuilderIntance = (builder) => builder.Services.SingleOrDefault(sd => sd.ServiceType == typeof(HttpClientOptionsBuilderRegistrant))?.ImplementationInstance as HttpClientOptionsBuilderRegistrant;

        private static readonly Func<IResilienceHttpClientBuilder, IConfiguration> _configuration = GetConfiguration;

        /// <summary>
        /// Adds Resilience <see cref="HttpClient"/> with custom options that can be used to inject inside of the TypedClient.
        /// </summary>
        /// <typeparam name="TClient">The interface for the typed client.</typeparam>
        /// <typeparam name="TImplementation">The implementation of the typed client./.</typeparam>
        /// <typeparam name="TOptions">The options type to be used to register with DI.</typeparam>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        /// <param name="sectionName">The default is null and resolves TOptions name as root.</param>
        /// <param name="optionsName">The default is TOptions name.</param>
        /// <returns>IResilienceHttpClientBuilder.</returns>
        public static IResilienceHttpClientBuilder AddResilienceTypedClient<TClient, TImplementation, TOptions>(
            this IServiceCollection services,
            string sectionName = null,
            string optionsName = null) where TClient : class where TImplementation : class, TClient where TOptions : HttpClientOptions, new()
        {
            var builder = AddResilienceHttpClientBuilder<TClient>(services);
            optionsName = optionsName ?? typeof(TOptions).Name;

            // adds default HttpOptionsConfiguration
            var implName = typeof(TImplementation).Name;
            AddNamedHttpClientOptions(sectionName, optionsName, builder, implName);

            // create options instance from the configuration
            services.AddTransient<IConfigureOptions<TOptions>>(sp =>
            {
                return new ConfigureOptions<TOptions>((options) =>
                {
                    var configuration = sp.GetRequiredService<IConfiguration>();

                    if (sectionName == null)
                    {
                        configuration.Bind(optionsName, options);
                    }
                    else
                    {
                        var section = configuration.GetSection(sectionName);
                        section.Bind(optionsName, options);
                    }
                });
            });

            Configure<TClient, TImplementation>(builder, sectionName, optionsName);

            return builder;
        }

        /// <summary>
        /// Adds Resilience <see cref="HttpClient"/>.
        /// </summary>
        /// <typeparam name="TClient">The interface for the typed client.</typeparam>
        /// <typeparam name="TImplementation">The implementation of the typed client./.</typeparam>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        /// <param name="sectionName">The default is null and resolves TOptions name as root.</param>
        /// <param name="optionsName">The default is TOptions name.</param>
        /// <returns></returns>
        public static IResilienceHttpClientBuilder AddResilienceTypedClient<TClient, TImplementation>(
            this IServiceCollection services,
            string sectionName = null,
            string optionsName = null)
            where TClient : class
            where TImplementation : class, TClient
        {
            var builder = AddResilienceHttpClientBuilder<TClient>(services);

            var implName = typeof(TImplementation).Name;

            // if not optionName present then Implementation name is used.
            optionsName = optionsName ?? implName;

            AddNamedHttpClientOptions(sectionName, optionsName, builder, implName);

            Configure<TClient, TImplementation>(builder, sectionName, optionsName);

            return builder;
        }

        /// <summary>
        /// Adds Default Http Policies to be executed within the context of the request.
        /// WaitAndRetry and CircuitBreaker.
        /// </summary>
        /// <param name="builder">The <see cref="IResilienceBuilder"/> instance to configure.</param>
        /// <param name="enableLogging">The configuration of the default policies to log the output. The default is false.</param>
        /// <param name="sectionName"></param>
        /// <returns></returns>
        public static IResilienceHttpClientBuilder AddDefaultPolicies(
            this IResilienceHttpClientBuilder builder,
            bool enableLogging = false,
            string sectionName = Constants.Policies)
        {
            builder.Services.AddTransient<IConfigureOptions<HttpPolicyOptions>>(sp =>
            {
                return new ConfigureOptions<HttpPolicyOptions>((options) =>
                {
                    var configuration = sp.GetRequiredService<IConfiguration>();
                    configuration.Bind(sectionName, options);
                });
            });

            var instance = _findHttpBuilderIntance(builder);

            if (instance.RegisteredHttpClientBuilders.TryGetValue(builder.Name, out var clientOptions))
            {
                clientOptions.EnableLogging = enableLogging;

                IAsyncPolicy<HttpResponseMessage> TimeoutPolicy(IServiceProvider sp, HttpRequestMessage request)
                {
                    var httpTimeoutOptions = sp.GetRequiredService<IOptions<HttpPolicyOptions>>().Value;
                    return Policies.GetTimeoutAsync(httpTimeoutOptions.Timeout);
                }

                AddPollyPolicy(clientOptions, TimeoutPolicy);

                IAsyncPolicy<HttpResponseMessage> RetryPolicy(IServiceProvider sp, HttpRequestMessage request)
                {
                    var httpPolicyOptions = sp.GetRequiredService<IOptions<HttpPolicyOptions>>().Value;
                    return Policies.GetRetryAsync(httpPolicyOptions.HttpRetry.Count, httpPolicyOptions.HttpRetry.BackoffPower);
                }

                AddPollyPolicy(clientOptions, RetryPolicy);

                IAsyncPolicy<HttpResponseMessage> CircuitBreakerPolicy(IServiceProvider sp, HttpRequestMessage request)
                {
                    var httpPolicyOptions = sp.GetRequiredService<IOptions<HttpPolicyOptions>>().Value;
                    return Policies.GetCircuitBreakerAsync(
                        httpPolicyOptions.HttpCircuitBreaker.ExceptionsAllowedBeforeBreaking,
                        httpPolicyOptions.HttpCircuitBreaker.DurationOfBreak);
                }

                AddPollyPolicy(clientOptions, CircuitBreakerPolicy);

                return builder;
            }

            throw new InvalidOperationException(string.Format(_message, builder.Name));
        }

        /// <summary>
        /// Adds <see cref="HttpClient"/> primary message handler.
        /// </summary>
        /// <param name="builder">The <see cref="IResilienceBuilder"/> instance to configure.</param>
        /// <param name="configure">The delegate to configure Primary Http MessageHandler for the <see cref="HttpClient"/> client.</param>
        /// <returns></returns>
        public static IResilienceHttpClientBuilder AddPrimaryHttpMessageHandler(
            this IResilienceHttpClientBuilder builder,
            Func<IServiceProvider, HttpMessageHandler> configure)
        {
            var instance = _findHttpBuilderIntance(builder);

            if (instance.RegisteredHttpClientBuilders.TryGetValue(builder.Name, out var options))
            {
                if (!options.IsPrimaryHanlderAdded)
                {
                    options.PrimaryHandler = (sp) => configure(sp);
                    options.HttpClientBuilder.ConfigurePrimaryHttpMessageHandler(options.PrimaryHandler);
                    options.IsPrimaryHanlderAdded = true;

                    return builder;
                }
                else
                {
                    throw new InvalidOperationException("Primary Handler was already added");
                }
            }

            throw new InvalidOperationException(string.Format(_message, builder.Name));
        }

        /// <summary>
        /// Adds <see cref="HttpClient"/> configuration for the client.
        /// </summary>
        /// <param name="builder">The <see cref="IResilienceBuilder"/> instance to configure.</param>
        /// <param name="configureClient">The delegate to configure <see cref="HttpClient"/> properties.</param>
        /// <returns></returns>
        public static IResilienceHttpClientBuilder AddHttpClientConfiguration(
            this IResilienceHttpClientBuilder builder,
            Action<IServiceProvider, HttpClient> configureClient)
        {
            var instance = _findHttpBuilderIntance(builder);

            if (instance.RegisteredHttpClientBuilders.TryGetValue(builder.Name, out var options))
            {
                options.ConfigureHttpClient.Add(configureClient);
                options.HttpClientBuilder.ConfigureHttpClient(configureClient);

                return builder;
            }

            throw new InvalidOperationException(string.Format(_message, builder.Name));
        }

        /// <summary>
        /// Adds Http Message Handler, will be executed in the order that added.
        /// </summary>
        /// <param name="builder">The <see cref="IResilienceBuilder"/> instance to configure.</param>
        /// <param name="configureHandler">The delegate to configure additional http message handler.</param>
        /// <returns></returns>
        public static IResilienceHttpClientBuilder AddHttpMessageHandler(
            this IResilienceHttpClientBuilder builder,
            Func<IServiceProvider, DelegatingHandler> configureHandler)
        {
            var instance = _findHttpBuilderIntance(builder);

            if (instance.RegisteredHttpClientBuilders.TryGetValue(builder.Name, out var options))
            {
                options.AdditionalHandlers.Add(configureHandler);
                options.HttpClientBuilder.ConfigurePrimaryHttpMessageHandler(configureHandler);

                return builder;
            }

            throw new InvalidOperationException(string.Format(_message, builder.Name));
        }

        /// <summary>
        /// Adds Polly policy to be executed.
        /// </summary>
        /// <param name="builder">The <see cref="IResilienceBuilder"/> instance to configure.</param>
        /// <param name="policySelector">The delegate to configure Polly policy for the handlers.</param>
        /// <returns></returns>
        public static IResilienceHttpClientBuilder AddPolicy(
            this IResilienceHttpClientBuilder builder,
            Func<IServiceProvider, HttpRequestMessage, IAsyncPolicy<HttpResponseMessage>> policySelector)
        {
            var instance = _findHttpBuilderIntance(builder);

            if (instance.RegisteredHttpClientBuilders.TryGetValue(builder.Name, out var options))
            {
                options.Policies.Add(policySelector);
                options.HttpClientBuilder.AddPolicyHandler(policySelector);

                return builder;
            }

            throw new InvalidOperationException(string.Format(_message, builder.Name));
        }

        /// <summary>
        /// Adds all of the <see cref="HttpClient"/> configurations at once.
        /// </summary>
        /// <param name="builder">The <see cref="IResilienceBuilder"/> instance to configure.</param>
        /// <param name="configure">The delegate to configure <see cref="HttpClient"/>.</param>
        /// <returns></returns>
        public static IResilienceHttpClientBuilder AddAllConfigurations(
            this IResilienceHttpClientBuilder builder,
            Action<HttpClientOptionsBuilder> configure)
        {
            var instance = _findHttpBuilderIntance(builder);

            if (instance.RegisteredHttpClientBuilders.TryGetValue(builder.Name, out var options))
            {
                configure?.Invoke(options);

                var httpBuilder = options.HttpClientBuilder;

                // the default is 2 min.
                httpBuilder.SetHandlerLifetime(options.Options.Timeout);

                // allows to register new primary handler.
                httpBuilder.ConfigurePrimaryHttpMessageHandler(options.PrimaryHandler);
                options.IsPrimaryHanlderAdded = true;

                // allows for multiple configurations to be registered.
                foreach (var httpClientAction in options.ConfigureHttpClient)
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

            throw new InvalidOperationException(string.Format(_message, builder.Name));
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

        /// <summary>
        /// This is required by both registrations,
        /// in order to get the basic configurations passed to DI.
        /// </summary>
        /// <param name="sectionName"></param>
        /// <param name="optionsName"></param>
        /// <param name="builder"></param>
        /// <param name="implName"></param>
        private static void AddNamedHttpClientOptions(
            string sectionName,
            string optionsName,
            IResilienceHttpClientBuilder builder,
            string implName)
        {
            builder.Services.AddTransient<IConfigureOptions<HttpClientOptions>>(sp =>
            {
                // this always must be associated with TImplementation
                return new ConfigureNamedOptions<HttpClientOptions>(implName, (options) =>
                {
                    var configuration = sp.GetRequiredService<IConfiguration>();
                    if (sectionName == null)
                    {
                        configuration.Bind(optionsName, options);
                    }
                    else
                    {
                        var section = configuration.GetSection(sectionName);
                        section.Bind(optionsName, options);
                    }
                });
            });
        }

        private static void AddPollyPolicy(
            HttpClientOptionsBuilder options,
            Func<IServiceProvider, HttpRequestMessage, IAsyncPolicy<HttpResponseMessage>> policy)
        {
            if (options.EnableLogging)
            {
                options.HttpClientBuilder.AddHttpMessageHandler((sp) =>
                {
                    var logger = sp.GetRequiredService<ILoggerFactory>().CreateLogger($"System.Net.Http.HttpClient.{options.Name}");
                    return new PolicyWithLoggingHttpMessageHandler((request) => policy(sp, request), logger, options.Name);
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
            // register the default service.
            services.TryAddSingleton(new HttpClientOptionsBuilderRegistrant());

            // create builder based on the name of the type
            // this matches the HttpFactoryClient logic https://github.com/aspnet/Extensions/blob/11cf90103841c35cbefe9afb8e5bf9fee696dd17/src/HttpClientFactory/Http/src/DependencyInjection/HttpClientFactoryServiceCollectionExtensions.cs#L517
            var name = TypeNameHelper.GetTypeDisplayName(typeof(TClient), fullName: false);
            return new ResilienceHttpClientBuilder(services, name);
        }

        private static void Configure<TClient, TImplementation>(
            IResilienceHttpClientBuilder builder,
            string sectionName = null,
            string optionsName = null)
            where TClient : class
            where TImplementation : class, TClient
        {
            var instance = _findHttpBuilderIntance(builder);

            // the instance of the resilience client wasn't added yet, so add it only once.
            if (!instance.RegisteredHttpClientBuilders.TryGetValue(builder.Name, out var options))
            {
                // Create Typed Client HttpFactoryClient
                var httpClientBuilder = builder.Services.AddHttpClient<TClient, TImplementation>();

                var newOptions = new HttpClientOptionsBuilder(builder.Name, httpClientBuilder);

                newOptions.ConfigureHttpClient.Add((sp, client) =>
                {
                    var config = sp.GetRequiredService<IOptionsMonitor<HttpClientOptions>>().Get(typeof(TImplementation).Name);

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

                // configures default values from configuration provider if present.
                if (_configuration(builder) is IConfiguration configuration)
                {
                    if (sectionName == null)
                    {
                        configuration.Bind(optionsName, newOptions.Options);
                    }
                    else
                    {
                        var section = configuration.GetSection(sectionName);
                        section.Bind(optionsName, newOptions.Options);
                    }
                }

                // configure default actions
                foreach (var httpClientAction in newOptions.ConfigureHttpClient)
                {
                    httpClientBuilder.ConfigureHttpClient(httpClientAction);
                }

                instance.RegisteredHttpClientBuilders[builder.Name] = newOptions;

                return;
            }

            throw new InvalidOperationException(string.Format(_message, builder.Name));
        }

#if NETSTANDARD2_0
        private static IConfiguration GetConfiguration(IResilienceBuilder builder)
        {
            return builder.Services.SingleOrDefault(sd => sd.ServiceType == typeof(IConfiguration))?.ImplementationInstance as IConfiguration;
        }
#elif NETSTANDARD2_1
        private static IConfiguration GetConfiguration(IResilienceBuilder builder)
        {
            // builder.Services.BuildServiceProvider() is not the best implementation but required for getting configurations at this point.
            return builder
                .Services
                .SingleOrDefault(sd => sd.ServiceType == typeof(IConfiguration))?
                .ImplementationFactory?
                .Invoke(builder.Services.BuildServiceProvider()) as IConfiguration;
        }
#endif
    }
}
