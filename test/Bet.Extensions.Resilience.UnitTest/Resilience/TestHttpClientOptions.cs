using Bet.Extensions.MessageHandlers;

namespace Bet.Extensions.Resilience.UnitTest
{
    public class TestHttpClientOptions : HttpClientOptions
    {
        public string Id { get; set; }
    }
}
