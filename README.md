# Bet.Extensions.Resilience

[![Build status](https://ci.appveyor.com/api/projects/status/tmqs7xbq1aqee3md/branch/master?svg=true)](https://ci.appveyor.com/project/kdcllc/bet-extensions-resilience/branch/master)
[![NuGet](https://img.shields.io/nuget/v/Bet.Extensions.Resilience.Http.svg)](https://www.nuget.org/packages?q=Bet.Extensions.Resilience.Http)
[![MyGet](https://img.shields.io/myget/kdcllc/v/Bet.Extensions.Resilience.Http.svg?label=myget)](https://www.myget.org/F/kdcllc/api/v2)

This library allows for Resilience configuration and logging for HttpClients and SQL in the future.

The Resilience is based on `Polly` standard library.

- [`Bet.Extensions.Resilience.Abstractions`](./src/Bet.Extensions.Resilience.Abstractions/README.md)
- [`Bet.Extensions.Http.MessageHandlers`](./src/Bet.Extensions.Http.MessageHandlers/README.md)
- [`Bet.Extensions.Resilience.Http`](./src/Bet.Extensions.Resilience.Http/README.md)
     `PolicyWithLoggingHttpMessageHandler` allows to add logger to Polly's context.

## Sampe Application

- [`Bet.Extensions.Resilience.SampleWebApp`](./src/Bet.Extensions.Resilience.SampleWebApp/README.md)

## Development Environment

This project supports:

- VSCode Remote Development in Dev Docker Container (Make sure that debugging of the app is used to run the application.)

- Visual Studio.NET Docker

## Reference

Get ip address of the docker container
```bash
 hostname -I
```

### HttpClient Diagnostics events order

System.Net.Http.HttpRequestOut.Start
System.Net.Http.Request
System.Net.Http.HttpRequestOut.Stop
System.Net.Http.Response
