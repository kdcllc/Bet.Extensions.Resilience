using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Bet.Extensions.Resilience.Abstractions;
using Bet.Extensions.Resilience.Abstractions.DependencyInjection;
using Bet.Extensions.Resilience.Abstractions.Options;
using Bet.Extensions.Resilience.UnitTest.Policies.Options;
using Bet.Extensions.Testing.Logging;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Polly;
using Polly.Registry;
using Polly.Timeout;

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
        public void TimeoutPolicy_Should_Register_1_Policies_With_Registry()
        {
            var policyOptionsName = PolicyOptionsKeys.TimeoutPolicy;

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

            services.AddPollyPolicy<AsyncTimeoutPolicy, TimeoutPolicyOptions>(PolicyOptionsKeys.TimeoutPolicy)
               .ConfigurePolicy(
               policyOptionsName,
               policy => policy.CreateTimeoutAsync());

            var sp = services.BuildServiceProvider();

            // simulates registrations for the policies.
            var registration = sp.GetRequiredService<DefaultPolicyProfileRegistrator>();
            registration.Register();

            var policyRegistry = sp.GetRequiredService<IPolicyRegistry<string>>();
            Assert.Equal(1, policyRegistry.Count);

            var registeredAsyncPolicy = policyRegistry.Get<IAsyncPolicy>(policyOptionsName);
            Assert.NotNull(registeredAsyncPolicy);
        }

        [Fact]
        public void TimeoutPolicy_Should_Register_3_Policies_With_Registry()
        {
            var defaultPolicyName = PolicyOptionsKeys.TimeoutPolicy;
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

            services.AddPollyPolicy<AsyncTimeoutPolicy, TimeoutPolicyOptions>(PolicyOptionsKeys.TimeoutPolicy)
                   .ConfigurePolicy(
                   defaultPolicyName,
                   policy => policy.CreateTimeoutAsync());

            services.AddPollyPolicy<AsyncTimeoutPolicy, TestTimeoutPolicyOptions>(customPolicyName)
                   .ConfigurePolicy(
                    customPolicyName,
                    policy => policy.CreateTimeoutAsync());

            var sp = services.BuildServiceProvider();

            // simulates registrations for the policies.
            var registration = sp.GetRequiredService<DefaultPolicyProfileRegistrator>();
            registration.Register();

            var policyRegistry = sp.GetRequiredService<IPolicyRegistry<string>>();

            Assert.Equal(3, policyRegistry.Count);

            var defaultAsyncPolicy = policyRegistry.Get<IAsyncPolicy>(defaultPolicyName);
            Assert.NotNull(defaultAsyncPolicy);

            var allPolicies = sp.GetServices<PolicyProfile<AsyncTimeoutPolicy, TestTimeoutPolicyOptions>>();
            Assert.Single(allPolicies);
        }

        [Fact]
        public async Task TimeoutPolicy_Async_Should_Throw_TimeoutRejectedException()
        {
            var policyOptionsName = PolicyOptionsKeys.TimeoutPolicy;

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

            services.AddPollyPolicy<AsyncTimeoutPolicy, TimeoutPolicyOptions>(PolicyOptionsKeys.TimeoutPolicy)
                   .ConfigurePolicy(
                   policyOptionsName,
                   policy => policy.CreateTimeoutAsync());

            var sp = services.BuildServiceProvider();

            var policy = (IAsyncPolicy)sp.GetRequiredService<PolicyProfile<AsyncTimeoutPolicy, TimeoutPolicyOptions>>().GetPolicy(policyOptionsName);
            Assert.NotNull(policy);

            async Task TimedOutTask()
            {
                await policy.ExecuteAsync(async () =>
                {
                    await Task.Delay(TimeSpan.FromSeconds(2));
                    await Task.CompletedTask;
                });
            }

            await Assert.ThrowsAsync<TimeoutRejectedException>(async () => await TimedOutTask());
        }

        [Fact]
        public async Task TimeoutPolicy_With_Result_Async_Should_Throw_TimeoutRejectedException()
        {
            var policyOptionsName = PolicyOptionsKeys.TimeoutPolicy;

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

            services.AddPollyPolicy<AsyncTimeoutPolicy<string>, TimeoutPolicyOptions>(PolicyOptionsKeys.TimeoutPolicy)
                  .ConfigurePolicy(
                  policyOptionsName,
                  policy => PolicyProfileCreators.CreateTimeoutAsync<TimeoutPolicyOptions, string>(policy));

            var sp = services.BuildServiceProvider();

            var policy = (IAsyncPolicy<string>)sp.GetRequiredService<PolicyProfile<AsyncTimeoutPolicy, TimeoutPolicyOptions>>().GetPolicy(policyOptionsName);
            Assert.NotNull(policy);

            async Task<string> TimedOutTask()
            {
                return await policy.ExecuteAsync(async () =>
                {
                    await Task.Delay(TimeSpan.FromSeconds(2));
                    return await Task.FromResult("Hello");
                });
            }

            await Assert.ThrowsAsync<TimeoutRejectedException>(async () => await TimedOutTask());
        }

        [Fact]
        public async Task TimeoutPolicy_Async_Should_Succeeded()
        {
            var policyOptionsName = PolicyOptionsKeys.TimeoutPolicy;

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

            services.AddPollyPolicy<AsyncTimeoutPolicy, TimeoutPolicyOptions>(PolicyOptionsKeys.TimeoutPolicy)
                   .ConfigurePolicy(
                   policyOptionsName,
                   policy => policy.CreateTimeoutAsync());

            var sp = services.BuildServiceProvider();

            var policy = (IAsyncPolicy)sp.GetRequiredService<PolicyProfile<AsyncTimeoutPolicy, TimeoutPolicyOptions>>().GetPolicy(policyOptionsName);
            Assert.NotNull(policy);

            async Task TimedOutTask()
            {
                await policy.ExecuteAsync(async () =>
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
            var policyOptionsName = PolicyOptionsKeys.TimeoutPolicy;

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

            services.AddPollyPolicy<AsyncTimeoutPolicy<string>, TimeoutPolicyOptions>(policyOptionsName)
                   .ConfigurePolicy(
                   policyOptionsName,
                   policy => PolicyProfileCreators.CreateTimeoutAsync<TimeoutPolicyOptions, string>(policy));

            var sp = services.BuildServiceProvider();

            // simulates registrations for the policies.
            var registration = sp.GetRequiredService<DefaultPolicyProfileRegistrator>();
            registration.Register();

            var policy = (IAsyncPolicy<string>)sp.GetRequiredService<PolicyProfile<AsyncTimeoutPolicy, TimeoutPolicyOptions>>().GetPolicy(policyOptionsName);
            Assert.NotNull(policy);

            async Task<string> TimedOutTask()
            {
                return await policy.ExecuteAsync(async () =>
                {
                    await Task.Delay(TimeSpan.FromSeconds(1));
                    return await Task.FromResult("Hello");
                });
            }

            Assert.Equal("Hello", await TimedOutTask());
        }

        [Fact]
        public void TimeoutPolicy_Sync_Should_Throw_TimeoutRejectedException()
        {
            var policyOptionsName = PolicyOptionsKeys.TimeoutPolicy;

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

            services.AddPollyPolicy<TimeoutPolicy, TimeoutPolicyOptions>(PolicyOptionsKeys.TimeoutPolicy)
                   .ConfigurePolicy(
                   policyOptionsName,
                   policy => policy.CreateTimeout());

            var sp = services.BuildServiceProvider();

            var policy = (ISyncPolicy)sp.GetRequiredService<PolicyProfile<TimeoutPolicy, TimeoutPolicyOptions>>().GetPolicy(policyOptionsName);
            Assert.NotNull(policy);

            void TimedOutTask()
            {
                policy.Execute(() =>
                {
                    Thread.Sleep(TimeSpan.FromSeconds(2));
                    return;
                });
            }

            Assert.Throws<TimeoutRejectedException>(() => TimedOutTask());
        }

        [Fact]
        public void TimeoutPolicy_With_Result_Sync_Should_Throw_TimeoutRejectedException()
        {
            var policyOptionsName = PolicyOptionsKeys.TimeoutPolicy;

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

            services.AddPollyPolicy<TimeoutPolicy<string>, TimeoutPolicyOptions>(PolicyOptionsKeys.TimeoutPolicy)
                   .ConfigurePolicy(
                   policyOptionsName,
                   policy => PolicyProfileCreators.CreateTimeout<TimeoutPolicyOptions, string>(policy));

            var sp = services.BuildServiceProvider();

            var policy = (ISyncPolicy<string>)sp.GetRequiredService<PolicyProfile<TimeoutPolicy, TimeoutPolicyOptions>>().GetPolicy(policyOptionsName);
            Assert.NotNull(policy);

            string TimedOutTask()
            {
                return policy.Execute(() =>
                {
                    Thread.Sleep(TimeSpan.FromSeconds(2));
                    return "Hello";
                });
            }

            Assert.Throws<TimeoutRejectedException>(() => TimedOutTask());
        }

        [Fact]
        public void TimeoutPolicy_Sync_Should_Succeeded()
        {
            var policyOptionsName = PolicyOptionsKeys.TimeoutPolicy;

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

            services.AddPollyPolicy<TimeoutPolicy, TimeoutPolicyOptions>(PolicyOptionsKeys.TimeoutPolicy)
                   .ConfigurePolicy(
                   policyOptionsName,
                   policy => policy.CreateTimeout());

            var sp = services.BuildServiceProvider();

            // simulates registrations for the policies.
            var registration = sp.GetRequiredService<DefaultPolicyProfileRegistrator>();
            registration.Register();

            var policy = (ISyncPolicy)sp.GetRequiredService<PolicyProfile<TimeoutPolicy, TimeoutPolicyOptions>>().GetPolicy(policyOptionsName);
            Assert.NotNull(policy);

            void TimedOutTask()
            {
                policy.Execute(() =>
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
            var policyOptionsName = PolicyOptionsKeys.TimeoutPolicy;

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

            services.AddPollyPolicy<TimeoutPolicy<string>, TimeoutPolicyOptions>(PolicyOptionsKeys.TimeoutPolicy)
                   .ConfigurePolicy(
                   policyOptionsName,
                   policy => PolicyProfileCreators.CreateTimeout<TimeoutPolicyOptions, string>(policy));

            var sp = services.BuildServiceProvider();

            var policy = (ISyncPolicy<string>)sp.GetRequiredService<PolicyProfile<TimeoutPolicy, TimeoutPolicyOptions>>().GetPolicy(policyOptionsName);
            Assert.NotNull(policy);

            string TimedOutTask()
            {
                return policy.Execute(() =>
                {
                    Thread.Sleep(TimeSpan.FromSeconds(1));
                    return "Hello";
                });
            }

            Assert.Equal("Hello", TimedOutTask());
        }
    }
}
