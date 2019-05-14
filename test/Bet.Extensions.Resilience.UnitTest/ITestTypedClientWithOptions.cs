using System.Net.Http;
using System.Threading.Tasks;

namespace Bet.Extensions.Resilience.UnitTest
{
    public interface ITestTypedClientWithOptions
    {
        HttpClient HttpClient { get; }

        string Id { get;  }

        Task<HttpResponseMessage> SendRequestAsync();
    }
}
