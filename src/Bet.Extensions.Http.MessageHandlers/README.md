# Bet.Extensions.Http.MessageHandlers

[![GitHub license](https://img.shields.io/badge/license-MIT-blue.svg?style=flat-square)](https://raw.githubusercontent.com/kdcllc/Bet.Extensions.Resilience/master/LICENSE)
[![Build status](https://ci.appveyor.com/api/projects/status/tmqs7xbq1aqee3md/branch/master?svg=true)](https://ci.appveyor.com/project/kdcllc/bet-extensions-resilience/branch/master)
[![NuGet](https://img.shields.io/nuget/v/Bet.Extensions.Http.MessageHandlers.svg)](https://www.nuget.org/packages?q=Bet.Extensions.Http.MessageHandlers)
![Nuget](https://img.shields.io/nuget/dt/Bet.Extensions.Http.MessageHandlers)
[![feedz.io](https://img.shields.io/badge/endpoint.svg?url=https://f.feedz.io/kdcllc/bet-extensions-resilience/shield/Bet.Extensions.Http.MessageHandlers/latest)](https://f.feedz.io/kdcllc/bet-extensions-resilience/packages/Bet.Extensions.Http.MessageHandlers/latest/download)

> The second letter in the Hebrew alphabet is the ב bet/beit. Its meaning is "house". In the ancient pictographic Hebrew it was a symbol resembling a tent on a landscape.

_Note: Pre-release packages are distributed via [feedz.io](https://f.feedz.io/kdcllc/bet-extensions-resilience/nuget/index.json)._

## Summary

This library extends various `MessageHandlers` for `HttpClient`.

[![buymeacoffee](https://www.buymeacoffee.com/assets/img/custom_images/orange_img.png)](https://www.buymeacoffee.com/vyve0og)

## Give a Star! :star:

If you like or are using this project to learn or start your solution, please give it a star. Thanks!

## Install

```bash
    dotnet add package Bet.Extensions.Resilience.MessageHandlers
```

## TimeoutHandler

This library extends the `HttpClient` timeout property by making a distinction between client canceling the request or the server.

## CookieAuthenticationHandler

This handler generates cookies per subsequently requests.

## AuthorizeHandler

Authorization Handler allows to create an `HttpClient` that automatically retrieve access token.

1. Registration of the client for TrustPilot Api.

```csharp

using System.Net.Http.Headers;
using System.Text;

using Bet.Extensions.Http.MessageHandlers.Authorize;

using Marketing.TrustPilot;
using Marketing.TrustPilot.Models;
using Marketing.TrustPilot.Options;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.DependencyInjection;

public static class TrustPilotServiceExtensions
{
    public static IServiceCollection AddTrustPilot(this IServiceCollection services)
    {
        services.AddChangeTokenOptions<TrustPilotOptions>(nameof(TrustPilotOptions), null, configureAction: c => { });

        services.AddHttpClient<TrustPilotClient>((sp, client) =>
        {
            var options = sp.GetRequiredService<IOptionsMonitor<TrustPilotOptions>>();
            client.BaseAddress = options.CurrentValue.BaseAddress;
        })
            .AddHttpMessageHandler(sp =>
            {
                // get logger
                var logger = sp.GetRequiredService<ILogger<AuthorizeHandler<TrustPilotOptions, AuthToken>>>();

                var options = sp.GetRequiredService<IOptionsMonitor<TrustPilotOptions>>();

                var handlerConfiguration = new AuthorizeHandlerConfiguration<TrustPilotOptions, AuthToken>(
                    (authClientOptions) =>
                    {
                        var body =
                            $"grant_type=password&username={authClientOptions.Username}&password={authClientOptions.Password}";

                        var request = new HttpRequestMessage(
                                HttpMethod.Post,
                                new Uri(authClientOptions.BaseAddress, "v1/oauth/oauth-business-users-for-applications/accesstoken"))
                                {
                                    Content = new StringContent(body, Encoding.UTF8, "application/x-www-form-urlencoded")
                                };

                        var base64Encoded = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{authClientOptions.ApiKey}:{authClientOptions.ApiSecret}"));

                        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", base64Encoded);

                        return request;
                    },
                    response =>
                    {
                        var accessToken = response.AccessToken;

                        // https://documentation-apidocumentation.trustpilot.com/faq
                        // "issued_at" is measured in milliseconds. "expires_in" is measured in seconds. Both are have data type - string.
                        var createdAt = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(response.IssuedAt.GetValueOrDefault());

                        var expiresIn = createdAt + TimeSpan.FromSeconds(response.ExpiresIn.GetValueOrDefault());

                        return (accessToken, expiresIn);
                    });

                return new AuthorizeHandler<TrustPilotOptions, AuthToken>(
                                        options.CurrentValue,
                                        handlerConfiguration,
                                        AuthType.Bearer,
                                        logger);
            });

        return services;
    }
}

```

2. Options for this client can be stored inside of Azure Vault.

```json
  "TrustPilotOptions": {
    "BaseAddress": "https://api.trustpilot.com/",
    "ContentType": "application/json",
    "ApiKey" : null,
    "ApiSecret" : null,
    "Username" : null,
    "Password" : null
  }
```

```csharp
using Bet.Extensions.Http.MessageHandlers.Authorize;

namespace Marketing.TrustPilot.Options;

public class TrustPilotOptions : AuthHttpClientOptions
{
    public string? ApiKey { get; set; }

    public string? ApiSecret { get; set; }

    public string? BusinessUnitId { get; set; }
}
```

3. Sample `HttpClient for TrustPilot Api

```csharp
using System.Net.Http.Json;

using Marketing.TrustPilot.Options;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using i = Marketing.TrustPilot.Models.Invitations;
using r = Marketing.TrustPilot.Models.Reviews;

namespace Marketing.TrustPilot;

public class TrustPilotClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<TrustPilotClient> _logger;
    private TrustPilotOptions _options;

    public TrustPilotClient(
        IOptionsMonitor<TrustPilotOptions> optionsMonitor,
        HttpClient httpClient,
        ILogger<TrustPilotClient> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _options = optionsMonitor.CurrentValue;
        optionsMonitor.OnChange(o => _options = o);
    }

    public async Task CreateInvitationAsync(i.Invitation invitation, CancellationToken cancellationToken)
    {
        _httpClient.BaseAddress = new Uri("https://invitations-api.trustpilot.com/v1/private/business-units/");

        var response = await _httpClient.PostAsJsonAsync($"{_options.BusinessUnitId}/email-invitations", invitation, SystemTextJson.Options, cancellationToken);
        var content = await response.Content.ReadAsStringAsync(cancellationToken);

        response.EnsureSuccessStatusCode();
    }

    public Task<r.Review?> GetReviewAsync(string id, CancellationToken cancellationToken)
    {
        return _httpClient.GetFromJsonAsync<r.Review>($"v1/private/reviews/{id}", SystemTextJson.Options, cancellationToken);
    }
}

```
## Other Example

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
