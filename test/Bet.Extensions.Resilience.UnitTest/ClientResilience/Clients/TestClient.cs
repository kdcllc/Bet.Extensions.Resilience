namespace Bet.Extensions.Resilience.UnitTest.ClientResilience.Clients;

public class TestClient : ITestClient
{
    public TestClient(HttpClient client)
    {
        HttpClient = client;
    }

    public HttpClient HttpClient { get; }
}
