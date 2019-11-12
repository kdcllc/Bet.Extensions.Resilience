# Bet.Extensions.Resilience

[![Build status](https://ci.appveyor.com/api/projects/status/tmqs7xbq1aqee3md/branch/master?svg=true)](https://ci.appveyor.com/project/kdcllc/bet-extensions-resilience/branch/master)
[![NuGet](https://img.shields.io/nuget/v/Bet.Extensions.Resilience.Http.svg)](https://www.nuget.org/packages?q=Bet.Extensions.Resilience.Http)
[![MyGet](https://img.shields.io/myget/kdcllc/v/Bet.Extensions.Resilience.Http.svg?label=myget)](https://www.myget.org/F/kdcllc/api/v2)

This library provides with a configurational Resilience framework for [HttpClientFactory](https://docs.microsoft.com/en-us/dotnet/architecture/microservices/implement-resilient-applications/use-httpclientfactory-to-implement-resilient-http-requests).

In addition later work will also include the Resilience providers for Azure SQL.

The Resilience of these packages is based on `Polly` standard library.

Message Handlers

- [`Bet.Extensions.Http.MessageHandlers`](./src/Bet.Extensions.Http.MessageHandlers/README.md)
- [`Bet.Extensions.Http.MessageHandlers.Abstractions`](./src/Bet.Extensions.Http.MessageHandlers.Abstractions/README.md)

Resilience

- [`Bet.Extensions.Resilience.Http`](./src/Bet.Extensions.Resilience.Http/README.md)
- [`Bet.Extensions.Resilience.Abstractions`](./src/Bet.Extensions.Resilience.Abstractions/README.md)
- [`Bet.Extensions.Hosting.Resilience`](./src/Bet.Extensions.Hosting.Resilience/README.md)


## Sample Application

- [`Bet.Extensions.Resilience.WebApp.Sample`](./src/Bet.Extensions.Resilience.WebApp.Sample/README.md)

## Development Environment

This project supports:

- VSCode Remote Development in Dev Docker Container (Make sure that debugging of the app is used to run the application.)

- Visual Studio.NET Docker

To get an ip address of the running docker container:

```bash
     hostname -I
```

## Docker images

This repo is utilizing KDCLLC Docker images:

- [kdcllc/dotnet:3.0-sdk-vscode-bionic](https://hub.docker.com/r/kdcllc/dotnet/tags) - for the VS Code In container development.

- [kdcllc/dotnet:3.0-sdk-buster](https://hub.docker.com/r/kdcllc/dotnet/tags) - for running the sample web application.

[Docker files Github repo](https://github.com/kdcllc/docker/blob/master/dotnet/dotnet-docker.md)

## Reference



### HttpClient Diagnostics events order

System.Net.Http.HttpRequestOut.Start
System.Net.Http.Request
System.Net.Http.HttpRequestOut.Stop
System.Net.Http.Response
