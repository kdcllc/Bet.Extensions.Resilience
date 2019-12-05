using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

using Bet.Extensions.Testing.Logging;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Polly;
using Polly.Registry;

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
            var policyOptionsName = HttpTimeoutPolicyOptions.DefaultName;

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

            services.AddHttpResiliencePolicy<IHttpTimeoutPolicy, HttpTimeoutPolicy, HttpTimeoutPolicyOptions>(
                policyOptionsName,
                policyOptionsName);

            var sp = services.BuildServiceProvider();

            var policy = sp.GetRequiredService<IHttpTimeoutPolicy>();
            Assert.NotNull(policy);

            async Task<HttpResponseMessage> TimedOutTask()
            {
                return await policy.GetAsyncPolicy().ExecuteAsync(async () =>
                {
                    await Task.Delay(TimeSpan.FromSeconds(2));
                    return await Task.FromResult(new HttpResponseMessage());
                });
            }

            await Assert.ThrowsAsync<Polly.Timeout.TimeoutRejectedException>(async () => await TimedOutTask());
        }

        [Fact]
        public async Task HttpTimeoutPolicy_With_Result_Async_Should_Succeeded()
        {
            var policyOptionsName = HttpTimeoutPolicyOptions.DefaultName;

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

            services.AddHttpResiliencePolicy<IHttpTimeoutPolicy, HttpTimeoutPolicy, HttpTimeoutPolicyOptions>(
                policyOptionsName,
                policyOptionsName);

            var sp = services.BuildServiceProvider();

            var policy = sp.GetRequiredService<IHttpTimeoutPolicy>();
            Assert.NotNull(policy);

            var result = await policy.GetAsyncPolicy().ExecuteAsync(async () =>
            {
                await Task.Delay(TimeSpan.FromSeconds(1));
                return await Task.FromResult(new HttpResponseMessage(HttpStatusCode.BadRequest));
            });

            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Fact]
        public async Task HttpTimeoutPolicy_Registry_With_Result_Async_Should_Succeeded()
        {
            var policyOptionsName = HttpTimeoutPolicyOptions.DefaultName;

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

            services.AddHttpResiliencePolicy<IHttpTimeoutPolicy, HttpTimeoutPolicy, HttpTimeoutPolicyOptions>(
                policyOptionsName,
                policyOptionsName);

            var sp = services.BuildServiceProvider();

            // register policy with global registry
            var registar = sp.GetRequiredService<IPolicyRegistrator>();
            Assert.NotNull(registar);
            registar.ConfigurePolicies();

            var registry = sp.GetRequiredService<IReadOnlyPolicyRegistry<string>>();

            var policy = registry.Get<IAsyncPolicy<HttpResponseMessage>>($"{policyOptionsName}Async");
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
