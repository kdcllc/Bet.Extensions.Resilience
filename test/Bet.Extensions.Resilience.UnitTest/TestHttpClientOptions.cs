using Bet.Extensions.Resilience.Http.Options;

namespace Bet.Extensions.Resilience.UnitTest
{
    public class TestHttpClientOptions : HttpClientOptions
    {
        public string Id { get; set; }
    }
}
