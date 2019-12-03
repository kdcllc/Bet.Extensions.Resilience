using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Bet.Extensions.Resilience.Abstractions;
using Bet.Extensions.Resilience.Abstractions.Options;
using Bet.Extensions.Resilience.Abstractions.Policies;
using Bet.Extensions.Resilience.UnitTest.Policies.Options;
using Bet.Extensions.Testing.Logging;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Registry;

using Xunit;
using Xunit.Abstractions;

namespace Bet.Extensions.Resilience.UnitTest.Policies
{
    public class TimeoutPolicyTests
    {
        private readonly ITestOutputHelper _output;

        public TimeoutPolicyTests(ITestOutputHelper output)
        {
            _output = output ?? throw new ArgumentNullException(nameof(output));
        }

        [Fact]
        public void TimeoutPolicy_Should_Register_2_Policies_With_Registry()
        {
            var policyOptionsName = TimeoutPolicyOptions.DefaultName;

            var services = new ServiceCollection();

            // logger is required for policies.
            services.AddLogging(builder =>
            {
                builder.AddXunit(_output);
            });

            var dic = new Dictionary<string, string>
            {
                { $"{policyOptionsName}:Timeout", "00:00:01" },
            };

            var config = new ConfigurationBuilder().AddInMemoryCollection(dic).Build();
            services.AddSingleton<IConfiguration>(config);

            services.AddResiliencePolicy<ITimeoutPolicy<TimeoutPolicyOptions>, TimeoutPolicy<TimeoutPolicyOptions>, TimeoutPolicyOptions>(
                policyOptionsName,
                policyOptionsName);

            var sp = services.BuildServiceProvider();

            // configure policies within Policy Registry.
            var configure = sp.GetRequiredService<IPolicyRegistrator>();
            configure.ConfigurePolicies();

            var policyRegistry = sp.GetRequiredService<IPolicyRegistry<string>>();
            Assert.Equal(2, policyRegistry.Count);

            var registeredAsyncPolicy = policyRegistry.Get<IAsyncPolicy>($"{policyOptionsName}Async");
            Assert.NotNull(registeredAsyncPolicy);
        }

        [Fact]
        public void TimeoutPolicy_Should_Register_4_Policies_With_Registry()
        {
            var defaultPolicyName = TimeoutPolicyOptions.DefaultName;
            var customPolicyName = "TestTimeoutPolicy";

            var services = new ServiceCollection();
            // logger is required for policies.
            services.AddLogging(builder =>
            {
                builder.AddXunit(_output);
            });
            var dic = new Dictionary<string, string>
            {
                { $"{defaultPolicyName}:Timeout", "00:00:01" },
                { $"{customPolicyName}:Timeout", "00:00:02" },
            };
            var config = new ConfigurationBuilder().AddInMemoryCollection(dic).Build();
            services.AddSingleton<IConfiguration>(config);

            services.AddResiliencePolicy<ITimeoutPolicy<TimeoutPolicyOptions>, TimeoutPolicy<TimeoutPolicyOptions>, TimeoutPolicyOptions>(
                defaultPolicyName,
                defaultPolicyName);

            services.AddResiliencePolicy<ITimeoutPolicy<TestTimeoutPolicyOptions>, TimeoutPolicy<TestTimeoutPolicyOptions>, TestTimeoutPolicyOptions>(
                customPolicyName,
                customPolicyName);

            var sp = services.BuildServiceProvider();
            // configure policies within Policy Registry.

            var configure = sp.GetRequiredService<IPolicyRegistrator>();

            configure.ConfigurePolicies();

            var policyRegistry = sp.GetRequiredService<IPolicyRegistry<string>>();

            Assert.Equal(4, policyRegistry.Count);

            var defaultAsyncPolicy = policyRegistry.Get<IAsyncPolicy>($"{defaultPolicyName}Async");
            Assert.NotNull(defaultAsyncPolicy);

            var allPolicies = sp.GetServices<ITimeoutPolicy<TestTimeoutPolicyOptions>>();
            Assert.Single(allPolicies);
        }

        [Fact]
        public async Task TimeoutPolicy_Async_Should_Throw_TimeoutRejectedException()
        {
            var policyOptionsName = TimeoutPolicyOptions.DefaultName;

            var services = new ServiceCollection();

            // logger is required for policies.
            services.AddLogging(builder =>
            {
                builder.AddXunit(_output);
            });

            var dic = new Dictionary<string, string>
            {
                { $"{policyOptionsName}:Timeout", "00:00:01" },
            };

            var config = new ConfigurationBuilder().AddInMemoryCollection(dic).Build();
            services.AddSingleton<IConfiguration>(config);

            services.AddResiliencePolicy<ITimeoutPolicy<TimeoutPolicyOptions>, TimeoutPolicy<TimeoutPolicyOptions>, TimeoutPolicyOptions>(
                policyOptionsName,
                policyOptionsName);

            var sp = services.BuildServiceProvider();

            var policy = sp.GetRequiredService<ITimeoutPolicy<TimeoutPolicyOptions>>();
            Assert.NotNull(policy);

            async Task TimedOutTask()
            {
                await policy.GetAsyncPolicy().ExecuteAsync(async () =>
                {
                    await Task.Delay(TimeSpan.FromSeconds(2));
                    await Task.CompletedTask;
                });
            }

            await Assert.ThrowsAsync<Polly.Timeout.TimeoutRejectedException>(async () => await TimedOutTask());
        }

        [Fact]
        public async Task TimeoutPolicy_With_Result_Async_Should_Throw_TimeoutRejectedException()
        {
            var policyOptionsName = TimeoutPolicyOptions.DefaultName;

            var services = new ServiceCollection();

            // logger is required for policies.
            services.AddLogging(builder =>
            {
                builder.AddXunit(_output);
            });

            var dic = new Dictionary<string, string>
            {
                { $"{policyOptionsName}:Timeout", "00:00:01" },
            };

            var config = new ConfigurationBuilder().AddInMemoryCollection(dic).Build();
            services.AddSingleton<IConfiguration>(config);

            services.AddResiliencePolicy<ITimeoutPolicy<TimeoutPolicyOptions, string>, TimeoutPolicy<TimeoutPolicyOptions, string>, TimeoutPolicyOptions, string>(
                policyOptionsName,
                policyOptionsName);

            var sp = services.BuildServiceProvider();

            var policy = sp.GetRequiredService<ITimeoutPolicy<TimeoutPolicyOptions, string>>();
            Assert.NotNull(policy);

            async Task<string> TimedOutTask()
            {
                return await policy.GetAsyncPolicy().ExecuteAsync(async () =>
                {
                    await Task.Delay(TimeSpan.FromSeconds(2));
                    return await Task.FromResult("Hello");
                });
            }

            await Assert.ThrowsAsync<Polly.Timeout.TimeoutRejectedException>(async () => await TimedOutTask());
        }

        [Fact]
        public async Task TimeoutPolicy_Async_Should_Succeeded()
        {
            var policyOptionsName = TimeoutPolicyOptions.DefaultName;

            var services = new ServiceCollection();

            // logger is required for policies.
            services.AddLogging(builder =>
            {
                builder.AddXunit(_output);
            });

            var dic = new Dictionary<string, string>
            {
                { $"{policyOptionsName}:Timeout", "00:00:02" },
            };

            var config = new ConfigurationBuilder().AddInMemoryCollection(dic).Build();
            services.AddSingleton<IConfiguration>(config);

            services.AddResiliencePolicy<ITimeoutPolicy<TimeoutPolicyOptions>, TimeoutPolicy<TimeoutPolicyOptions>, TimeoutPolicyOptions>(
                policyOptionsName,
                policyOptionsName);

            var sp = services.BuildServiceProvider();

            var policy = sp.GetRequiredService<ITimeoutPolicy<TimeoutPolicyOptions>>();
            Assert.NotNull(policy);

            async Task TimedOutTask()
            {
                await policy.GetAsyncPolicy().ExecuteAsync(async () =>
                {
                    await Task.Delay(TimeSpan.FromSeconds(1));
                    await Task.CompletedTask;
                });
            }

            await TimedOutTask();
        }

        [Fact]
        public async Task TimeoutPolicy_With_Result_Async_Should_Succeeded()
        {
            var policyOptionsName = TimeoutPolicyOptions.DefaultName;

            var services = new ServiceCollection();

            // logger is required for policies.
            services.AddLogging(builder =>
            {
                builder.AddXunit(_output);
            });

            var dic = new Dictionary<string, string>
            {
                { $"{policyOptionsName}:Timeout", "00:00:02" },
            };

            var config = new ConfigurationBuilder().AddInMemoryCollection(dic).Build();
            services.AddSingleton<IConfiguration>(config);

            services.AddResiliencePolicy<ITimeoutPolicy<TimeoutPolicyOptions, string>, TimeoutPolicy<TimeoutPolicyOptions, string>, TimeoutPolicyOptions, string>(
                policyOptionsName,
                policyOptionsName);

            var sp = services.BuildServiceProvider();

            var policy = sp.GetRequiredService<ITimeoutPolicy<TimeoutPolicyOptions, string>>();
            Assert.NotNull(policy);

            async Task<string> TimedOutTask()
            {
                return await policy.GetAsyncPolicy().ExecuteAsync(async () =>
                {
                    await Task.Delay(TimeSpan.FromSeconds(1));
                    return await Task.FromResult<string>("Hello");
                });
            }

            Assert.Equal("Hello", await TimedOutTask());
        }

        [Fact]
        public void TimeoutPolicy_Sync_Should_Throw_TimeoutRejectedException()
        {
            var policyOptionsName = TimeoutPolicyOptions.DefaultName;

            var services = new ServiceCollection();

            // logger is required for policies.
            services.AddLogging(builder =>
            {
                builder.AddXunit(_output);
            });

            var dic = new Dictionary<string, string>
            {
                { $"{policyOptionsName}:Timeout", "00:00:01" },
            };

            var config = new ConfigurationBuilder().AddInMemoryCollection(dic).Build();
            services.AddSingleton<IConfiguration>(config);

            services.AddResiliencePolicy<ITimeoutPolicy<TimeoutPolicyOptions>, TimeoutPolicy<TimeoutPolicyOptions>, TimeoutPolicyOptions>(
                policyOptionsName,
                policyOptionsName);

            var sp = services.BuildServiceProvider();

            var policy = sp.GetRequiredService<ITimeoutPolicy<TimeoutPolicyOptions>>();
            Assert.NotNull(policy);

            void TimedOutTask()
            {
                policy.GetSyncPolicy().Execute(() =>
                {
                    Thread.Sleep(TimeSpan.FromSeconds(2));
                    return;
                });
            }

            Assert.Throws<Polly.Timeout.TimeoutRejectedException>(() => TimedOutTask());
        }

        [Fact]
        public void TimeoutPolicy_With_Result_Sync_Should_Throw_TimeoutRejectedException()
        {
            var policyOptionsName = TimeoutPolicyOptions.DefaultName;

            var services = new ServiceCollection();

            // logger is required for policies.
            services.AddLogging(builder =>
            {
                builder.AddXunit(_output);
            });

            var dic = new Dictionary<string, string>
            {
                { $"{policyOptionsName}:Timeout", "00:00:01" },
            };

            var config = new ConfigurationBuilder().AddInMemoryCollection(dic).Build();
            services.AddSingleton<IConfiguration>(config);

            services.AddResiliencePolicy<ITimeoutPolicy<TimeoutPolicyOptions, string>, TimeoutPolicy<TimeoutPolicyOptions, string>, TimeoutPolicyOptions, string>(
                policyOptionsName,
                policyOptionsName);

            var sp = services.BuildServiceProvider();

            var policy = sp.GetRequiredService<ITimeoutPolicy<TimeoutPolicyOptions, string>>();
            Assert.NotNull(policy);

            string TimedOutTask()
            {
                return policy.GetSyncPolicy().Execute(() =>
                {
                    Thread.Sleep(TimeSpan.FromSeconds(2));
                    return "Hello";
                });
            }

            Assert.Throws<Polly.Timeout.TimeoutRejectedException>(() => TimedOutTask());
        }

        [Fact]
        public void TimeoutPolicy_Sync_Should_Succeeded()
        {
            var policyOptionsName = TimeoutPolicyOptions.DefaultName;

            var services = new ServiceCollection();

            // logger is required for policies.
            services.AddLogging(builder =>
            {
                builder.AddXunit(_output);
            });

            var dic = new Dictionary<string, string>
            {
                { $"{policyOptionsName}:Timeout", "00:00:02" },
            };

            var config = new ConfigurationBuilder().AddInMemoryCollection(dic).Build();
            services.AddSingleton<IConfiguration>(config);

            services.AddResiliencePolicy<ITimeoutPolicy<TimeoutPolicyOptions>, TimeoutPolicy<TimeoutPolicyOptions>, TimeoutPolicyOptions>(
                policyOptionsName,
                policyOptionsName);

            var sp = services.BuildServiceProvider();

            var policy = sp.GetRequiredService<ITimeoutPolicy<TimeoutPolicyOptions>>();
            Assert.NotNull(policy);

            void TimedOutTask()
            {
                policy.GetSyncPolicy().Execute(() =>
                {
                    Thread.Sleep(TimeSpan.FromSeconds(1));
                    return;
                });
            }

            TimedOutTask();
        }

        [Fact]
        public void TimeoutPolicy_With_Result_Sync_Should_Succeeded()
        {
            var policyOptionsName = TimeoutPolicyOptions.DefaultName;

            var services = new ServiceCollection();

            // logger is required for policies.
            services.AddLogging(builder =>
            {
                builder.AddXunit(_output);
            });

            var dic = new Dictionary<string, string>
            {
                { $"{policyOptionsName}:Timeout", "00:00:02" },
            };

            var config = new ConfigurationBuilder().AddInMemoryCollection(dic).Build();
            services.AddSingleton<IConfiguration>(config);

            services.AddResiliencePolicy<ITimeoutPolicy<TimeoutPolicyOptions, string>, TimeoutPolicy<TimeoutPolicyOptions, string>, TimeoutPolicyOptions, string>(
                policyOptionsName,
                policyOptionsName);

            var sp = services.BuildServiceProvider();

            var policy = sp.GetRequiredService<ITimeoutPolicy<TimeoutPolicyOptions, string>>();
            Assert.NotNull(policy);

            string TimedOutTask()
            {
                return policy.GetSyncPolicy().Execute(() =>
                {
                    Thread.Sleep(TimeSpan.FromSeconds(1));
                    return "Hello";
                });
            }

            Assert.Equal("Hello", TimedOutTask());
        }
    }
}
