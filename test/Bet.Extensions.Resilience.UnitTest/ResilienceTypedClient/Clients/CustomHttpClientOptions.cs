using Bet.Extensions.Http.MessageHandlers.Abstractions.Options;

namespace Bet.Extensions.Resilience.UnitTest.ResilienceTypedClient.Clients
{
    public class CustomHttpClientOptions : HttpClientOptions
    {
        public string Id { get; set; }
    }
}
