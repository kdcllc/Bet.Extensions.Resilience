using System.Net.Http;
using System.Threading.Tasks;

namespace Bet.Extensions.Resilience.UnitTest.Resilience
{
    public interface ITestTypedClientWithOptions
    {
        HttpClient HttpClient { get; }

        string Id { get;  }

        Task<HttpResponseMessage> SendRequestAsync();
    }
}
