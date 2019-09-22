using System;
using System.Collections.Generic;
using System.Net.Http;

using Bet.Extensions.MessageHandlers;

using Polly;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// The configuration settings for <see cref="HttpClient"/>.
    /// </summary>
    public class HttpClientOptionsBuilder
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HttpClientOptionsBuilder"/> class.
        /// </summary>
        /// <param name="httpClientName"></param>
        /// <param name="httpClientBuilder"></param>
        public HttpClientOptionsBuilder(string httpClientName, IHttpClientBuilder httpClientBuilder)
        {
            Name = httpClientName;
            HttpClientBuilder = httpClientBuilder ?? throw new ArgumentNullException(nameof(httpClientBuilder));
        }

        /// <summary>
        /// The flag to enable or disable custom logging of Http requests.
        /// </summary>
        public bool EnableLogging { get; set; } = false;

        /// <summary>
        /// The primary default <see cref="HttpMessageHandler"/>.
        /// </summary>
        public Func<IServiceProvider, HttpMessageHandler> PrimaryHandler { get; set; } = (sp) => new HttpClientHandler();

        /// <summary>
        /// The delegates that will be used to configure a named <see cref="HttpClient"/>.
        /// </summary>
        public IList<Action<IServiceProvider, HttpClient>> ConfigureHttpClient { get; } = new List<Action<IServiceProvider, HttpClient>>();

        /// <summary>
        /// An additional message handlers.
        /// </summary>
        public IList<Func<IServiceProvider, DelegatingHandler>> AdditionalHandlers { get; } = new List<Func<IServiceProvider, DelegatingHandler>>();

        /// <summary>
        /// Polly Policies selectors.
        /// </summary>
        public IList<Func<IServiceProvider, HttpRequestMessage, IAsyncPolicy<HttpResponseMessage>>> Policies { get; } = new List<Func<IServiceProvider, HttpRequestMessage, IAsyncPolicy<HttpResponseMessage>>>();

        /// <summary>
        /// <see cref="HttpClient"/> options for this client.
        /// </summary>
        public HttpClientOptions Options { get; set; } = new HttpClientOptions();

        /// <summary>
        /// The name of the registered <see cref="HttpClient"/>.
        /// </summary>
        internal string Name { get; set; }

        internal IHttpClientBuilder HttpClientBuilder { get; set; }

        internal bool IsPrimaryHanlderAdded { get; set; } = false;
    }
}
