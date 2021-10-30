using Bet.Extensions.Resilience.Http.Abstractions.Options;

namespace Bet.Extensions.Http.MessageHandlers.CookieAuthentication
{
    public sealed class CookieAuthenticationHandlerOptions
    {
        public CookieGeneratorOptions Options { get; set; } = new CookieGeneratorOptions(new HttpClientBasicAuthOptions());

        public HttpMessageHandler? InnerHandler { get; set; }
    }
}
