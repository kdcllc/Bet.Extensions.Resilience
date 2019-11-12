using Bet.Extensions.Http.MessageHandlers.Abstractions.Options;

namespace Bet.AspNetCore.Resilience.UnitTest.ResilienceTypedClient.Clients
{
    public class CustomHttpClientOptions : HttpClientOptions
    {
        public string Id { get; set; }
    }
}
