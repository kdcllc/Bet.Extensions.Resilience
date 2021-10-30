using System.Net;

namespace Bet.Extensions.Http.MessageHandlers
{
    public class DefaultHttpClientHandler : HttpClientHandler
    {
        public DefaultHttpClientHandler()
        {
            if (SupportsAutomaticDecompression)
            {
                AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
            }

            UseCookies = false;
        }
    }
}
