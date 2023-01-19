using System.Net;

using Bet.Extensions.Resilience.Abstractions;
using Bet.Extensions.Resilience.Abstractions.Options;
using Bet.Extensions.Resilience.Http.Options;
using Bet.Extensions.Testing.Logging;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Polly;
using Polly.Fallback;
using Polly.Timeout;

using Xunit;
using Xunit.Abstractions;

namespace Bet.Extensions.Resilience.UnitTest.Policies;

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
                    sectionName: PolicyOptionsKeys.TimeoutPolicy,
                    (policy) => PolicyShapes.CreateTimeoutAsync<TimeoutPolicyOptions, HttpResponseMessage>(policy));

        services.AddPollyPolicy<AsyncFallbackPolicy<HttpResponseMessage>, HttpFallbackPolicyOptions>(HttpPolicyOptionsKeys.HttpFallbackPolicy)
                .ConfigurePolicy(
                    sectionName: HttpPolicyOptionsKeys.HttpFallbackPolicy,
                    (policy) => policy.HttpCreateFallbackAsync());

        var sp = services.BuildServiceProvider();

        var fallbackPolicy = sp.GetRequiredService<PolicyBucket<AsyncFallbackPolicy<HttpResponseMessage>, HttpFallbackPolicyOptions>>().GetPolicy(HttpPolicyOptionsKeys.HttpFallbackPolicy) as IAsyncPolicy<HttpResponseMessage>;

        Assert.NotNull(fallbackPolicy);

        var timeoutPolicy = sp.GetRequiredService<PolicyBucket<AsyncTimeoutPolicy<HttpResponseMessage>, TimeoutPolicyOptions>>().GetPolicy(PolicyOptionsKeys.TimeoutPolicy) as IAsyncPolicy<HttpResponseMessage>;
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
