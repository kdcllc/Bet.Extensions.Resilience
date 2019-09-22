using Bet.Extensions.Http.MessageHandlers;

namespace Bet.Extensions.Resilience.UnitTest.Resilience
{
    public class TestHttpClientOptions : HttpClientOptions
    {
        public string Id { get; set; }
    }
}
