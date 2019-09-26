using System.Net.Http;
using System.Threading.Tasks;

namespace Bet.Extensions.Resilience.UnitTest.ResilienceTypedClient.Clients
{
    public interface ICustomTypedClientWithOptions
    {
        HttpClient HttpClient { get; }

        string Id { get; }

        Task<HttpResponseMessage> SendRequestAsync();
    }
}
