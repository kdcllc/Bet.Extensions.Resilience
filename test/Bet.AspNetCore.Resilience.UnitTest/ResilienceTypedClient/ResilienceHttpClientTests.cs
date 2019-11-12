using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

using Bet.AspNetCore.Resilience.UnitTest.ResilienceTypedClient.Clients;
using Bet.Extensions.Http.MessageHandlers;
using Bet.Extensions.Http.MessageHandlers.Abstractions.Options;
using Bet.Extensions.Resilience.Http.Policies;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Polly.Registry;

using Xunit;
using Xunit.Abstractions;

namespace Bet.AspNetCore.Resilience.UnitTest.ResilienceTypedClient
{
    public class ResilienceHttpClientTests
    {
        public ResilienceHttpClientTests(ITestOutputHelper output)
        {
            Output = output;
        }

        public ITestOutputHelper Output { get; }

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
                { "TestTypedClient:BaseAddress", "http://localhost" },
                { "TestTypedClient:Timeout", "00:05:00" },
                { "TestTypedClient:ContentType", "application/json" }
            };

            var configurationBuilder = new ConfigurationBuilder().AddInMemoryCollection(dic1);

            serviceCollection.AddSingleton<IConfiguration>(configurationBuilder.Build());

            var client = serviceCollection.AddResilienceTypedClient<ICustomTypedClient, CustomTypedClient>()
                .AddAllConfigurations(options =>
                {
                    options.ConfigureHttpClient.Add((sp, cl) => cl.Timeout = TimeSpan.FromSeconds(5));
                })
                .AddAllConfigurations(options =>
                {
                    options.ConfigureHttpClient.Add((sp, cl) => cl.Timeout = TimeSpan.FromSeconds(15));
                });

            Assert.Equal(nameof(ICustomTypedClient), client.Name);

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
                { "TestTypedClient:BaseAddress", "http://localhost" },
                { "TestTypedClient:Timeout", "00:05:00" },
                { "TestTypedClient:ContentType", "application/json" }
            };

            var configurationBuilder = new ConfigurationBuilder().AddInMemoryCollection(dic1);

            serviceCollection.AddSingleton<IConfiguration>(configurationBuilder.Build());

            var client = serviceCollection.AddResilienceTypedClient<ICustomTypedClient, CustomTypedClient>();

            Assert.Equal(nameof(ICustomTypedClient), client.Name);

            var services = serviceCollection.BuildServiceProvider();
            var factory = services.GetRequiredService<IHttpMessageHandlerFactory>();

            // Act2
            var handler = factory.CreateHandler();

            // Assert
            Assert.NotNull(handler);
        }

        [Fact]
        public void Test_AddResilienceTypedClientDefault_With_Custom_SectionName()
        {
            // Assign
            var serviceCollection = new ServiceCollection();

            var dic1 = new Dictionary<string, string>()
            {
                { "Clients:CustomTypedClient:BaseAddress", "http://localhost" },
                { "Clients:CustomTypedClient:Timeout", "00:05:00" },
                { "Clients:CustomTypedClient:ContentType", "application/json" }
            };

            var configurationBuilder = new ConfigurationBuilder().AddInMemoryCollection(dic1);

            serviceCollection.AddSingleton(_ => configurationBuilder.Build() as IConfiguration);

            var clientBuilder = serviceCollection.AddResilienceTypedClient<ICustomTypedClient, CustomTypedClient>(sectionName: "Clients")
                 .AddPrimaryHttpMessageHandler((sp) =>
                 {
                     return new DefaultHttpClientHandler();
                 })
                .AddAllConfigurations(opt =>
                {
                    opt.ConfigureHttpClient.Add((sp, cl) =>
                    {
                            // checks if the options have been already configured via configuration.
                            cl.BaseAddress = opt.Options.BaseAddress;
                    });
                });

            var services = serviceCollection.BuildServiceProvider();

            var client = services.GetRequiredService<ICustomTypedClient>();

            Assert.Equal("http://localhost/", client.HttpClient.BaseAddress.ToString());

            var options = services.GetRequiredService<IOptionsMonitor<HttpClientOptions>>();

            var value = options.Get(nameof(CustomTypedClient));

            Assert.NotNull(value);
        }

        [Fact]
        public void Test_AddResilienceTypedClientDefault_With_Custom_SectionName_And_OptionName()
        {
            // Assign
            var serviceCollection = new ServiceCollection();

            var dic1 = new Dictionary<string, string>()
            {
                { "Clients:TestTypedClient2:BaseAddress", "http://localhost" },
                { "Clients:TestTypedClient2:Timeout", "00:05:00" },
                { "Clients:TestTypedClient2:ContentType", "application/json" }
            };

            var configurationBuilder = new ConfigurationBuilder().AddInMemoryCollection(dic1);

            serviceCollection.AddSingleton(_ => configurationBuilder.Build() as IConfiguration);

            var clientBuilder = serviceCollection.AddResilienceTypedClient<ICustomTypedClient, CustomTypedClient>(sectionName: "Clients", "TestTypedClient2")
                 .AddPrimaryHttpMessageHandler((sp) =>
                 {
                     return new DefaultHttpClientHandler();
                 })
                .AddAllConfigurations(opt =>
                {
                    opt.ConfigureHttpClient.Add((sp, cl) =>
                    {
                        // checks if the options have been already configured via configuration.
                        cl.BaseAddress = opt.Options.BaseAddress;
                    });
                });

            var services = serviceCollection.BuildServiceProvider();

            var client = services.GetRequiredService<ICustomTypedClient>();

            Assert.Equal("http://localhost/", client.HttpClient.BaseAddress.ToString());

            var options = services.GetRequiredService<IOptionsMonitor<HttpClientOptions>>().Get(nameof(CustomTypedClient));
            Assert.NotNull(options?.BaseAddress);
        }

        [Fact]
        public void Test_AddResilienceTypedClientDefault()
        {
            // Assign
            var serviceCollection = new ServiceCollection();

            var dic1 = new Dictionary<string, string>()
            {
                { "TestTypedClient:BaseAddress", "http://localhost" },
                { "TestTypedClient:Timeout", "00:05:00" },
                { "TestTypedClient:ContentType", "application/json" }
            };

            var configurationBuilder = new ConfigurationBuilder().AddInMemoryCollection(dic1);

            serviceCollection.AddSingleton<IConfiguration>(configurationBuilder.Build());

            var clientBuilder = serviceCollection.AddResilienceTypedClient<ICustomTypedClient, CustomTypedClient>()
                .AddPrimaryHttpMessageHandler((sp) =>
                {
                    return new DefaultHttpClientHandler();
                });

            var services = serviceCollection.BuildServiceProvider();

            var client = services.GetRequiredService<ICustomTypedClient>();

            var options = services.GetRequiredService<IOptionsMonitor<HttpClientOptions>>();

            var value = options.Get(nameof(CustomTypedClient));

            Assert.NotNull(value);
        }

        [Fact]
        public void Test_AddClientTypedCustomOptions()
        {
            // Assign
            var serviceCollection = new ServiceCollection();

            var id = Guid.NewGuid().ToString();

            var dic1 = new Dictionary<string, string>()
            {
                { "CustomHttpClientOptions:BaseAddress", "http://localhost" },
                { "CustomHttpClientOptions:Timeout", "00:05:00" },
                { "CustomHttpClientOptions:ContentType", "application/json" },
                { "CustomHttpClientOptions:Id", id }
            };

            var configurationBuilder = new ConfigurationBuilder().AddInMemoryCollection(dic1);

            serviceCollection.AddSingleton(_ => configurationBuilder.Build() as IConfiguration);

            var clientBuilder = serviceCollection.AddResilienceTypedClient<ICustomTypedClientWithOptions, CustomTypedClientWithOptions, CustomHttpClientOptions>()
               .AddPrimaryHttpMessageHandler((sp) =>
               {
                   return new DefaultHttpClientHandler();
               })
                .AddAllConfigurations(opt =>
                {
                    opt.ConfigureHttpClient.Add((sp, cl) =>
                    {
                        // checks if the options have been already configured via configuration.
                        cl.BaseAddress = opt.Options.BaseAddress;
                    });
                });

            var services = serviceCollection.BuildServiceProvider();

            var options = services.GetRequiredService<IOptionsMonitor<HttpClientOptions>>().Get(nameof(CustomTypedClientWithOptions));
            Assert.NotNull(options?.BaseAddress);

            var client = services.GetRequiredService<ICustomTypedClientWithOptions>();
            Assert.Equal(id, client.Id);

            var clientOptions = services.GetRequiredService<IOptions<CustomHttpClientOptions>>().Value;
            Assert.NotNull(clientOptions?.BaseAddress);
            Assert.Equal(id, clientOptions.Id);
        }

        [Fact]
        public async Task Test_AddClientTyped_WithOptions_And_Default_Policies()
        {
            // Assign
            var services = new ServiceCollection();

            var id = Guid.NewGuid().ToString();

            var dic1 = new Dictionary<string, string>()
            {
                { "TestHttpClient:BaseAddress", "http://testserver:5000" },
                { "TestHttpClient:Timeout", "00:05:00" },
                { "TestHttpClient:ContentType", "application/json" },
                { "TestHttpClient:Id", id }
            };

            var configurationBuilder = new ConfigurationBuilder().AddInMemoryCollection(dic1);

            services.AddLogging(builder =>
            {
                builder.AddProvider(new XunitLoggerProvider(Output));
            });

            services.AddSingleton<IConfiguration>(configurationBuilder.Build());

            using (var server = new ResilienceTypedClientTestServerBuilder(Output).GetSimpleServer())
            {
                var handler = server.CreateHandler();

                var clientBuilder = services
                    .AddResilienceTypedClient<ICustomTypedClientWithOptions, CustomTypedClientWithOptions, CustomHttpClientOptions>(optionsName: "TestHttpClient")
                    .AddPrimaryHttpMessageHandler((sp) => handler)
                    .AddDefaultPolicies(enableLogging: true);

                var provider = services.BuildServiceProvider();

                // simulates registrations for the policies.
                var registration = provider.GetService<IHttpPolicyRegistrator>();
                registration.ConfigurePolicies();

                var client = provider.GetRequiredService<ICustomTypedClientWithOptions>();

                var result = await client.SendRequestAsync();

                Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            }
        }

        [Fact]
        public void Test_AddClientTyped_OnlyOnePrimaryHandler()
        {
            // Assign
            var serviceCollection = new ServiceCollection();

            serviceCollection.AddLogging(builder =>
            {
                builder.AddProvider(new XunitLoggerProvider(Output));
            });

            using (var server = new ResilienceTypedClientTestServerBuilder(Output).GetSimpleServer())
            {
                var handler = server.CreateHandler();

                Assert.Throws<InvalidOperationException>(() => serviceCollection
                    .AddResilienceTypedClient<ICustomTypedClient, CustomTypedClient>()
                    .AddPrimaryHttpMessageHandler((sp) => new DefaultHttpClientHandler())
                    .AddPrimaryHttpMessageHandler((sp) => new DefaultHttpClientHandler()));
            }
        }
    }
}
