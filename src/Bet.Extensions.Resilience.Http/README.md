# Bet.Extensions.Resilience.Http

[![GitHub license](https://img.shields.io/badge/license-MIT-blue.svg?style=flat-square)](https://raw.githubusercontent.com/kdcllc/Bet.Extensions.Resilience/master/LICENSE)

[![Build status](https://ci.appveyor.com/api/projects/status/tmqs7xbq1aqee3md/branch/master?svg=true)](https://ci.appveyor.com/project/kdcllc/bet-extensions-resilience/branch/master)

[![NuGet](https://img.shields.io/nuget/v/Bet.Extensions.Resilience.Http.svg)](https://www.nuget.org/packages?q=Bet.Extensions.Resilience.Http)
![Nuget](https://img.shields.io/nuget/dt/Bet.Extensions.Resilience.Http)

[![feedz.io](https://img.shields.io/badge/endpoint.svg?url=https://f.feedz.io/kdcllc/bet-extensions-resilience/shield/Bet.Extensions.Resilience.Http/latest)](https://f.feedz.io/kdcllc/bet-extensions-resilience/packages/Bet.Extensions.Resilience.Http/latest/download))
## Install

```cmd
    dotnet add package Bet.Extensions.Resilience.Http
```


## Usage

```csharp
    services.AddPollyPolicy<AsyncTimeoutPolicy<HttpResponseMessage>, TimeoutPolicyOptions>(HttpPolicyOptionsKeys.HttpTimeoutPolicy)
                .ConfigurePolicy(
                    sectionName: $"{sectionName}:{HttpPolicyOptionsKeys.HttpTimeoutPolicy}",
                    (policy) => PolicyShapes.CreateTimeoutAsync<TimeoutPolicyOptions, HttpResponseMessage>(policy));

    services.AddPollyPolicy<AsyncCircuitBreakerPolicy<HttpResponseMessage>, CircuitBreakerPolicyOptions>(HttpPolicyOptionsKeys.HttpCircuitBreakerPolicy)
                .ConfigurePolicy(
                    sectionName: $"{sectionName}:{HttpPolicyOptionsKeys.HttpCircuitBreakerPolicy}",
                    (policy) => policy.HttpCreateCircuitBreakerAsync());

    services.AddPollyPolicy<AsyncRetryPolicy<HttpResponseMessage>, RetryPolicyOptions>(HttpPolicyOptionsKeys.HttpRetryPolicy)
                .ConfigurePolicy(
                    sectionName: $"{sectionName}:{HttpPolicyOptionsKeys.HttpRetryPolicy}",
                    (policy) => policy.HttpCreateRetryAsync());
```
