# Bet.Extensions.Hosting.Resilience

[![Build status](https://ci.appveyor.com/api/projects/status/tmqs7xbq1aqee3md/branch/master?svg=true)](https://ci.appveyor.com/project/kdcllc/bet-extensions-resilience/branch/master)
[![NuGet](https://img.shields.io/nuget/v/Bet.Extensions.Hosting.Resilience.svg)](https://www.nuget.org/packages?q=Bet.Extensions.Hosting.Resilience)
[![MyGet](https://img.shields.io/myget/kdcllc/v/Bet.Extensions.Hosting.Resilience.svg?label=myget)](https://www.myget.org/F/kdcllc/api/v2)

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