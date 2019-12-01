using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Bet.Extensions.Resilience.Abstractions.Options;
using Bet.Extensions.Resilience.Abstractions.Policies;
using Bet.Extensions.Testing.Logging;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Xunit;
using Xunit.Abstractions;

namespace Bet.Extensions.Resilience.UnitTest.Policies
{
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
            var policyOptionsName = RetryPolicyOptions.DefaultName;

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

            services.AddResiliencePolicy<IRetryPolicy<RetryPolicyOptions>, RetryPolicy<RetryPolicyOptions>, RetryPolicyOptions>(
                policyOptionsName,
                policyOptionsName);

            var sp = services.BuildServiceProvider();

            var policy = sp.GetRequiredService<IRetryPolicy<RetryPolicyOptions>>().GetAsyncPolicy();
            Assert.NotNull(policy);

            async Task TimedOutTask()
            {
                await policy.ExecuteAsync(async () =>
                {
                    throw new Exception("Failed");
                });
            }

            await Assert.ThrowsAsync<Exception>(async () => await TimedOutTask());
        }

        [Fact]
        public async Task RetryPolicy_Async_With_Result_Should_Throw_Exception()
        {
            var policyOptionsName = RetryPolicyOptions.DefaultName;

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

            services.AddResiliencePolicy<IRetryPolicy<RetryPolicyOptions, bool>, RetryPolicy<RetryPolicyOptions, bool>, RetryPolicyOptions, bool>(
                policyOptionsName,
                policyOptionsName);

            var sp = services.BuildServiceProvider();

            var policy = sp.GetRequiredService<IRetryPolicy<RetryPolicyOptions, bool>>().GetAsyncPolicy();
            Assert.NotNull(policy);

            async Task<bool> TimedOutTask()
            {
                return await policy.ExecuteAsync(async () =>
                {
                    throw new Exception("Failed");
                });
            }

            await Assert.ThrowsAsync<Exception>(async () => await TimedOutTask());
        }
    }
}
