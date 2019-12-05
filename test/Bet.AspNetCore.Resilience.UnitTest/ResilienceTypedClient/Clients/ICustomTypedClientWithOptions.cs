using System.Net.Http;
using System.Threading.Tasks;

namespace Bet.AspNetCore.Resilience.UnitTest.ResilienceTypedClient.Clients
{
    public interface ICustomTypedClientWithOptions
    {
        HttpClient HttpClient { get; }

        string Id { get; }

        Task<HttpResponseMessage> SendRequestAsync();
    }
}
