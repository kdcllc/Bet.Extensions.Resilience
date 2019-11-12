using Bet.Extensions.Http.MessageHandlers.Abstractions.Options;

namespace Bet.Extensions.Resilience.UnitTest.ClientResilience.Clients
{
    public class TestClientOptions : HttpClientOptions
    {
        public string ExtraValue { get; set; }
    }
}
