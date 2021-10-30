# Bet.Extensions.Resilience

[![GitHub license](https://img.shields.io/badge/license-MIT-blue.svg?style=flat-square)](https://raw.githubusercontent.com/kdcllc/Bet.Extensions.Resilience/master/LICENSE)
[![Build status](https://ci.appveyor.com/api/projects/status/tmqs7xbq1aqee3md/branch/master?svg=true)](https://ci.appveyor.com/project/kdcllc/bet-extensions-resilience/branch/master)
[![NuGet](https://img.shields.io/nuget/v/Bet.Extensions.Resilience.Abstractions.svg)](https://www.nuget.org/packages?q=Bet.Extensions.Resilience.Abstractions)
![Nuget](https://img.shields.io/nuget/dt/Bet.Extensions.Resilience.Abstractions)
[![feedz.io](https://img.shields.io/badge/endpoint.svg?url=https://f.feedz.io/kdcllc/bet-extensions-resilience/shield/Bet.Extensions.Resilience.Abstractions/latest)](https://f.feedz.io/kdcllc/bet-extensions-resilience/packages/Bet.Extensions.Resilience.Abstractions/latest/download)

> The second letter in the Hebrew alphabet is the ב bet/beit. Its meaning is "house". In the ancient pictographic Hebrew it was a symbol resembling a tent on a landscape.

*Note: Pre-release packages are distributed via [feedz.io](https://f.feedz.io/kdcllc/bet-extensions-resilience/nuget/index.json).*

## Summary

This project contains a number of libraries to satisfy the needs of Microservices development in Kubernetes environment.

The bedrock for this project's Resilience is based on [`Polly`](https://github.com/App-vNext/Polly) policy libraries.

This library provides with a configurational Resilience framework for [HttpClientFactory](https://docs.microsoft.com/en-us/dotnet/architecture/microservices/implement-resilient-applications/use-httpclientfactory-to-implement-resilient-http-requests).

[![buymeacoffee](https://www.buymeacoffee.com/assets/img/custom_images/orange_img.png)](https://www.buymeacoffee.com/vyve0og)

## Give a Star! :star:

If you like or are using this project to learn or start your solution, please give it a star. Thanks!

## Resilience Libraries

- [`Bet.Extensions.Resilience.Abstractions`](./src/Bet.Extensions.Resilience.Abstractions/) - the foundation library for Resilience policies.
- [`Bet.Extensions.Resilience.Http`](./src/Bet.Extensions.Resilience.Http/) - provides with base Policy Shapes for `HttpClient`.
- [`Bet.Extensions.Resilience.Data.SqlClient`](./src/Bet.Extensions.Resilience.Data.SqlClient/) - provides with base SQL specific Policy Shapes.

## Resilience Hosting Libraries

- [`Bet.Extensions.Hosting.Resilience`](./src/Bet.Extensions.Hosting.Resilience/) - Registering for Generic Host Policies with DI and `IPolicyRegistry<string>`
- [`Bet.AspNetCore.Hosting.Resilience`](./src/Bet.AspNetCore.Hosting.Resilience/) - Registering for AspNetCore Host Policies with DI and `IPolicyRegistry<string>`

## Http Delegating Message Handlers

- [`Bet.Extensions.Http.MessageHandlers.Abstractions`](./src/Bet.Extensions.Http.MessageHandlers.Abstractions/)
- [`Bet.Extensions.Http.MessageHandlers`](./src/Bet.Extensions.Http.MessageHandlers/) - Timeout, Authorization

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

## Docker Images

This repo is utilizing [King David Consulting LLC Docker Images](https://hub.docker.com/u/kdcllc):

- [kdcllc/dotnet-sdk:3.1](https://hub.docker.com/r/kdcllc/dotnet-sdk-vscode):  - the docker image for templated `DotNetCore` build of the sample web application.

- [kdcllc/dotnet-sdk-vscode:3.1](https://hub.docker.com/r/kdcllc/dotnet-sdk/tags): the docker image for the Visual Studio Code In container development.

- [Docker Github repo](https://github.com/kdcllc/docker/blob/master/dotnet/dotnet-docker.md)

## Reference

### HttpClient Diagnostics events order

System.Net.Http.HttpRequestOut.Start
System.Net.Http.Request
System.Net.Http.HttpRequestOut.Stop
System.Net.Http.Response
