using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

using Bet.Extensions.Resilience.Http.Options;
using Bet.Extensions.Testing.Logging;
using Bet.Extensions.Resilience.Abstractions;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Polly;

using Xunit;
using Xunit.Abstractions;
using Bet.Extensions.Resilience.Abstractions.Options;
using Polly.Timeout;
using Polly.Fallback;

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
                { $"{PolicyOptionsKeys.TimeoutPolicy}:Timeout", "00:00:01" },
                { $"{HttpPolicyOptionsKeys.HttpFallbackPolicy}:Message", "Hello" },
                { $"{HttpPolicyOptionsKeys.HttpFallbackPolicy}:StatusCode", "400" },
            };

            var config = new ConfigurationBuilder().AddInMemoryCollection(dic).Build();
            services.AddSingleton<IConfiguration>(config);

            services.AddPollyPolicy<AsyncTimeoutPolicy<HttpResponseMessage>, TimeoutPolicyOptions>(PolicyOptionsKeys.TimeoutPolicy)
                .ConfigurePolicy(
                PolicyOptionsKeys.TimeoutPolicy,
                policy => PolicyProfileCreators.CreateTimeoutAsync<TimeoutPolicyOptions, HttpResponseMessage>(policy));

            services.AddPollyPolicy<AsyncFallbackPolicy<HttpResponseMessage>, FallbackPolicyOptions>(HttpPolicyOptionsKeys.HttpFallbackPolicy)
                .ConfigurePolicy(
                PolicyOptionsKeys.FallbackPolicy,
                policy => PolicyProfileCreators.CreateFallbackAsync<FallbackPolicyOptions, HttpResponseMessage>(policy, (outcome) => outcome.GetExceptionMessages()));


            var sp = services.BuildServiceProvider();

            var fallbackPolicy = sp.GetRequiredService<PolicyProfile<AsyncFallbackPolicy, FallbackPolicyOptions>>().GetPolicy(HttpPolicyOptionsKeys.HttpFallbackPolicy) as IAsyncPolicy<HttpResponseMessage>;

            Assert.NotNull(fallbackPolicy);

            var timeoutPolicy = sp.GetRequiredService<PolicyProfile<AsyncTimeoutPolicy, TimeoutPolicyOptions>>().GetPolicy(PolicyOptionsKeys.TimeoutPolicy) as IAsyncPolicy<HttpResponseMessage>;
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
