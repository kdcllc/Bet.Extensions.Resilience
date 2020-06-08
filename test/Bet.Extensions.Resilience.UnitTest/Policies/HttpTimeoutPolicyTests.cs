using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

using Bet.Extensions.Resilience.Abstractions;
using Bet.Extensions.Resilience.Abstractions.DependencyInjection;
using Bet.Extensions.Resilience.Abstractions.Options;
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
    public class HttpTimeoutPolicyTests
    {
        private readonly ITestOutputHelper _output;

        public HttpTimeoutPolicyTests(ITestOutputHelper output)
        {
            _output = output ?? throw new ArgumentNullException(nameof(output));
        }

        [Fact]
        public async Task HttpTimeoutPolicy_With_Result_Async_Should_Throw_TimeoutRejectedException()
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

            services.AddPollyPolicy<AsyncTimeoutPolicy<HttpResponseMessage>, TimeoutPolicyOptions>(PolicyOptionsKeys.TimeoutPolicy)
                   .ConfigurePolicy(
                   policyOptionsName,
                   policy => PolicyShapes.CreateTimeoutAsync<TimeoutPolicyOptions, HttpResponseMessage>(policy));

            var sp = services.BuildServiceProvider();

            var policy = (IAsyncPolicy<HttpResponseMessage>)sp.GetRequiredService<PolicyBucket<AsyncTimeoutPolicy<HttpResponseMessage>, TimeoutPolicyOptions>>()
                                                              .GetPolicy(policyOptionsName);
            Assert.NotNull(policy);

            async Task<HttpResponseMessage> TimedOutTask()
            {
                return await policy.ExecuteAsync(async () =>
                {
                    await Task.Delay(TimeSpan.FromSeconds(2));
                    return await Task.FromResult(new HttpResponseMessage());
                });
            }

            await Assert.ThrowsAsync<TimeoutRejectedException>(async () => await TimedOutTask());
        }

        [Fact]
        public async Task HttpTimeoutPolicy_With_Result_Async_Should_Succeeded()
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

            services.AddPollyPolicy<AsyncTimeoutPolicy<HttpResponseMessage>, TimeoutPolicyOptions>(PolicyOptionsKeys.TimeoutPolicy)
                   .ConfigurePolicy(
                   policyOptionsName,
                   policy => PolicyShapes.CreateTimeoutAsync<TimeoutPolicyOptions, HttpResponseMessage>(policy));

            var sp = services.BuildServiceProvider();

            var policy = (IAsyncPolicy<HttpResponseMessage>)sp.GetRequiredService<PolicyBucket<AsyncTimeoutPolicy<HttpResponseMessage>, TimeoutPolicyOptions>>()
                                                              .GetPolicy(policyOptionsName);
            Assert.NotNull(policy);

            var result = await policy.ExecuteAsync(async () =>
            {
                await Task.Delay(TimeSpan.FromSeconds(1));
                return await Task.FromResult(new HttpResponseMessage(HttpStatusCode.BadRequest));
            });

            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Fact]
        public async Task HttpTimeoutPolicy_Registry_With_Result_Async_Should_Succeeded()
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

            services.AddPollyPolicy<AsyncTimeoutPolicy<HttpResponseMessage>, TimeoutPolicyOptions>(PolicyOptionsKeys.TimeoutPolicy)
                   .ConfigurePolicy(
                   policyOptionsName,
                   policy => PolicyShapes.CreateTimeoutAsync<TimeoutPolicyOptions, HttpResponseMessage>(policy));

            var sp = services.BuildServiceProvider();

            // register policy with global registry
            // simulates registrations for the policies.
            var registration = sp.GetRequiredService<PolicyBucketConfigurator>();
            registration.Register();

            var registry = sp.GetRequiredService<IReadOnlyPolicyRegistry<string>>();

            var policy = registry.Get<IAsyncPolicy<HttpResponseMessage>>(policyOptionsName);
            Assert.NotNull(policy);

            var result = await policy.ExecuteAsync(async () =>
            {
                await Task.Delay(TimeSpan.FromSeconds(1));
                return await Task.FromResult(new HttpResponseMessage(HttpStatusCode.BadRequest));
            });

            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        }
    }
}
