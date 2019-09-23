using System;
using System.Collections.Generic;

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
        public void Should_throw_exception_NullReferenceException()
        {
            // Assign
            var serviceCollection = new ServiceCollection();

            serviceCollection.AddLogging(builder => builder.AddProvider(new XunitLoggerProvider(Output)));
            var logger = serviceCollection.BuildServiceProvider().GetRequiredService<ILoggerFactory>().CreateLogger("test");
            var pollyContext = new Context();

            pollyContext.AddLogger(logger);
        }

        [Fact]
        public void Should_Configure_Options()
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

            services.AddResiliencePolicyConfiguration();
            services.AddResiliencePolicyConfiguration<CustomResilientOptions>();

            services.AddSingleton<PolicyRegistration>();

            var sp = services.BuildServiceProvider();

            var registration = sp.GetService<PolicyRegistration>();

            registration.Register();

            var policy = sp.GetRequiredService<IPolicyRegistry<string>>();

            // no policy added at this point to the registry
            Assert.Equal(0, policy.Count);

            var options = sp.GetRequiredService<IOptionsMonitor<CustomResilientOptions>>().Get(DefaultPoliciesKeys.DefaultPolicies);
            Assert.Equal(TimeSpan.FromSeconds(14), options.HttpCircuitBreaker.DurationOfBreak);
            Assert.Equal(6, options.HttpCircuitBreaker.ExceptionsAllowedBeforeBreaking);
            Assert.Equal(1, options.HttpRetry.BackoffPower);
            Assert.Equal(10, options.HttpRetry.Count);
            Assert.Equal(100, options.CustomCount);
            Assert.Equal(DefaultPoliciesKeys.DefaultPolicies, options.PolicyName);
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
            services.AddResiliencePolicyConfiguration<CustomResilientOptions>(policySectionName: "CustomPolicies");

            var sp = services.BuildServiceProvider();
            var policy = sp.GetRequiredService<IPolicyRegistry<string>>();

            // no policy added at this point to the registry
            Assert.Equal(0, policy.Count);
            var options = sp.GetRequiredService<IOptionsMonitor<CustomResilientOptions>>().Get(DefaultPoliciesKeys.DefaultPolicies);

            Assert.Equal(TimeSpan.FromSeconds(14), options.HttpCircuitBreaker.DurationOfBreak);
            Assert.Equal(6, options.HttpCircuitBreaker.ExceptionsAllowedBeforeBreaking);
            Assert.Equal(1, options.HttpRetry.BackoffPower);
            Assert.Equal(10, options.HttpRetry.Count);
            Assert.Equal(100, options.CustomCount);
            Assert.Equal(DefaultPoliciesKeys.DefaultPolicies, options.PolicyName);
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
            services.AddResiliencePolicyConfiguration<CustomResilientOptions>(policyName: "CustomName");

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

            services.AddResiliencePolicyConfiguration(policySectionName: "Policy", policyName: "CustomName");

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

            services.AddResiliencePolicyConfiguration<CustomResilientOptions>(policySectionName: "Policy", policyName: "CustomPolicy");
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

            services.AddResiliencePolicyConfiguration(o =>
            {
                o.HttpCircuitBreaker.ExceptionsAllowedBeforeBreaking = 83;
                o.HttpRetry.BackoffPower = 22;
            });

            var sp = services.BuildServiceProvider();
            var policy = sp.GetRequiredService<IPolicyRegistry<string>>();

            // no policy added at this point to the registry
            Assert.Equal(0, policy.Count);
            var options = sp.GetRequiredService<IOptionsMonitor<HttpPolicyOptions>>().Get(DefaultPoliciesKeys.DefaultPolicies);

            Assert.Equal(TimeSpan.FromSeconds(14), options.HttpCircuitBreaker.DurationOfBreak);
            Assert.Equal(83, options.HttpCircuitBreaker.ExceptionsAllowedBeforeBreaking);
            Assert.Equal(22, options.HttpRetry.BackoffPower);
            Assert.Equal(10, options.HttpRetry.Count);
            Assert.Equal(DefaultPoliciesKeys.DefaultPolicies, options.PolicyName);
        }
    }
}
