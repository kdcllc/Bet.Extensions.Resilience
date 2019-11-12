using System.Net.Http;

namespace Bet.Extensions.Resilience.UnitTest.ClientResilience.Clients
{
    public interface ITestClient
    {
        HttpClient HttpClient { get; }
    }
}
