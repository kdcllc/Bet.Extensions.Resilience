using System;
using System.Threading.Tasks;

using Bet.Extensions.Resilience.Abstractions;
using Bet.Extensions.Resilience.Abstractions.Options;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;

using Polly;
using Polly.Registry;
using Polly.Timeout;

namespace Bet.Extensions.Resilience.Worker.Sample
{
    internal sealed class Program
    {
        public static async Task<int> Main(string[] args)
        {
            using var host = CreateHostBuilder(args).Build();

            await host.StartAsync();

            var config = host.Services.GetRequiredService<IConfiguration>();
            var logger = host.Services.GetRequiredService<ILogger<Program>>();

            var policies = host.Services.GetServices<PolicyBucket<AsyncTimeoutPolicy, TimeoutPolicyOptions>>();

            var policy = host.Services.GetRequiredService<PolicyBucket<AsyncTimeoutPolicy, TimeoutPolicyOptions>>();

            var optimisticPolicy = policy.GetPolicy("TimeoutPolicyAsync") as IAsyncPolicy;
            var pessimisticPolicy = policy.GetPolicy("TimeoutPolicyPessimistic") as IAsyncPolicy<bool>;

            ChangeToken.OnChange(
                () => config.GetReloadToken(),
                async () =>
                {
                    var srv = host.Services.GetRequiredService<IOptionsMonitor<TimeoutPolicyOptions>>().Get("TimeoutPolicyOptimistic");

                    var policy = host.Services.GetRequiredService<PolicyBucket<AsyncTimeoutPolicy, TimeoutPolicyOptions>>();

                    var optimisticPolicy = policy.GetPolicy("TimeoutPolicyOptimistic") as IAsyncPolicy;
                    var pessimisticPolicy = policy.GetPolicy("TimeoutPolicyPessimistic") as IAsyncPolicy<bool>;

                    var result = await pessimisticPolicy.ExecuteAsync(async () =>
                    {
                        await Task.Delay(TimeSpan.FromSeconds(3));

                        return true;
                    });
                });

            var policyRegistry = host.Services.GetRequiredService<IPolicyRegistry<string>>();

            var pessemisticPolicy = host.Services.GetRequiredService<IPolicyRegistry<string>>()
                .Get<IAsyncPolicy<bool>>("TimeoutPolicyPessimistic");

            try
            {
                var result = await pessemisticPolicy.ExecuteAsync(async () =>
                {
                    await Task.Delay(TimeSpan.FromSeconds(3));

                    return true;
                });

                logger.LogInformation("Result: {0}", result);
            }
            catch
            {
            }

            host.WaitForShutdown();

            return 0;
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)

                    .UseStartupFilter<PolicyConfiguratorStartupFilter>()
                    .UseCorrelationId(options => options.UseGuidForCorrelationId = true)
                    .ConfigureServices((hostContext, services) =>
                    {
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
                    });
        }
    }
}
