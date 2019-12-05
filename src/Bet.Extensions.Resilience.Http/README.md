# Bet.Extensions.Resilience.Http

[![Build status](https://ci.appveyor.com/api/projects/status/tmqs7xbq1aqee3md/branch/master?svg=true)](https://ci.appveyor.com/project/kdcllc/bet-extensions-resilience/branch/master)
[![NuGet](https://img.shields.io/nuget/v/Bet.Extensions.Resilience.Http.svg)](https://www.nuget.org/packages?q=Bet.Extensions.Resilience.Http)
[![MyGet](https://img.shields.io/myget/kdcllc/v/Bet.Extensions.Resilience.Http.svg?label=myget)](https://www.myget.org/F/kdcllc/api/v2)

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