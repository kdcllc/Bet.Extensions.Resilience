using System.Net.Http;

namespace Bet.Extensions.Http.MessageHandlers.CookieAuthentication
{
    public sealed class CookieAuthenticationHandlerOptions
    {
        public CookieGeneratorOptions Options { get; set; } = new CookieGeneratorOptions(new HttpBasicAuthClientOptions());

        public HttpMessageHandler InnerHandler { get; set; }
    }
}
