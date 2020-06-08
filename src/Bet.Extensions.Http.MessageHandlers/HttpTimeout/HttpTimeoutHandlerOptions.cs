using System;
using System.Net.Http;

namespace Bet.Extensions.Http.MessageHandlers.HttpTimeout
{
    /// <summary>
    /// The options to configure for <see cref="HttpTimeoutHandler"/>.
    /// </summary>
    public sealed class HttpTimeoutHandlerOptions
    {
        /// <summary>
        /// The default time out for <see cref="HttpClient"/>.
        /// The default value is 100 seconds which matches the default value of <see cref="HttpClient"/> timeout property.
        /// </summary>
        public TimeSpan DefaultTimeout { get; set; } = TimeSpan.FromSeconds(100);

        /// <summary>
        /// The innerHanlder to be used for the Handler.
        /// </summary>
        public HttpMessageHandler? InnerHandler { get; set; }
    }
}
