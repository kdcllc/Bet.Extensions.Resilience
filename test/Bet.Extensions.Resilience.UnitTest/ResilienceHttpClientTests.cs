using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Bet.Extensions.Resilience.Http.MessageHandlers;
using Bet.Extensions.Resilience.Http.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly.Registry;

using Xunit;
using Xunit.Abstractions;

namespace Bet.Extensions.Resilience.UnitTest
{
    public class ResilienceHttpClientTests
    {
        public ITestOutputHelper Output { get; }

        public ResilienceHttpClientTests(ITestOutputHelper output)
        {
            Output = output;
        }

        [Fact]
        public void Test_ResilienceHttpClientBuilder()
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
        public void Test_AddResilienceTypedClient_ConfigureAll()
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

            var client = serviceCollection.AddResilienceTypedClient<ITestTypedClient, TestTypedClient>()
                .AddAllConfigurations(options =>
                {
                    options.HttpClientActions.Add((sp, client) => client.Timeout = TimeSpan.FromSeconds(5) );
                })
                .AddAllConfigurations(options =>
                {
                    options.HttpClientActions.Add((sp, client) => client.Timeout = TimeSpan.FromSeconds(15));
                });

            Assert.Equal(nameof(ITestTypedClient), client.Name);

            var services = serviceCollection.BuildServiceProvider();
            var factory = services.GetRequiredService<IHttpMessageHandlerFactory>();
            // Act2
            var handler = factory.CreateHandler();
            // Assert
            Assert.NotNull(handler);
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
                .AddPrimaryHttpMessageHandler((sp) =>
                {
                   return new DefaultHttpClientHandler();
                });

            var services = serviceCollection.BuildServiceProvider();

            var client = services.GetRequiredService<ITestTypedClient>();

            var options = services.GetRequiredService<IOptionsMonitor<HttpClientOptions>>();

            var value = options.Get(nameof(TestTypedClient));

            Assert.NotNull(value);
        }

        [Fact]
        public void Test_AddClientTyped_WithOptions()
        {
            // Assign
            var serviceCollection = new ServiceCollection();

            var id = Guid.NewGuid().ToString();

            var dic1 = new Dictionary<string, string>()
            {
                {"TestHttpClientOptions:BaseAddress", "http://localhost"},
                {"TestHttpClientOptions:Timeout", "00:05:00"},
                {"TestHttpClientOptions:ContentType", "application/json"},
                {"TestHttpClientOptions:Id", id}
            };

            var configurationBuilder = new ConfigurationBuilder().AddInMemoryCollection(dic1);

            serviceCollection.AddSingleton<IConfiguration>(configurationBuilder.Build());

            var clientBuilder = serviceCollection.AddResilienceTypedClient<ITestTypedClientWithOptions, TestTypedClientWithOptions, TestHttpClientOptions>()
                .AddPrimaryHttpMessageHandler((sp) =>
                {
                    return new DefaultHttpClientHandler();
                });

            var services = serviceCollection.BuildServiceProvider();

            var client = services.GetRequiredService<ITestTypedClientWithOptions>();

            Assert.Equal(id, client.Id);
        }

        [Fact]
        public async Task Test_AddClientTyped_WithOptions_And_Default_Policies()
        {
            // Assign
            var serviceCollection = new ServiceCollection();

            var id = Guid.NewGuid().ToString();

            var dic1 = new Dictionary<string, string>()
            {
                {"TestHttpClient:BaseAddress", "http://testserver:5000"},
                {"TestHttpClient:Timeout", "00:05:00"},
                {"TestHttpClient:ContentType", "application/json"},
                {"TestHttpClient:Id", id}
            };

            var configurationBuilder = new ConfigurationBuilder().AddInMemoryCollection(dic1);

            serviceCollection.AddLogging(builder => {
                builder.AddProvider(new TestLoggerProvider(Output));
            });

            serviceCollection.AddSingleton<IConfiguration>(configurationBuilder.Build());

            var server = new TestServerBuilder(Output).GetSimpleServer();
            var handler = server.CreateHandler();

            var clientBuilder = serviceCollection
                .AddResilienceTypedClient<ITestTypedClientWithOptions, TestTypedClientWithOptions, TestHttpClientOptions>("TestHttpClient")
                .AddPrimaryHttpMessageHandler((sp) =>
                {
                    return handler;
                })
                .AddDefaultPolicies(enableLogging:true);

            var services = serviceCollection.BuildServiceProvider();

            var client = services.GetRequiredService<ITestTypedClientWithOptions>();

            var result = await client.SendRequestAsync();

            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        }

        [Fact]
        public void Test_AddClientTyped_OnlyOnePrimaryHandler()
        {
            // Assign
            var serviceCollection = new ServiceCollection();

            serviceCollection.AddLogging(builder => {
                builder.AddProvider(new TestLoggerProvider(Output));
            });

            var server = new TestServerBuilder(Output).GetSimpleServer();
            var handler = server.CreateHandler();

            Assert.Throws<InvalidOperationException>(() => serviceCollection
                .AddResilienceTypedClient<ITestTypedClient, TestTypedClient>()
                .AddPrimaryHttpMessageHandler((sp) =>
                {
                    return new DefaultHttpClientHandler();
                })

                .AddPrimaryHttpMessageHandler((sp) =>
                {
                    return new DefaultHttpClientHandler();
                }));

        }
    }
}
