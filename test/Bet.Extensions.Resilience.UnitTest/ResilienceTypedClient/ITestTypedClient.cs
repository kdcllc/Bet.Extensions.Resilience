using System.Net.Http;

namespace Bet.Extensions.Resilience.UnitTest.ResilienceTypedClient.Clients
{
    // Simple typed client for use in tests
    public interface ITestTypedClient
    {
        HttpClient HttpClient { get; }
    }
}
