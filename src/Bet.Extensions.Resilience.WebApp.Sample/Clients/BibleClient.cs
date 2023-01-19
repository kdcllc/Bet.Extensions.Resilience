using System.Text.Json;

using Microsoft.Extensions.Options;

namespace Bet.Extensions.Resilience.WebApp.Sample.Clients;

public class BibleClient : IBibleClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<BibleClient> _logger;
    private BibleClientOptions _options;

    public BibleClient(
        HttpClient httpClient,
        IOptionsMonitor<BibleClientOptions> optionsMonitor,
        ILogger<BibleClient> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _logger = logger;
        _options = optionsMonitor.Get(nameof(BibleClient));

        optionsMonitor.OnChange((newValues) =>
        {
            _options = newValues;
        });
    }

    public async Task<VerseReference?> GetQuoteAsync(string search, CancellationToken cancellationToken)
    {
        try
        {
            var response = await _httpClient.GetAsync(search, cancellationToken);

            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync(cancellationToken);

            return JsonSerializer.Deserialize<VerseReference>(json);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve passage for {search}", search);
        }

        return null;
    }
}
