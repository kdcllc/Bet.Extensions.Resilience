using System.Net.Http.Headers;

using Bet.Extensions.Resilience.Abstractions;
using Bet.Extensions.Resilience.Abstractions.Options;
using Bet.Extensions.Resilience.Http.Abstractions.Options;
using Bet.Extensions.Resilience.Http.Options;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Internal;
using Microsoft.Extensions.Options;

using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using Polly.Timeout;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <inheritdoc/>
    internal class ResilienceHttpTypedClientBuilder<TClient, TImplementation> : IResilienceHttpClientBuilder
        where TClient : class where TImplementation : class, TClient
    {
        private readonly List<(bool configured, Action<IServiceProvider, HttpClient> options)> _configurationHttpClientCollection
            = new List<(bool configured, Action<IServiceProvider, HttpClient> options)>();

        private readonly List<(bool configured, Func<IServiceProvider, DelegatingHandler> handlers)> _delegatingHandlerCollection
            = new List<(bool configured, Func<IServiceProvider, DelegatingHandler> handlers)>();

        private readonly List<(bool configured, Func<IServiceProvider, HttpRequestMessage, IAsyncPolicy<HttpResponseMessage>> policy)> _policyCollection
            = new List<(bool configured, Func<IServiceProvider, HttpRequestMessage, IAsyncPolicy<HttpResponseMessage>>)>();

        private (bool configured, Func<IServiceProvider, HttpMessageHandler> handlerFactory) _configuredPrimaryHandler;

        /// <summary>
        /// Initializes a new instance of the <see cref="ResilienceHttpTypedClientBuilder{TClient, TImplementation}"/> class.
        /// </summary>
        /// <param name="services">The DI services.</param>
        /// <param name="name">The name of the builder.</param>
        /// <param name="optionsName">The name of the options to be used.</param>
        public ResilienceHttpTypedClientBuilder(IServiceCollection services, string? name = null, string? optionsName = null)
        {
            Services = services ?? throw new ArgumentNullException(nameof(services));

            var builderName = name ?? TypeNameHelper.GetTypeDisplayName(typeof(TClient), fullName: false);
            var options = optionsName ?? TypeNameHelper.GetTypeDisplayName(typeof(TImplementation), fullName: false);

            Name = builderName;
            OptionsName = options;

            HttpClientBuilder = Services.AddHttpClient<TClient, TImplementation>(Name);
        }

        /// <inheritdoc/>
        public string Name { get; }

        /// <inheritdoc/>
        public string OptionsName { get; private set; }

        /// <inheritdoc/>
        public IServiceCollection Services { get; }

        public IHttpClientBuilder HttpClientBuilder { get; }

        /// <inheritdoc/>
        public IResilienceHttpClientBuilder ConfigureHttpClientOptions(
            string? rootSectionName = null,
            Action<HttpClientOptions>? configureAction = null)
        {
            ConfigureHttpClientOptions(OptionsName, OptionsName, rootSectionName, configureAction);
            ConfigureHttpClient<HttpClientOptions>(OptionsName);
            return this;
        }

        /// <inheritdoc/>
        public IResilienceHttpClientBuilder ConfigureHttpClientOptions(
            string optionsSectionName,
            string? rootSectionName = null,
            Action<HttpClientOptions>? configureAction = null)
        {
            // override the option name.
            OptionsName = optionsSectionName;

            ConfigureHttpClientOptions(optionsSectionName, OptionsName, rootSectionName, configureAction);
            ConfigureHttpClient<HttpClientOptions>(OptionsName);
            return this;
        }

        /// <inheritdoc/>
        public IResilienceHttpClientBuilder ConfigureHttpClientOptions<TOptions>(
            string? rootSectionName = null,
            Action<TOptions>? configureAction = null) where TOptions : HttpClientOptions, new()
        {
            ConfigureHttpClientOptions(OptionsName, string.Empty, rootSectionName, configureAction);
            ConfigureHttpClient<TOptions>(string.Empty);
            return this;
        }

        /// <inheritdoc/>
        public IResilienceHttpClientBuilder ConfigureHttpClientOptions<TOptions>(
            string optionsSectionName,
            string? rootSectionName = null,
            Action<TOptions>? configureAction = null) where TOptions : HttpClientOptions, new()
        {
            OptionsName = optionsSectionName;

            ConfigureHttpClientOptions(OptionsName, string.Empty, rootSectionName, configureAction);
            ConfigureHttpClient<TOptions>(string.Empty);
            return this;
        }

        /// <inheritdoc/>
        public IResilienceHttpClientBuilder ConfigurePrimaryHandler(Func<IServiceProvider, HttpMessageHandler>? handlerFactory = null)
        {
            if (_configuredPrimaryHandler.configured
                && _configuredPrimaryHandler.handlerFactory != null)
            {
                throw new InvalidOperationException("Only one primary delegating hander can be registered");
            }

            if (handlerFactory == null
                && !_configuredPrimaryHandler.configured)
            {
                _configuredPrimaryHandler.configured = false;
                _configuredPrimaryHandler.handlerFactory = (sp) => new HttpClientHandler();
            }
            else if (handlerFactory != null
                && !_configuredPrimaryHandler.configured)
            {
                _configuredPrimaryHandler.configured = false;
                _configuredPrimaryHandler.handlerFactory = handlerFactory;
            }

            ConfigureAll();
            return this;
        }

        /// <inheritdoc/>
        public IResilienceHttpClientBuilder ConfigureHttpMessageHandler(Func<IServiceProvider, DelegatingHandler> configureHandler)
        {
            _delegatingHandlerCollection.Add((false, configureHandler));
            ConfigureAll();
            return this;
        }

        /// <inheritdoc/>
        public IResilienceHttpClientBuilder ConfigurePolicy(Func<IServiceProvider, HttpRequestMessage, IAsyncPolicy<HttpResponseMessage>> policySelector)
        {
            _policyCollection.Add((false, policySelector));
            ConfigureAll();
            return this;
        }

        /// <inheritdoc/>
        public (bool IsPrimaryHandlerSet, int OptionsCount, int DelegatingHandlerCount, int PolicyCount) Debug()
        {
            return (_configuredPrimaryHandler.configured, _configurationHttpClientCollection.Count, _delegatingHandlerCollection.Count, _policyCollection.Count);
        }

        /// <inheritdoc/>
        public IResilienceHttpClientBuilder ConfigureDefaultPolicies()
        {
            // register default policies with default options name.
            // TODO: rework the issue with registration of the default policies
            // Services.AddHttpDefaultResiliencePolicies();

            // TODO: rework with policies the issue of registration.
            IAsyncPolicy<HttpResponseMessage> TimeoutPolicy(IServiceProvider sp, HttpRequestMessage request)
            {
                var policy = sp.GetRequiredService<PolicyBucket<AsyncTimeoutPolicy<HttpResponseMessage>, TimeoutPolicyOptions>>()
                                .GetPolicy(HttpPolicyOptionsKeys.HttpTimeoutPolicy);
                return (IAsyncPolicy<HttpResponseMessage>)policy;
            }

            HttpClientBuilder.AddPolicyHandler(TimeoutPolicy);

            IAsyncPolicy<HttpResponseMessage> RetryPolicy(IServiceProvider sp, HttpRequestMessage request)
            {
                var policy = sp.GetRequiredService<PolicyBucket<AsyncRetryPolicy<HttpResponseMessage>, RetryPolicyOptions>>()
                                .GetPolicy(HttpPolicyOptionsKeys.HttpRetryPolicy);
                return (IAsyncPolicy<HttpResponseMessage>)policy;
            }

            HttpClientBuilder.AddPolicyHandler(RetryPolicy);

            IAsyncPolicy<HttpResponseMessage>? CircuitBreakerPolicy(IServiceProvider sp, HttpRequestMessage request)
            {
                var policy = sp.GetRequiredService<PolicyBucket<AsyncCircuitBreakerPolicy<HttpResponseMessage>, CircuitBreakerPolicyOptions>>()
                                    .GetPolicy(HttpPolicyOptionsKeys.HttpCircuitBreakerPolicy);
                return (IAsyncPolicy<HttpResponseMessage>)policy;
            }

            HttpClientBuilder.AddPolicyHandler(CircuitBreakerPolicy);

            return this;
        }

        private void ConfigureHttpClientOptions<TOptions>(
           string optionsSectionName,
           string optionName,
           string? rootSectionName = null,
           Action<TOptions>? configureAction = null) where TOptions : HttpClientOptions, new()
        {
            // configure changeable configurations
            Services.AddSingleton((Func<IServiceProvider, IOptionsChangeTokenSource<TOptions>>)((sp) =>
            {
                var configuration = sp.GetRequiredService<IConfiguration>();

                if (rootSectionName == null)
                {
                    return new ConfigurationChangeTokenSource<TOptions>(optionName, configuration);
                }
                else
                {
                    var section = configuration.GetSection(rootSectionName);
                    return new ConfigurationChangeTokenSource<TOptions>(optionName, section);
                }
            }));

            Services.AddSingleton<IConfigureOptions<TOptions>>(sp =>
            {
                // if optionName = string.Empty then default value exist
                return new ConfigureNamedOptions<TOptions>(optionName, (options) =>
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
            Services.AddSingleton(resolver => resolver.GetRequiredService<IOptions<TOptions>>().Value);

            // Registers an IConfigureOptions<TOptions> action configurator.
            // Being last it will bind from configuration source first
            // and run the customization afterwards
            Services.Configure<TOptions>(optionName, options => configureAction?.Invoke(options));
        }

        private void ConfigureHttpClient<TOptions>(string optionsName) where TOptions : HttpClientOptions, new()
        {
            _configurationHttpClientCollection.Add((
                 false,
                 (sp, client) =>
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
                 }
            ));

            ConfigureAll();
        }

        private void ConfigureAll()
        {
            // only configure once...
            if (!_configuredPrimaryHandler.configured
                && _configuredPrimaryHandler.handlerFactory != null)
            {
                _configuredPrimaryHandler.configured = true;
                HttpClientBuilder.ConfigurePrimaryHttpMessageHandler(_configuredPrimaryHandler.handlerFactory);
            }

            // http client options.
            foreach (var (configured, options) in _configurationHttpClientCollection)
            {
                if (!configured)
                {
                    HttpClientBuilder.ConfigureHttpClient(options);
                }
            }

            // additional handlers in order of registration.
            foreach (var (configured, handlers) in _delegatingHandlerCollection)
            {
                if (!configured)
                {
                    HttpClientBuilder.AddHttpMessageHandler(sp => handlers(sp));
                }
            }

            foreach (var (configured, policy) in _policyCollection)
            {
                if (!configured)
                {
                    HttpClientBuilder.AddPolicyHandler(policy);
                }
            }
        }
    }
}
