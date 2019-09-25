using System;
using System.Collections.Generic;
using System.Linq;
using Bet.Extensions.Resilience.Http.Options;
using Bet.Extensions.Resilience.Http.Policies;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Polly;
using Polly.Registry;

using Xunit;
using Xunit.Abstractions;

namespace Bet.Extensions.Resilience.UnitTest.PolicyBuilders
{
    public class ResilienceHttpPolicyBuilderTests
    {
        public ResilienceHttpPolicyBuilderTests(ITestOutputHelper output)
        {
            Output = output;
        }

        public ITestOutputHelper Output { get; }

        [Fact]
        public void Should_Not_Allow_Two_Policy_With_The_Same_Name_Throws_Argument_Exception()
        {
            var services = new ServiceCollection();
            var dic = new Dictionary<string, string>
            {
                { "Policies:HttpCircuitBreaker:DurationOfBreak", "00:00:14" },
                { "Policies:HttpCircuitBreaker:ExceptionsAllowedBeforeBreaking", "6" },
                { "Policies:HttpRetry:BackOffPower", "1" },
                { "Policies:HttpRetry:Count", "10" },
                { "Policies:CustomCount", "100" }
            };
            var config = new ConfigurationBuilder().AddInMemoryCollection(dic).Build();
            services.AddOptions();
            services.AddSingleton<IConfiguration>(config);

            services.AddHttpResiliencePolicy();

            Assert.Throws<ArgumentException>(() => services.AddHttpResiliencePolicy<CustomResilientOptions>());
        }

        [Fact]
        public void Should_Not_Allow_Two_Default_Policy_With_The_Same_Name_Throws_Argument_Exception()
        {
            var services = new ServiceCollection();
            var dic = new Dictionary<string, string>
            {
                { "Policies:HttpCircuitBreaker:DurationOfBreak", "00:00:14" },
                { "Policies:HttpCircuitBreaker:ExceptionsAllowedBeforeBreaking", "6" },
                { "Policies:HttpRetry:BackOffPower", "1" },
                { "Policies:HttpRetry:Count", "10" },
                { "Policies:CustomCount", "100" }
            };
            var config = new ConfigurationBuilder().AddInMemoryCollection(dic).Build();
            services.AddOptions();
            services.AddSingleton<IConfiguration>(config);

            services.AddHttpDefaultResiliencePolicies();

            Assert.Throws<ArgumentException>(() => services.AddHttpDefaultResiliencePolicies<CustomResilientOptions>());
        }

        [Fact]
        public void Should_Configure_Default_Policies_And_Options()
        {
            var services = new ServiceCollection();
            var dic = new Dictionary<string, string>
            {
                { "Policies:Timeout", "100" },
                { "Policies:HttpRetry:BackOffPower", "1" },
                { "Policies:HttpRetry:Count", "10" },
                { "Policies:HttpCircuitBreaker:DurationOfBreak", "00:00:14" },
                { "Policies:HttpCircuitBreaker:ExceptionsAllowedBeforeBreaking", "6" }
            };

            var config = new ConfigurationBuilder().AddInMemoryCollection(dic).Build();
            services.AddOptions();
            services.AddSingleton<IConfiguration>(config);

            services.AddLogging(builder => builder.AddProvider(new XunitLoggerProvider(Output)));

            services.AddHttpDefaultResiliencePolicies<HttpPolicyOptions>();

            var sp = services.BuildServiceProvider();

            // simulates the hosting service registration
            var registration = sp.GetService<IHttpPolicyRegistrator>();
            registration.ConfigurePolicies();

            var policy = sp.GetRequiredService<IPolicyRegistry<string>>();

            // no policy added at this point to the registry
            Assert.Equal(6, policy.Count);

            Assert.True(policy.ContainsKey(HttpPoliciesKeys.HttpTimeoutPolicy));
            Assert.True(policy.ContainsKey(HttpPoliciesKeys.HttpTimeoutPolicyAsync));

            Assert.True(policy.ContainsKey(HttpPoliciesKeys.HttpWaitAndRetryPolicy));
            Assert.True(policy.ContainsKey(HttpPoliciesKeys.HttpWaitAndRetryPolicyAsync));

            Assert.True(policy.ContainsKey(HttpPoliciesKeys.HttpCircuitBreakerPolicy));
            Assert.True(policy.ContainsKey(HttpPoliciesKeys.HttpCircuitBreakerPolicyAsync));

            var options = sp.GetRequiredService<IHttpPolicyConfigurator<HttpPolicyOptions>>();

            Assert.Equal(7, options.OptionsCollection.Count);
            Assert.Equal(3, options.AsyncPolicyCollection.Count);
            Assert.Equal(3, options.SyncPolicyCollection.Count);

            var individualPolicies = sp.GetServices<IHttpPolicy<HttpPolicyOptions>>();

            Assert.Equal(3, individualPolicies.Count());

            // Assert.Equal(TimeSpan.FromSeconds(14), options.HttpCircuitBreaker.DurationOfBreak);
            // Assert.Equal(6, options.HttpCircuitBreaker.ExceptionsAllowedBeforeBreaking);
            // Assert.Equal(1, options.HttpRetry.BackoffPower);
            // Assert.Equal(10, options.HttpRetry.Count);
            // Assert.Equal(100, options.CustomCount);
            // Assert.Equal(HttpPoliciesKeys.DefaultPolicies, options.PolicyName);
        }

        [Fact]
        public void Should_Configure_Options_When_Use_Custom_Section_Name()
        {
            var services = new ServiceCollection();
            var dic = new Dictionary<string, string>
            {
                { "CustomPolicies:HttpCircuitBreaker:DurationOfBreak", "00:00:14" },
                { "CustomPolicies:HttpCircuitBreaker:ExceptionsAllowedBeforeBreaking", "6" },
                { "CustomPolicies:HttpRetry:BackOffPower", "1" },
                { "CustomPolicies:HttpRetry:Count", "10" },
                { "CustomPolicies:CustomCount", "100" }
            };
            var config = new ConfigurationBuilder().AddInMemoryCollection(dic).Build();

            services.AddOptions();
            services.AddSingleton<IConfiguration>(config);
            services.AddHttpResiliencePolicy<CustomResilientOptions>(policySectionName: "CustomPolicies");

            var sp = services.BuildServiceProvider();
            var policy = sp.GetRequiredService<IPolicyRegistry<string>>();

            // no policy added at this point to the registry
            Assert.Equal(0, policy.Count);
            var options = sp.GetRequiredService<IOptionsMonitor<CustomResilientOptions>>().Get(HttpPoliciesKeys.DefaultPolicies);

            Assert.Equal(TimeSpan.FromSeconds(14), options.HttpCircuitBreaker.DurationOfBreak);
            Assert.Equal(6, options.HttpCircuitBreaker.ExceptionsAllowedBeforeBreaking);
            Assert.Equal(1, options.HttpRetry.BackoffPower);
            Assert.Equal(10, options.HttpRetry.Count);
            Assert.Equal(100, options.CustomCount);
            Assert.Equal(HttpPoliciesKeys.DefaultPolicies, options.PolicyName);
        }

        [Fact]
        public void Should_Configure_Options_When_Use_Custom_Policy_Name()
        {
            var services = new ServiceCollection();
            var dic = new Dictionary<string, string>
            {
                { "Policies:HttpCircuitBreaker:DurationOfBreak", "00:00:14" },
                { "Policies:HttpCircuitBreaker:ExceptionsAllowedBeforeBreaking", "6" },
                { "Policies:HttpRetry:BackOffPower", "1" },
                { "Policies:HttpRetry:Count", "10" },
                { "Policies:CustomCount", "100" }
            };
            var config = new ConfigurationBuilder().AddInMemoryCollection(dic).Build();
            services.AddOptions();
            services.AddSingleton<IConfiguration>(config);
            services.AddHttpResiliencePolicy<CustomResilientOptions>(policyName: "CustomName");

            var sp = services.BuildServiceProvider();
            var policy = sp.GetRequiredService<IPolicyRegistry<string>>();

            // no policy added at this point to the registry
            Assert.Equal(0, policy.Count);

            var options = sp.GetRequiredService<IOptionsMonitor<CustomResilientOptions>>().Get("CustomName");

            Assert.Equal(TimeSpan.FromSeconds(14), options.HttpCircuitBreaker.DurationOfBreak);
            Assert.Equal(6, options.HttpCircuitBreaker.ExceptionsAllowedBeforeBreaking);
            Assert.Equal(1, options.HttpRetry.BackoffPower);
            Assert.Equal(10, options.HttpRetry.Count);
            Assert.Equal(100, options.CustomCount);
        }

        [Fact]
        public void Should_Configure_Default_Options_When_Use_Custom_Section_And_PolicyName()
        {
            var services = new ServiceCollection();
            var dic = new Dictionary<string, string>
            {
                { "Policy:HttpCircuitBreaker:DurationOfBreak", "00:00:14" },
                { "Policy:HttpCircuitBreaker:ExceptionsAllowedBeforeBreaking", "6" },
                { "Policy:HttpRetry:BackOffPower", "1" },
                { "Policy:HttpRetry:Count", "10" },
            };

            var config = new ConfigurationBuilder().AddInMemoryCollection(dic).Build();
            services.AddOptions();
            services.AddSingleton<IConfiguration>(config);

            services.AddHttpResiliencePolicy(policySectionName: "Policy", policyName: "CustomName");

            var sp = services.BuildServiceProvider();
            var policy = sp.GetRequiredService<IPolicyRegistry<string>>();

            // no policy added at this point to the registry
            Assert.Equal(0, policy.Count);

            var options = sp.GetRequiredService<IOptionsMonitor<HttpPolicyOptions>>().Get("CustomName");

            Assert.Equal(TimeSpan.FromSeconds(14), options.HttpCircuitBreaker.DurationOfBreak);
            Assert.Equal(6, options.HttpCircuitBreaker.ExceptionsAllowedBeforeBreaking);
            Assert.Equal(1, options.HttpRetry.BackoffPower);
            Assert.Equal(10, options.HttpRetry.Count);
        }

        [Fact]
        public void Should_Configure_Custom_Options_When_Use_Custom_Section_And_PolicyName()
        {
            var services = new ServiceCollection();
            var dic = new Dictionary<string, string>
            {
                { "Policy:HttpCircuitBreaker:DurationOfBreak", "00:00:14" },
                { "Policy:HttpCircuitBreaker:ExceptionsAllowedBeforeBreaking", "6" },
                { "Policy:HttpRetry:BackOffPower", "1" },
                { "Policy:HttpRetry:Count", "10" },
                { "Policy:CustomCount", "100" }
            };
            var config = new ConfigurationBuilder().AddInMemoryCollection(dic).Build();
            services.AddOptions();
            services.AddSingleton<IConfiguration>(config);

            services.AddHttpResiliencePolicy<CustomResilientOptions>(policySectionName: "Policy", policyName: "CustomPolicy");
            var sp = services.BuildServiceProvider();

            var policy = sp.GetRequiredService<IPolicyRegistry<string>>();

            // no policy added at this point to the registry
            Assert.Equal(0, policy.Count);

            var options = sp.GetRequiredService<IOptionsMonitor<CustomResilientOptions>>().Get("CustomPolicy");

            Assert.Equal(TimeSpan.FromSeconds(14), options.HttpCircuitBreaker.DurationOfBreak);
            Assert.Equal(6, options.HttpCircuitBreaker.ExceptionsAllowedBeforeBreaking);
            Assert.Equal(1, options.HttpRetry.BackoffPower);
            Assert.Equal(10, options.HttpRetry.Count);
            Assert.Equal(100, options.CustomCount);
            Assert.Equal("CustomPolicy", options.PolicyName);
        }

        [Fact]
        public void Should_Configure_Options_As_Per_The_Configuration_Action()
        {
            var services = new ServiceCollection();
            var dic = new Dictionary<string, string>
            {
                { "Policies:HttpCircuitBreaker:DurationOfBreak", "00:00:14" },
                { "Policies:HttpCircuitBreaker:ExceptionsAllowedBeforeBreaking", "6" },
                { "Policies:HttpRetry:BackOffPower", "1" },
                { "Policies:HttpRetry:Count", "10" },
                { "Policies:CustomCount", "100" }
            };

            var config = new ConfigurationBuilder().AddInMemoryCollection(dic).Build();

            services.AddOptions();
            services.AddSingleton<IConfiguration>(config);

            services.AddHttpResiliencePolicy(o =>
            {
                o.HttpCircuitBreaker.ExceptionsAllowedBeforeBreaking = 83;
                o.HttpRetry.BackoffPower = 22;
            });

            var sp = services.BuildServiceProvider();
            var policy = sp.GetRequiredService<IPolicyRegistry<string>>();

            // no policy added at this point to the registry
            Assert.Equal(0, policy.Count);
            var options = sp.GetRequiredService<IOptionsMonitor<HttpPolicyOptions>>().Get(HttpPoliciesKeys.DefaultPolicies);

            Assert.Equal(TimeSpan.FromSeconds(14), options.HttpCircuitBreaker.DurationOfBreak);
            Assert.Equal(83, options.HttpCircuitBreaker.ExceptionsAllowedBeforeBreaking);
            Assert.Equal(22, options.HttpRetry.BackoffPower);
            Assert.Equal(10, options.HttpRetry.Count);
            Assert.Equal(HttpPoliciesKeys.DefaultPolicies, options.PolicyName);
        }
    }
}
