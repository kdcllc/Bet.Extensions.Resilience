using System.Net.Http;

using Microsoft.Extensions.Options;

namespace Bet.Extensions.Resilience.UnitTest
{
    public class TestTypedClientWithOptions : ITestTypedClientWithOptions
    {
        public TestTypedClientWithOptions(HttpClient client, IOptions<TestHttpClientOptions> options)
        {
            HttpClient = client;

            Id = options.Value.Id;
        }

        public HttpClient HttpClient { get; }

        public string Id { get; }
    }
}
