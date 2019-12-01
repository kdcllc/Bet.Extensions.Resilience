using System.Threading.Tasks;

using Bet.Extensions.Resilience.Abstractions;
using Bet.Extensions.Resilience.Abstractions.Options;
using Bet.Extensions.Resilience.Abstractions.Policies;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;

using Polly;

namespace Bet.Extensions.Resilience.Worker.Sample
{
    internal sealed class Program
    {
        public static async Task<int> Main(string[] args)
        {
            using var host = CreateHostBuilder(args).Build();

            await host.StartAsync();

            var logger = host.Services.GetRequiredService<ILogger<Program>>();

            var optionsConfigurator = host.Services.GetRequiredService<IPolicyOptionsConfigurator<BulkheadPolicyOptions>>();

            logger.LogInformation(
                "Options value: {maxParallelization}.",
                optionsConfigurator.GetOptions(BulkheadPolicyOptions.DefaultNameOptionsName).MaxParallelization);

            ChangeToken.OnChange(
                () => optionsConfigurator.GetChangeToken(),
                () =>
                {
                    // this is triggered only when the 'appsettings.json' is modified.
                    var policy = host.Services.GetRequiredService<BulkheadPolicy<BulkheadPolicyOptions>>().GetSyncPolicy();

                    policy.Execute(
                        async (context) =>
                        {
                            logger.LogInformation(
                                "Executed Policy Key: {policyKey} and Operation Key: {operationKey}",
                                context.PolicyKey,
                                context.OperationKey);

                            await Task.CompletedTask;
                        }, new Context());

                    logger.LogInformation("Options Changed: {maxParallelization} new value.", optionsConfigurator
                        .GetOptions(BulkheadPolicyOptions.DefaultNameOptionsName)
                        .MaxParallelization);
                });

            host.WaitForShutdown();

            return 0;
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .UseStartupFilter<PolicyConfiguratorStartupFilter>()
                .ConfigureServices((hostContext, services) =>
                {
                    // adds only options
                    services.ConfigureResilienceOptions<BulkheadPolicyOptions>(
                        BulkheadPolicyOptions.DefaultNameOptionsName,
                        "CustomBulkheadPolicy");

                    // adds default policy
                    services
                    .AddResiliencePolicy<BulkheadPolicy<BulkheadPolicyOptions>, BulkheadPolicyOptions>(
                        BulkheadPolicyOptions.DefaultName,
                        BulkheadPolicyOptions.DefaultNameOptionsName);

                    // adds default policy options but new policy key
                    services
                    .AddResiliencePolicy<BulkheadPolicy<BulkheadPolicyOptions>, BulkheadPolicyOptions>(
                        "CustomBulkheadPolicy2",
                        BulkheadPolicyOptions.DefaultNameOptionsName,
                        "CustomBulkheadPolicy2");
                });
        }
    }
}
