using System.Net.Http;

namespace Bet.Extensions.Resilience.UnitTest
{
    public interface ITestTypedClientWithOptions
    {
        HttpClient HttpClient { get; }

        string Id { get;  }
    }
}
