# Bet.Extensions.Resilience
This library allows for Resilience configuration and logging for HttpClients.

- `PolicyWithLoggingHttpMessageHandler` allows to add logger to Polly's context.


## Usage

1. Add Typed Client `ChavahClient`

```csharp
public class ChavahClient : IChavahClient
{
    private readonly HttpClient _httpClient;

    public ChavahClient(HttpClient httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    public async Task<IEnumerable<Song>> GetPopular(int count)
    {
        var response = await _httpClient.GetAsync($"api/songs/getpopular?count={count}");

        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();

        return JsonConvert.DeserializeObject<List<Song>>(json);
    }
}
```


2. Add Typed Client Configuration in `appsetting.json`

```json

"ChavahClient": {
    "BaseAddress": "https://messianicradio.com",
    "Timeout": "00:05:00",
    "ContentType": "application/json"
  }

```

3. Add Typed Client Registration in `Startup.cs`

```csharp

  services.AddResilienceTypedClient<IChavahClient, ChavahClient>()
          .AddDefaultPolicies();

```
