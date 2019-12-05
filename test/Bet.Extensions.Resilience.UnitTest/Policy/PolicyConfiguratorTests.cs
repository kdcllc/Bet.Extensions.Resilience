using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

using Bet.Extensions.Resilience.Abstractions;
using Bet.Extensions.Resilience.Abstractions.Options;
using Bet.Extensions.Resilience.Http;
using Bet.Extensions.Testing.Logging;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using Polly.Registry;

using Xunit;
using Xunit.Abstractions;

namespace Bet.Extensions.Resilience.UnitTest.Policy
{
    public class PolicyConfiguratorTests
    {
        private readonly ITestOutputHelper _output;

        public PolicyConfiguratorTests(ITestOutputHelper output)
        {
            _output = output ?? throw new ArgumentNullException(nameof(output));
        }

        //[Theory]
        //[InlineData(HttpPolicyName.DefaultHttpPolicy, HttpPolicyName.DefaultHttpPolicy, 1, "00:05:00")]
        //[InlineData(PolicyName.DefaultPolicy, "MyPolicy", 1, "00:05:00")]
        //[InlineData("MyPolicyOptions", "MyPolicy", 1, "00:05:00")]
        //public void Should_Configure_Policy_With_Default_Options_Type(
        //    string policyOptionsName,
        //    string policyName,
        //    int backOffPower,
        //    string maxDelay)
        //{
        //    var services = new ServiceCollection();

        //    void Configure(PolicyOptions c)
        //    {
        //        c.Retry.BackoffPower = backOffPower;
        //        c.JitterRetry.MaxDelay = TimeSpan.Parse(maxDelay);
        //    }

        //    var dic = new Dictionary<string, string>
        //    {
        //        { $"{policyOptionsName}:Timeout", "00:03:00" },
        //        { $"{policyOptionsName}:CircuitBreaker:DurationOfBreak", "00:00:14" },
        //        { $"{policyOptionsName}:CircuitBreaker:ExceptionsAllowedBeforeBreaking", "6" },
        //        { $"{policyOptionsName}:Retry:BackOffPower", backOffPower.ToString() },
        //        { $"{policyOptionsName}:Retry:Count", "10" },
        //        { $"{policyOptionsName}:JitterRetry:MaxDelay", maxDelay },
        //        { $"{policyOptionsName}:JitterRetry:MaxRetries", "20" },
        //        { $"{policyOptionsName}:Bulkhead:MaxParallelization", "200" },
        //        { $"{policyOptionsName}:Bulkhead:MaxQueuedItems", "300" },
        //    };

        //    var config = new ConfigurationBuilder().AddInMemoryCollection(dic).Build();
        //    services.AddOptions();
        //    services.AddSingleton<IConfiguration>(config);

        //    services.AddHttpResiliencePolicy(policyOptionsName, policyName, null, null, Configure);

        //    var sp = services.BuildServiceProvider();

        //    var optionsMonitor = sp.GetRequiredService<IOptionsMonitor<PolicyOptions>>().Get(policyName);

        //    // IPolicyConfigurator can return multiple instances.
        //    var allConfigurations = sp.GetServices<IPolicyConfigurator<PolicyOptions, HttpResponseMessage>>();
        //    Assert.Single(allConfigurations);

        //    var configurator = allConfigurations.First(x => x.ParentPolicyName == policyName);

        //    var optionsConfigurator = configurator.GetOptions(policyName);

        //    // Timeout policy options
        //    Assert.Equal(TimeSpan.FromMinutes(3), optionsMonitor.Timeout);
        //    Assert.Equal(optionsMonitor.Timeout, optionsConfigurator.Timeout);

        //    // Retry policy options
        //    Assert.Equal(backOffPower, optionsMonitor.Retry.BackoffPower);
        //    Assert.Equal(optionsMonitor.Retry.BackoffPower, optionsConfigurator.Retry.BackoffPower);

        //    Assert.Equal(10, optionsMonitor.Retry.Count);
        //    Assert.Equal(optionsMonitor.Retry.Count, optionsConfigurator.Retry.Count);

        //    // JitterRetry policy options
        //    Assert.Equal(TimeSpan.Parse(maxDelay), optionsMonitor.JitterRetry.MaxDelay);
        //    Assert.Equal(optionsMonitor.JitterRetry.MaxDelay, optionsConfigurator.JitterRetry.MaxDelay);

        //    Assert.Equal(20, optionsMonitor.JitterRetry.MaxRetries);
        //    Assert.Equal(optionsMonitor.JitterRetry.MaxRetries, optionsConfigurator.JitterRetry.MaxRetries);

        //    // CircuitBreaker policy options
        //    Assert.Equal(TimeSpan.FromSeconds(14), optionsMonitor.CircuitBreaker.DurationOfBreak);
        //    Assert.Equal(optionsMonitor.CircuitBreaker.DurationOfBreak, optionsConfigurator.CircuitBreaker.DurationOfBreak);

        //    Assert.Equal(6, optionsMonitor.CircuitBreaker.ExceptionsAllowedBeforeBreaking);
        //    Assert.Equal(optionsMonitor.CircuitBreaker.ExceptionsAllowedBeforeBreaking, optionsConfigurator.CircuitBreaker.ExceptionsAllowedBeforeBreaking);

        //    // Bulkhead policy options
        //    Assert.Equal(200, optionsMonitor.Bulkhead.MaxParallelization);
        //    Assert.Equal(300, optionsMonitor.Bulkhead.MaxQueuedItems);
        //}

        //[Theory]
        //[InlineData(PolicyName.DefaultPolicy, PolicyName.DefaultPolicy, 1, "00:05:00")]
        //[InlineData(PolicyName.DefaultPolicy, "MyPolicy", 1, "00:05:00")]
        //[InlineData("MyPolicyOptions", "MyPolicy", 1, "00:05:00")]
        //public void Should_Configure_Policy_With_Custom_Options_Type(
        //    string policyOptionsName,
        //    string policyName,
        //    int backOffPower,
        //    string maxDelay)
        //{
        //    var services = new ServiceCollection();

        //    void Configure(PolicyOptions c)
        //    {
        //        c.Retry.BackoffPower = backOffPower;
        //        c.JitterRetry.MaxDelay = TimeSpan.Parse(maxDelay);
        //    }

        //    var dic = new Dictionary<string, string>
        //    {
        //        { $"{policyOptionsName}:Timeout", "00:03:00" },
        //        { $"{policyOptionsName}:CircuitBreaker:DurationOfBreak", "00:00:14" },
        //        { $"{policyOptionsName}:CircuitBreaker:ExceptionsAllowedBeforeBreaking", "6" },
        //        { $"{policyOptionsName}:Retry:BackOffPower", backOffPower.ToString() },
        //        { $"{policyOptionsName}:Retry:Count", "10" },
        //        { $"{policyOptionsName}:JitterRetry:MaxDelay", maxDelay },
        //        { $"{policyOptionsName}:JitterRetry:MaxRetries", "20" },
        //        { $"{policyOptionsName}:Bulkhead:MaxParallelization", "200" },
        //        { $"{policyOptionsName}:Bulkhead:MaxQueuedItems", "300" },
        //    };

        //    var config = new ConfigurationBuilder().AddInMemoryCollection(dic).Build();
        //    services.AddOptions();
        //    services.AddSingleton<IConfiguration>(config);

        //    services.AddHttpResiliencePolicy<TestPolicyOptions>(policyOptionsName, policyName, null, null, Configure);

        //    var sp = services.BuildServiceProvider();

        //    var optionsMonitor = sp.GetRequiredService<IOptionsMonitor<TestPolicyOptions>>().Get(policyName);

        //    // IPolicyConfigurator can return multiple instances.
        //    var allConfigurations = sp.GetServices<IPolicyConfigurator<TestPolicyOptions, HttpResponseMessage>>();
        //    Assert.Single(allConfigurations);

        //    var configurator = allConfigurations.First(x => x.ParentPolicyName == policyName);

        //    var optionsConfigurator = configurator.GetOptions(policyName);

        //    // Timeout policy options
        //    Assert.Equal(TimeSpan.FromMinutes(3), optionsMonitor.Timeout);
        //    Assert.Equal(optionsMonitor.Timeout, optionsConfigurator.Timeout);

        //    // Retry policy options
        //    Assert.Equal(backOffPower, optionsMonitor.Retry.BackoffPower);
        //    Assert.Equal(optionsMonitor.Retry.BackoffPower, optionsConfigurator.Retry.BackoffPower);

        //    Assert.Equal(10, optionsMonitor.Retry.Count);
        //    Assert.Equal(optionsMonitor.Retry.Count, optionsConfigurator.Retry.Count);

        //    // JitterRetry policy options
        //    Assert.Equal(TimeSpan.Parse(maxDelay), optionsMonitor.JitterRetry.MaxDelay);
        //    Assert.Equal(optionsMonitor.JitterRetry.MaxDelay, optionsConfigurator.JitterRetry.MaxDelay);

        //    Assert.Equal(20, optionsMonitor.JitterRetry.MaxRetries);
        //    Assert.Equal(optionsMonitor.JitterRetry.MaxRetries, optionsConfigurator.JitterRetry.MaxRetries);

        //    // CircuitBreaker policy options
        //    Assert.Equal(TimeSpan.FromSeconds(14), optionsMonitor.CircuitBreaker.DurationOfBreak);
        //    Assert.Equal(optionsMonitor.CircuitBreaker.DurationOfBreak, optionsConfigurator.CircuitBreaker.DurationOfBreak);

        //    Assert.Equal(6, optionsMonitor.CircuitBreaker.ExceptionsAllowedBeforeBreaking);
        //    Assert.Equal(optionsMonitor.CircuitBreaker.ExceptionsAllowedBeforeBreaking, optionsConfigurator.CircuitBreaker.ExceptionsAllowedBeforeBreaking);

        //    // Bulkhead policy options
        //    Assert.Equal(200, optionsMonitor.Bulkhead.MaxParallelization);
        //    Assert.Equal(300, optionsMonitor.Bulkhead.MaxQueuedItems);
        //}

        //[Fact]
        //public void Should_Not_Allow_Two_Policy_With_The_Same_Name_Throws_Argument_Exception()
        //{
        //    var services = new ServiceCollection();
        //    var policyOptionsName = PolicyName.DefaultPolicy;

        //    var dic = new Dictionary<string, string>
        //    {
        //        { $"{policyOptionsName}:CircuitBreaker:DurationOfBreak", "00:00:14" },
        //        { $"{policyOptionsName}:CircuitBreaker:ExceptionsAllowedBeforeBreaking", "6" },
        //        { $"{policyOptionsName}:Retry:BackOffPower", "1" },
        //        { $"{policyOptionsName}:Retry:Count", "10" },
        //        { $"{policyOptionsName}:Count", "100" }
        //    };
        //    var config = new ConfigurationBuilder().AddInMemoryCollection(dic).Build();
        //    services.AddOptions();
        //    services.AddSingleton<IConfiguration>(config);

        //    services.AddHttpResiliencePolicy();

        //    Assert.Throws<ArgumentException>(() => services.AddHttpResiliencePolicy<TestPolicyOptions>());
        //}

        //[Fact]
        //public void Should_Not_Allow_Two_Default_Policy_With_The_Same_Name()
        //{
        //    var services = new ServiceCollection();

        //    var policyOptionsName = PolicyName.DefaultPolicy;

        //    var dic = new Dictionary<string, string>
        //    {
        //        { $"{policyOptionsName}:CircuitBreaker:DurationOfBreak", "00:00:14" },
        //        { $"{policyOptionsName}:CircuitBreaker:ExceptionsAllowedBeforeBreaking", "6" },
        //        { $"{policyOptionsName}:Retry:BackOffPower", "1" },
        //        { $"{policyOptionsName}:Retry:Count", "10" },
        //        { $"{policyOptionsName}:Count", "100" }
        //    };
        //    var config = new ConfigurationBuilder().AddInMemoryCollection(dic).Build();
        //    services.AddOptions();
        //    services.AddSingleton<IConfiguration>(config);

        //    services.AddHttpDefaultResiliencePolicies();
        //    services.AddHttpDefaultResiliencePolicies<TestPolicyOptions>();

        //    var sp = services.BuildServiceProvider();

        //    var firstOptions = sp.GetRequiredService<IOptionsMonitor<PolicyOptions>>().Get(policyOptionsName);

        //    Assert.Equal(PolicyName.DefaultPolicy, firstOptions.Name);
        //    Assert.Equal(policyOptionsName, firstOptions.OptionsName);

        //    var secondOptions = sp.GetRequiredService<IOptionsMonitor<TestPolicyOptions>>().Get(policyOptionsName);

        //    Assert.Equal(string.Empty, secondOptions.Name);
        //    Assert.Equal(string.Empty, secondOptions.OptionsName);
        //}

        //[Fact]
        //public void Should_Register_Default_Policies_With_Default_Options()
        //{
        //    var services = new ServiceCollection();

        //    // var policyName = PolicyName.DefaultPolicy;
        //    var policyOptionsName = PolicyName.DefaultPolicy;

        //    var dic = new Dictionary<string, string>
        //    {
        //        { $"{policyOptionsName}:Timeout", "00:03:00" },
        //        { $"{policyOptionsName}:CircuitBreaker:DurationOfBreak", "00:00:14" },
        //        { $"{policyOptionsName}:CircuitBreaker:ExceptionsAllowedBeforeBreaking", "6" },
        //        { $"{policyOptionsName}:Retry:BackOffPower", "1" },
        //        { $"{policyOptionsName}:Retry:Count", "10" },
        //        { $"{policyOptionsName}:JitterRetry:MaxDelay", "00:02:00" },
        //        { $"{policyOptionsName}:JitterRetry:MaxRetries", "20" },
        //        { $"{policyOptionsName}:Bulkhead:MaxParallelization", "200" },
        //        { $"{policyOptionsName}:Bulkhead:MaxQueuedItems", "300" },
        //    };

        //    var config = new ConfigurationBuilder().AddInMemoryCollection(dic).Build();

        //    services.AddOptions();

        //    services.AddSingleton<IConfiguration>(config);

        //    services.AddLogging(builder => builder.AddXunit(_output));

        //    services.AddHttpDefaultResiliencePolicies<PolicyOptions>();

        //    var sp = services.BuildServiceProvider();

        //    // simulates the hosting service registration
        //    var registration = sp.GetService<IPolicyRegistrator>();
        //    registration.ConfigurePolicies();

        //    var policyOptions = sp.GetRequiredService<IOptionsMonitor<PolicyOptions>>().Get(policyOptionsName);

        //    Assert.Equal(policyOptionsName, policyOptions.Name);

        //    var policy = sp.GetRequiredService<IPolicyRegistry<string>>();

        //    // policies added to the registry
        //    Assert.Equal(6, policy.Count);

        //    Assert.True(policy.ContainsKey(PolicyName.TimeoutPolicy));
        //    Assert.True(policy.ContainsKey(PolicyName.TimeoutPolicyAsync));

        //    Assert.True(policy.ContainsKey(PolicyName.RetryPolicy));
        //    Assert.True(policy.ContainsKey(PolicyName.RetryPolicyAsync));

        //    Assert.True(policy.ContainsKey(PolicyName.CircuitBreakerPolicy));
        //    Assert.True(policy.ContainsKey(PolicyName.CircuitBreakerPolicyAsync));

        //    var options = sp.GetRequiredService<IPolicyConfigurator<PolicyOptions, HttpResponseMessage>>();

        //    Assert.Equal(7, options.OptionsCollection.Count);
        //    Assert.Equal(3, options.AsyncPolicyCollection.Count);
        //    Assert.Equal(3, options.SyncPolicyCollection.Count);

        //    var defaultPolicies = sp.GetServices<IPolicyCreator<PolicyOptions, HttpResponseMessage>>();

        //    Assert.Equal(3, defaultPolicies.Count());

        //    // Timeout Policy options
        //    var timeoutPolicy = defaultPolicies.First(x => x.Name == PolicyName.TimeoutPolicy);
        //    var timoutPolicyOptions = timeoutPolicy.Options;
        //    Assert.Equal(TimeSpan.FromMinutes(3), timoutPolicyOptions.Timeout);

        //    // CircuitBreaker policy options
        //    var retryPolicy = defaultPolicies.First(x => x.Name == PolicyName.CircuitBreakerPolicy);
        //    var retryPolicyOptions = retryPolicy.Options;

        //    Assert.Equal(TimeSpan.FromSeconds(14), retryPolicyOptions.CircuitBreaker.DurationOfBreak);
        //    Assert.Equal(6, retryPolicyOptions.CircuitBreaker.ExceptionsAllowedBeforeBreaking);

        //    // CircuitBreaker policy options
        //    var circuitBreakerPolicy = defaultPolicies.First(x => x.Name == PolicyName.CircuitBreakerPolicy);
        //    var circuitBreakerPolicyOptions = circuitBreakerPolicy.Options;

        //    Assert.Equal(TimeSpan.FromSeconds(14), circuitBreakerPolicyOptions.CircuitBreaker.DurationOfBreak);
        //    Assert.Equal(6, circuitBreakerPolicyOptions.CircuitBreaker.ExceptionsAllowedBeforeBreaking);
        //}
    }
}
