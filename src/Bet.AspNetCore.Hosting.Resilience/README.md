# Bet.AspNetCore.Hosting.Resilience

[![GitHub license](https://img.shields.io/badge/license-MIT-blue.svg?style=flat-square)](https://raw.githubusercontent.com/kdcllc/Bet.Extensions.Resilience/master/LICENSE)

[![Build status](https://ci.appveyor.com/api/projects/status/tmqs7xbq1aqee3md/branch/master?svg=true)](https://ci.appveyor.com/project/kdcllc/bet-extensions-resilience/branch/master)

[![NuGet](https://img.shields.io/nuget/v/Bet.AspNetCore.Hosting.Resilience.svg)](https://www.nuget.org/packages?q=Bet.AspNetCore.Hosting.Resilience)
![Nuget](https://img.shields.io/nuget/dt/Bet.AspNetCore.Hosting.Resilience)

[![feedz.io](https://img.shields.io/badge/endpoint.svg?url=https://f.feedz.io/kdcllc/bet-extensions-resilience/shield/Bet.AspNetCore.Hosting.Resilience/latest)](https://f.feedz.io/kdcllc/bet-extensions-resilience/packages/Bet.AspNetCore.Hosting.Resilience/latest/download)

> The second letter in the Hebrew alphabet is the ב bet/beit. Its meaning is "house". In the ancient pictographic Hebrew it was a symbol resembling a tent on a landscape.

_Note: Pre-release packages are distributed via [feedz.io](https://f.feedz.io/kdcllc/bet-extensions-resilience/nuget/index.json)._

## Summary

This Library provides registration for hosting container of `IHost` interface for `HttpClient` message handlers.

[![buymeacoffee](https://www.buymeacoffee.com/assets/img/custom_images/orange_img.png)](https://www.buymeacoffee.com/vyve0og)

## Give a Star! :star:

If you like or are using this project to learn or start your solution, please give it a star. Thanks!

## Install

```bash
    dotnet add package Bet.AspNetCore.Hosting.Resilience
```

## Usage

Add the following in `Program.cs`

```csharp
 public static IHostBuilder CreateHostBuilder(string[] args)
{
    return Host
        .CreateDefaultBuilder(args)
        .ConfigureWebHostDefaults(webBuilder =>
        {
            webBuilder.UseResilienceOnStartup();

            webBuilder.UseStartup<Startup>();
        });
}
```

`appsettings.json`

```json
  "DefaultHttpPolicies": {

    "HttpTimeoutPolicy": {
      "Timeout": "00:01:40" // Timeout for an individual try
    },

    "HttpCircuitBreakerPolicy": {
      "DurationOfBreak": "00:00:10",
      "ExceptionsAllowedBeforeBreaking": 2
    },

    "HttpRetryPolicy": {
      "BackoffPower": 2,
      "Count": 3
    }

  }
```
