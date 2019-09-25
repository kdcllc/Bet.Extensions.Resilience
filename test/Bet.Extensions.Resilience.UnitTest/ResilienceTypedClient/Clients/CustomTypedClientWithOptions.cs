using System.Net.Http;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Bet.Extensions.Resilience.UnitTest.ResilienceTypedClient.Clients
{
    public class TestTypedClientWithOptions : ITestTypedClientWithOptions
    {
        private readonly ILogger<TestTypedClientWithOptions> _logger;

        public TestTypedClientWithOptions(
            HttpClient client,
            IOptions<TestHttpClientOptions> options,
            ILogger<TestTypedClientWithOptions> logger)
        {
            HttpClient = client;

            Id = options.Value.Id;
            _logger = logger ?? throw new System.ArgumentNullException(nameof(logger));
        }

        public HttpClient HttpClient { get; }

        public string Id { get; }

        public async Task<HttpResponseMessage> SendRequestAsync()
        {
            _logger.LogInformation("Sending Test Request");

            return await HttpClient.GetAsync(string.Empty);
        }
    }
}
