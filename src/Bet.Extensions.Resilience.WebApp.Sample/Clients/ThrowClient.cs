namespace Bet.Extensions.Resilience.WebApp.Sample.Clients;

public class ThrowClient : IThrowClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ThrowClient> _logger;

    public ThrowClient(
        HttpClient httpClient,
        ILogger<ThrowClient> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<string> GetStatusAsync(CancellationToken cancellationToken)
    {
        try
        {
            var result = await _httpClient.GetAsync("/api/throw", cancellationToken);
            if (result?.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return "Successful";
            }

            return $"Failed with {result?.StatusCode}";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed");
        }

        return "Completely failed";
    }
}
