using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;

namespace Bet.Extensions.Http.MessageHandlers.CookieAuthentication
{
    public sealed class CookieGeneratorOptions
    {
        public CookieGeneratorOptions(HttpBasicAuthClientOptions options)
        {
            HttpOptions = options;
        }

        public HttpBasicAuthClientOptions HttpOptions { get; set; }

        public Action<HttpStatusCode, string> OnError { get; set; }

        public Func<HttpBasicAuthClientOptions, HttpRequestMessage> AuthenticationRequest { get; set; } = (options) =>
        {
            var request = new HttpRequestMessage(HttpMethod.Get, options.BaseAddress);
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", options.GetBasicAuthorizationHeaderValue());

            return request;
        };
    }
}
