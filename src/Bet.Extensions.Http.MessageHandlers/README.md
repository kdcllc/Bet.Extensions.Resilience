# Bet.Extensions.Http.MessageHandlers

[![Build status](https://ci.appveyor.com/api/projects/status/tmqs7xbq1aqee3md/branch/master?svg=true)](https://ci.appveyor.com/project/kdcllc/bet-extensions-resilience/branch/master)


[![NuGet](https://img.shields.io/nuget/v/Bet.Extensions.Http.MessageHandlers.svg)](https://www.nuget.org/packages?q=Bet.Extensions.Http.MessageHandlers)
![Nuget](https://img.shields.io/nuget/dt/Bet.Extensions.Http.MessageHandlers)
[![feedz.io](https://img.shields.io/badge/endpoint.svg?url=https://f.feedz.io/kdcllc/bet-extensions-resilience/shield/Bet.Extensions.Http.MessageHandlers/latest)](https://f.feedz.io/kdcllc/bet-extensions-resilience/packages/Bet.Extensions.Http.MessageHandlers/latest/download)


This library extends various `MessageHandlers` for `HttpClient`.

## Install

```bash
    dotnet add package Bet.Extensions.Resilience.MessageHandlers
```

## TimeoutHandler

This library extends the `HttpClient` timeout property by making a distinction between client canceling the request or the server.

## CookieAuthenticationHandler

This handler generates cookies per subsequently requests.

### Usage

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
