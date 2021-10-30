using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Bet.AspNetCore.Resilience.UnitTest.ResilienceTypedClient.Clients
{
    public class CustomTypedClientWithOptions : ICustomTypedClientWithOptions
    {
        private readonly ILogger<CustomTypedClientWithOptions> _logger;

        public CustomTypedClientWithOptions(
            HttpClient client,
            IOptions<CustomHttpClientOptions> options,
            ILogger<CustomTypedClientWithOptions> logger)
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
