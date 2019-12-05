using Bet.Extensions.Resilience.Http.Abstractions.Options;

namespace Bet.Extensions.Resilience.UnitTest.ClientResilience.Clients
{
    public class TestClientOptions : HttpClientOptions
    {
        public string ExtraValue { get; set; }
    }
}
