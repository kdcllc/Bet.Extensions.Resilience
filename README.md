# Bet.Extensions.Resilience

[![Build status](https://ci.appveyor.com/api/projects/status/tmqs7xbq1aqee3md/branch/master?svg=true)](https://ci.appveyor.com/project/kdcllc/bet-extensions-resilience/branch/master)
[![NuGet](https://img.shields.io/nuget/v/Bet.Extensions.Resilience.Http.svg)](https://www.nuget.org/packages?q=Bet.Extensions.Resilience.Http)
[![MyGet](https://img.shields.io/myget/kdcllc/v/Bet.Extensions.Resilience.Http.svg?label=myget)](https://www.myget.org/F/kdcllc/api/v2)

This project contains a number of libraries to satisfy the needs of Microservices development in Kubernetes environment.

The bedrock for this project's Resilience is based on [`Polly`](https://github.com/App-vNext/Polly) policy libraries.

This library provides with a configurational Resilience framework for [HttpClientFactory](https://docs.microsoft.com/en-us/dotnet/architecture/microservices/implement-resilient-applications/use-httpclientfactory-to-implement-resilient-http-requests).

## Resilience Libraries

- [`Bet.Extensions.Resilience.Abstractions`](./src/Bet.Extensions.Resilience.Abstractions/) - the foundation library for Resilience policies.
- [`Bet.Extensions.Resilience.Http`](./src/Bet.Extensions.Resilience.Http/) - provides with base Policy Shapes for `HttpClient`.
- [`Bet.Extensions.Resilience.SqlClient`](./src/Bet.Extensions.Resilience.Http/) - provides with base SQL specific Policy Shapes.

## Resilience Hosting Libraries

- [`Bet.Extensions.Hosting.Resilience`](./src/Bet.Extensions.Hosting.Resilience/) - Registering for Generic Host Policies with DI and `IPolicyRegistry<string>`
- [`Bet.AspNetCore.Hosting.Resilience`](./src/Bet.AspNetCore.Hosting.Resilience/) - Registering for AspNetCore Host Policies with DI and `IPolicyRegistry<string>`

## Http Delegating Message Handlers

- [`Bet.Extensions.Http.MessageHandlers.Abstractions`](./src/Bet.Extensions.Http.MessageHandlers.Abstractions/)
- [`Bet.Extensions.Http.MessageHandlers`](./src/Bet.Extensions.Http.MessageHandlers/)

## Sample Applications

- [`Bet.Extensions.Resilience.WebApp.Sample`](./src/Bet.Extensions.Resilience.WebApp.Sample/)
- [`Bet.Extensions.Resilience.Worker.Sample`](./src/Bet.Extensions.Resilience.Worker.Sample/)

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

- [Docker files Github repo](https://github.com/kdcllc/docker/blob/master/dotnet/dotnet-docker.md)

## Reference

### HttpClient Diagnostics events order

System.Net.Http.HttpRequestOut.Start
System.Net.Http.Request
System.Net.Http.HttpRequestOut.Stop
System.Net.Http.Response
