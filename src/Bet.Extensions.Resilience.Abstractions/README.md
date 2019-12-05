# Bet.Extensions.Resilience.Abstractions

[![Build status](https://ci.appveyor.com/api/projects/status/tmqs7xbq1aqee3md/branch/master?svg=true)](https://ci.appveyor.com/project/kdcllc/bet-extensions-resilience/branch/master)
[![NuGet](https://img.shields.io/nuget/v/Bet.Extensions.Resilience.Abstractions.svg)](https://www.nuget.org/packages?q=Bet.Extensions.Resilience.Abstractions)
[![MyGet](https://img.shields.io/myget/kdcllc/v/Bet.Extensions.Resilience.Abstractions.svg?label=myget)](https://www.myget.org/F/kdcllc/api/v2)

The abstraction library for Bet.Extensions.Resilience [`Polly`](https://github.com/App-vNext/Polly).

It provides with easy configuration syntax for [`Polly`](https://github.com/App-vNext/Polly) policies.

## Install

```bash
    dotnet add package Bet.Extensions.Resilience.Abstractions
```

## Usage

1. Add Policies with their Shapes

```csharp

services.AddPollyPolicy<AsyncTimeoutPolicy, TimeoutPolicyOptions>("TimeoutPolicyOptimistic")

        .ConfigurePolicy(
            sectionName: PolicyOptionsKeys.TimeoutPolicy,
            (policy) =>
            {
                policy.CreateTimeoutAsync(TimeoutStrategy.Optimistic);
            })

        .ConfigurePolicy(
            sectionName: PolicyOptionsKeys.TimeoutPolicy,
            (policy) =>
            {
                policy.ConfigurePolicy = (options, logger) =>
                {
                    logger.LogInformation("Hello TimeoutPolicyOptimistic");

                    return Policy.TimeoutAsync(options.Timeout, TimeoutStrategy.Optimistic);
                };
            },
            policyName: "TimeoutPolicyAsync");

services.AddPollyPolicy<AsyncTimeoutPolicy<bool>, TimeoutPolicyOptions>("TimeoutPolicyPessimistic")
        .ConfigurePolicy(
            sectionName: "DefaultPolicy:TimeoutPolicy",
            (policy) =>
            {
                PolicyShapes.CreateTimeoutAsync<TimeoutPolicyOptions, bool>(policy);
            });
```

2. Use withing the other components by getting values from the `PolicyBucket`

```csharp

    var policy = host.Services.GetRequiredService<PolicyBucket<AsyncTimeoutPolicy, TimeoutPolicyOptions>>();

    var optimisticPolicy = policy.GetPolicy("TimeoutPolicyAsync") as IAsyncPolicy;
    var pessimisticPolicy = policy.GetPolicy("TimeoutPolicyPessimistic") as IAsyncPolicy<bool>;
```

3. Or using `IPolicyRegistry<string>` if the policies are configured for the host

```csharp
    var policyRegistry = host.Services.GetRequiredService<IPolicyRegistry<string>>();

    var pessemisticPolicy = host.Services.GetRequiredService<IPolicyRegistry<string>>()
                    .Get<IAsyncPolicy<bool>>("TimeoutPolicyPessimistic");
```