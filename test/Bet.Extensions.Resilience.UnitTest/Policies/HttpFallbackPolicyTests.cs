using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

using Bet.Extensions.Resilience.Http.Options;
using Bet.Extensions.Testing.Logging;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Polly;

using Xunit;
using Xunit.Abstractions;

namespace Bet.Extensions.Resilience.UnitTest.Policies
{
    public class HttpFallbackPolicyTests
    {
        private readonly ITestOutputHelper _output;

        public HttpFallbackPolicyTests(ITestOutputHelper output)
        {
            _output = output ?? throw new ArgumentNullException(nameof(output));
        }

        [Fact]
        public async Task HttpTimeoutPolicy_With_Result_Async_Should()
        {
            var services = new ServiceCollection();

            // logger is required for policies.
            services.AddLogging(builder =>
            {
                builder.AddXunit(_output);
            });

            var dic = new Dictionary<string, string>
            {
                { $"{HttpTimeoutPolicyOptions.DefaultName}:Timeout", "00:00:01" },
                { $"{HttpFallbackPolicyOptions.DefaultName}:Message", "Hello" },
                { $"{HttpFallbackPolicyOptions.DefaultName}:StatusCode", "400" },
            };

            var config = new ConfigurationBuilder().AddInMemoryCollection(dic).Build();
            services.AddSingleton<IConfiguration>(config);

            services.AddHttpResiliencePolicy<IHttpTimeoutPolicy, HttpTimeoutPolicy, HttpTimeoutPolicyOptions>(
                HttpTimeoutPolicyOptions.DefaultName,
                HttpTimeoutPolicyOptions.DefaultName);

            services.AddHttpResiliencePolicy<IHttpFallbackPolicy, HttpFallbackPolicy, HttpFallbackPolicyOptions>(
                HttpFallbackPolicyOptions.DefaultName,
                HttpFallbackPolicyOptions.DefaultName);

            var sp = services.BuildServiceProvider();

            var fallbackPolicy = sp.GetRequiredService<IHttpFallbackPolicy>().GetAsyncPolicy();
            Assert.NotNull(fallbackPolicy);

            var timeoutPolicy = sp.GetRequiredService<IHttpTimeoutPolicy>().GetAsyncPolicy();
            Assert.NotNull(timeoutPolicy);

            var policy = fallbackPolicy.WrapAsync(timeoutPolicy);

            var result = await policy.ExecuteAsync(async () =>
            {
                await Task.Delay(TimeSpan.FromSeconds(2));
                return await Task.FromResult(new HttpResponseMessage());
            });

            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        }
    }
}
