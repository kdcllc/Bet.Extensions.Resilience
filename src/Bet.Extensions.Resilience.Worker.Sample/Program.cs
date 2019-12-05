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

            var policies = host.Services.GetServices<PolicyProfile<AsyncTimeoutPolicy, TimeoutPolicyOptions>>();

            var policy = host.Services.GetRequiredService<PolicyProfile<AsyncTimeoutPolicy, TimeoutPolicyOptions>>();

            var optimisticPolicy = policy.GetPolicy("TimeoutPolicyAsync") as IAsyncPolicy;
            var pessimisticPolicy = policy.GetPolicy("TimeoutPolicyPessimistic") as IAsyncPolicy<bool>;

            ChangeToken.OnChange(
                () => config.GetReloadToken(),
                async () =>
                {
                    var srv = host.Services.GetRequiredService<IOptionsMonitor<TimeoutPolicyOptions>>().Get("TimeoutPolicyOptimistic");

                    var policy = host.Services.GetRequiredService<PolicyProfile<AsyncTimeoutPolicy, TimeoutPolicyOptions>>();

                    var optimisticPolicy = policy.GetPolicy("TimeoutPolicyOptimistic") as IAsyncPolicy;
                    var pessimisticPolicy = policy.GetPolicy("TimeoutPolicyPessimistic") as IAsyncPolicy<bool>;

                    var result = await pessimisticPolicy.ExecuteAsync(async () =>
                    {
                        await Task.Delay(TimeSpan.FromSeconds(3));

                        return true;
                    });
                });

            var registry = host.Services.GetRequiredService<IPolicyRegistry<string>>();

            var regPolicy = host.Services.GetRequiredService<IPolicyRegistry<string>>().Get<IAsyncPolicy<bool>>("TimeoutPolicyPessimistic");

            var result = await regPolicy.ExecuteAsync(async () =>
            {
                await Task.Delay(TimeSpan.FromSeconds(3));

                return true;
            });

            logger.LogInformation("Result: {0}", result);
            host.WaitForShutdown();

            return 0;
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)

                    .UseStartupFilter<PolicyConfiguratorStartupFilter>()
                    .ConfigureServices((hostContext, services) =>
                    {
                        services.AddPollyPolicy<AsyncTimeoutPolicy, TimeoutPolicyOptions>("TimeoutPolicyOptimistic")
                                .ConfigurePolicy(
                                    PolicyOptionsKeys.TimeoutPolicy,
                                    (policy) =>
                                    {
                                        policy.CreateTimeoutAsync(TimeoutStrategy.Optimistic);
                                    },
                                    policyName: "TimeoutPolicy")
                                    .ConfigurePolicy(
                                    PolicyOptionsKeys.TimeoutPolicy,
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
                                .ConfigurePolicy("DefaultPolicy:TimeoutPolicy", (policy) =>
                                {
                                    PolicyProfileCreators.CreateTimeoutAsync<TimeoutPolicyOptions, bool>(policy);
                                });
                    });
        }
    }
}
