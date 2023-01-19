using Bet.Extensions.Resilience.Abstractions;
using Bet.Extensions.Resilience.Abstractions.Options;
using Bet.Extensions.Testing.Logging;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Polly;
using Polly.Retry;

using Xunit;
using Xunit.Abstractions;

namespace Bet.Extensions.Resilience.UnitTest.Policies;

public class RetryPolicyTests
{
    private readonly ITestOutputHelper _output;

    public RetryPolicyTests(ITestOutputHelper output)
    {
        _output = output ?? throw new ArgumentNullException(nameof(output));
    }

    [Fact]
    public async Task RetryPolicy_Async_Should_Throw_Exception()
    {
        var policyOptionsName = PolicyOptionsKeys.RetryPolicy;

        var services = new ServiceCollection();

        // logger is required for policies.
        services.AddLogging(builder =>
        {
            builder.AddXunit(_output);
        });

        var dic = new Dictionary<string, string>
        {
            { $"{policyOptionsName}:BackOffPower", "2" },
            { $"{policyOptionsName}:Count", "3" },
        };

        var config = new ConfigurationBuilder().AddInMemoryCollection(dic).Build();
        services.AddSingleton<IConfiguration>(config);

        services.AddPollyPolicy<AsyncRetryPolicy, RetryPolicyOptions>(policyOptionsName)
           .ConfigurePolicy(
           policyOptionsName,
           policy => policy.CreateRetryAsync());

        var sp = services.BuildServiceProvider();

        var policy = (IAsyncPolicy)sp.GetRequiredService<PolicyBucket<AsyncRetryPolicy, RetryPolicyOptions>>().GetPolicy(policyOptionsName);
        Assert.NotNull(policy);

        async Task TimedOutTask()
        {
            await policy.ExecuteAsync(() =>
            {
                throw new Exception("Failed");
            });
        }

        await Assert.ThrowsAsync<Exception>(async () => await TimedOutTask());
    }

    [Fact]
    public async Task RetryPolicy_Async_With_Result_Should_Throw_Exception()
    {
        var policyOptionsName = PolicyOptionsKeys.RetryPolicy;

        var services = new ServiceCollection();

        // logger is required for policies.
        services.AddLogging(builder =>
        {
            builder.AddXunit(_output);
        });

        var dic = new Dictionary<string, string>
        {
            { $"{policyOptionsName}:BackOffPower", "2" },
            { $"{policyOptionsName}:Count", "3" },
        };

        var config = new ConfigurationBuilder().AddInMemoryCollection(dic).Build();
        services.AddSingleton<IConfiguration>(config);

        services.AddPollyPolicy<AsyncRetryPolicy, RetryPolicyOptions>(policyOptionsName)
           .ConfigurePolicy(
           policyOptionsName,
           policy => PolicyShapes.CreateRetryAsync<RetryPolicyOptions, bool>(policy, (outcome) => outcome.GetExceptionMessages()));

        var sp = services.BuildServiceProvider();

        var policy = (IAsyncPolicy<bool>)sp.GetRequiredService<PolicyBucket<AsyncRetryPolicy, RetryPolicyOptions>>().GetPolicy(policyOptionsName);
        Assert.NotNull(policy);

        async Task<bool> TimedOutTask()
        {
            return await policy.ExecuteAsync(() =>
            {
                throw new Exception("Failed");
            });
        }

        await Assert.ThrowsAsync<Exception>(async () => await TimedOutTask());
    }
}
