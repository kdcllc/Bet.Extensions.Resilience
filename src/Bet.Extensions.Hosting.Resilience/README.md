# Bet.Extensions.Hosting.Resilience

[![GitHub license](https://img.shields.io/badge/license-MIT-blue.svg?style=flat-square)](https://raw.githubusercontent.com/kdcllc/Bet.Extensions.Resilience/master/LICENSE)


[![Build status](https://ci.appveyor.com/api/projects/status/tmqs7xbq1aqee3md/branch/master?svg=true)](https://ci.appveyor.com/project/kdcllc/bet-extensions-resilience/branch/master)

[![NuGet](https://img.shields.io/nuget/v/Bet.Extensions.Hosting.Resilience.svg)](https://www.nuget.org/packages?q=Bet.Extensions.Hosting.Resilience)
![Nuget](https://img.shields.io/nuget/dt/Bet.Extensions.Hosting.Resilience)

[![feedz.io](https://img.shields.io/badge/endpoint.svg?url=https://f.feedz.io/kdcllc/bet-extensions-resilience/shield/Bet.Extensions.Hosting.Resilience/latest)](https://f.feedz.io/kdcllc/bet-extensions-resilience/packages/Bet.Extensions.Hosting.Resilience/latest/download)


This Library provides registration for hosting container of `IHost` interface for `HttpClient` message handlers.

## Nuget

```cmd
    dotnet add package Bet.Extensions.Hosting.Resilience
```

## Usage

```csharp

    public static IHostBuilder CreateHostBuilder(string[] args)
    {
        return Host.CreateDefaultBuilder(args)

                // adds resilience
                .UseResilienceOnStartup()

                // use correlationid for http context
                .UseCorrelationId(options => options.UseGuidForCorrelationId = true);
    }

    private async Task<string> GetWeatherForecast()
    {
        // inject IOptions<CorrelationIdOptions> correlationOptions
        // creates a parent activity context...
        var activity = new Activity("CallToBackend")
                            .AddBaggage(_correlationOptions.Header, Guid.NewGuid().ToString())
                            .Start();

        try
        {
            return await _httpClient.GetStringAsync(
                                   "http://localhost:5000/weatherforecastproxy");
        }
        finally
        {
            activity.Stop();
        }
    }
```
