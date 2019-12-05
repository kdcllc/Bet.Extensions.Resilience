using System.Net.Http;

namespace Bet.AspNetCore.Resilience.UnitTest.ResilienceTypedClient.Clients
{
    // Simple typed client for use in tests
    public interface ICustomTypedClient
    {
        HttpClient HttpClient { get; }
    }
}
