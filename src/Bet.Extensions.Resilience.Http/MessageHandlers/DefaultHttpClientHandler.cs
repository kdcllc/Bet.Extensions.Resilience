using System.Net;
using System.Net.Http;

namespace Bet.Extensions.Resilience.Http.MessageHandlers
{
    public class DefaultHttpClientHandler : HttpClientHandler
    {
        public DefaultHttpClientHandler()
        {
            AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
        }
    }
}
