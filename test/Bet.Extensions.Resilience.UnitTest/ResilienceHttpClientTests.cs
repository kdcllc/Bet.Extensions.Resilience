using System.Collections.Generic;
using System.Net.Http;
using Bet.Extensions.Resilience.Http.MessageHandlers;
using Bet.Extensions.Resilience.Http.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Polly.Registry;

using Xunit;

namespace Bet.Extensions.Resilience.UnitTest
{
    public class ResilienceHttpClientTests
    {
        [Fact]
        public void Test1()
        {
            var serviceCollection = new ServiceCollection();

            var name = "TestName";

            var builder = new ResilienceHttpClientBuilder(serviceCollection, name);

            Assert.Equal(name, builder.Name);
        }

        [Fact]
        public void Test_Policy()
        {
            var serviceCollection = new ServiceCollection();

            var registry = new PolicyRegistry();

            var result = serviceCollection.TryAddPolicyRegistry(registry);

            Assert.Equal(registry, result);

            var result2 = serviceCollection.TryAddPolicyRegistry(registry);

            Assert.Equal(result, result2);
        }

        [Fact]
        public void Test_AddResilienceTypedClient()
        {
            // Assign
            var serviceCollection = new ServiceCollection();

            var dic1 = new Dictionary<string, string>()
            {
                {"TestTypedClient:BaseAddress", "http://localhost"},
                {"TestTypedClient:Timeout", "00:05:00"},
                {"TestTypedClient:ContentType", "application/json"}
            };

            var configurationBuilder = new ConfigurationBuilder().AddInMemoryCollection(dic1);

            serviceCollection.AddSingleton<IConfiguration>(configurationBuilder.Build());

            var client = serviceCollection.AddResilienceTypedClient<ITestTypedClient, TestTypedClient>();

            Assert.Equal(nameof(ITestTypedClient), client.Name);

            var services = serviceCollection.BuildServiceProvider();
            var factory = services.GetRequiredService<IHttpMessageHandlerFactory>();
            // Act2
            var handler = factory.CreateHandler();
            // Assert
            Assert.NotNull(handler);
        }

        [Fact]
        public void Test_AddTyped_Configure_Build_Client()
        {
            // Assign
            var serviceCollection = new ServiceCollection();

            var dic1 = new Dictionary<string, string>()
            {
                {"TestTypedClient:BaseAddress", "http://localhost"},
                {"TestTypedClient:Timeout", "00:05:00"},
                {"TestTypedClient:ContentType", "application/json"}
            };

            var configurationBuilder = new ConfigurationBuilder().AddInMemoryCollection(dic1);

            serviceCollection.AddSingleton<IConfiguration>(configurationBuilder.Build());

            var clientBuilder = serviceCollection.AddResilienceTypedClient<ITestTypedClient, TestTypedClient>()
                .AddPrimaryHandler((sp) =>
                {
                   return new DefaultHttpClientHandler();
                })
                .Build();

            var services = serviceCollection.BuildServiceProvider();

            var client = services.GetRequiredService<ITestTypedClient>();

            var options = services.GetRequiredService<IOptionsMonitor<HttpClientOptions>>();

            var value = options.Get(nameof(TestTypedClient));

            Assert.NotNull(value);
        }
    }
}
