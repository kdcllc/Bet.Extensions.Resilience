using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Bet.Extensions.Resilience.Abstractions;
using Bet.Extensions.Resilience.Abstractions.Options;
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

        [Fact]
        public void Should_Register_Default_Policies_With_Default_Options()
        {
            var services = new ServiceCollection();

            var policyName = PolicyName.DefaultPolicy;
            var policyOptionsName = PolicyName.DefaultPolicy;

            var setupOptions = new Dictionary<string, string>
            {
                { $"{policyName}:Timeout", "100" },
                { $"{policyName}:HttpRetry:BackOffPower", "1" },
                { $"{policyName}:HttpRetry:Count", "10" },
                { $"{policyName}:HttpCircuitBreaker:DurationOfBreak", "00:00:14" },
                { $"{policyName}:HttpCircuitBreaker:ExceptionsAllowedBeforeBreaking", "6" }
            };

            var config = new ConfigurationBuilder().AddInMemoryCollection(setupOptions).Build();

            services.AddOptions();

            services.AddSingleton<IConfiguration>(config);

            services.AddLogging(builder => builder.AddXunit(_output));

            services.AddHttpDefaultResiliencePolicies<PolicyOptions>();

            var sp = services.BuildServiceProvider();

            // simulates the hosting service registration
            var registration = sp.GetService<IPolicyRegistrator>();
            registration.ConfigurePolicies();

            var po = sp.GetRequiredService<IOptionsMonitor<PolicyOptions>>().Get(policyOptionsName);

            Assert.Equal(policyOptionsName, po.Name);

            var policy = sp.GetRequiredService<IPolicyRegistry<string>>();

            // no policy added at this point to the registry
            Assert.Equal(6, policy.Count);

            Assert.True(policy.ContainsKey(PolicyName.TimeoutPolicy));
            Assert.True(policy.ContainsKey(PolicyName.TimeoutPolicyAsync));

            Assert.True(policy.ContainsKey(PolicyName.RetryPolicy));
            Assert.True(policy.ContainsKey(PolicyName.RetryPolicyAsync));

            Assert.True(policy.ContainsKey(PolicyName.CircuitBreakerPolicy));
            Assert.True(policy.ContainsKey(PolicyName.CircuitBreakerPolicyAsync));

            var options = sp.GetRequiredService<IPolicyConfigurator<HttpResponseMessage, PolicyOptions>>();

            Assert.Equal(7, options.OptionsCollection.Count);
            Assert.Equal(3, options.AsyncPolicyCollection.Count);
            Assert.Equal(3, options.SyncPolicyCollection.Count);

            var individualPolicies = sp.GetServices<IPolicyCreator<HttpResponseMessage, PolicyOptions>>();

            Assert.Equal(3, individualPolicies.Count());
        }
    }
}
