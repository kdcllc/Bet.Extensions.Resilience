using System.Net;
using System.Net.Http.Headers;

using Bet.Extensions.Resilience.Http.Abstractions.Options;

namespace Bet.Extensions.Http.MessageHandlers.CookieAuthentication;

public sealed class CookieGeneratorOptions
{
    public CookieGeneratorOptions(HttpClientBasicAuthOptions options)
    {
        HttpOptions = options;
    }

    public HttpClientBasicAuthOptions HttpOptions { get; set; }

    public Action<HttpStatusCode, string>? OnError { get; set; }

    public Func<HttpClientBasicAuthOptions, HttpRequestMessage> AuthenticationRequest { get; set; } = (options) =>
    {
        var request = new HttpRequestMessage(HttpMethod.Get, options.BaseAddress);
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", options.GetBasicAuthorizationHeaderValue());

        return request;
    };
}
