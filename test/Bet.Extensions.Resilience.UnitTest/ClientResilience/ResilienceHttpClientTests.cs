using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Bet.Extensions.Http.MessageHandlers;
using Bet.Extensions.Http.MessageHandlers.Abstractions.Options;
using Bet.Extensions.Http.MessageHandlers.Timeout;
using Bet.Extensions.Resilience.UnitTest.ClientResilience.Clients;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Internal;
using Microsoft.Extensions.Options;
using Polly;
using Xunit;

namespace Bet.Extensions.Resilience.UnitTest.ClientResilience
{
    public class ResilienceHttpClientTests
    {
        [Theory]
        [InlineData(null, null)] // default values
        [InlineData("builderName", "optionsName")] // custom values
        public void ResilienceHttpClientBuilder_Should_Have_Set_Name_And_Options(string? name, string? optionsName)
        {
            // assign
            var serviceCollection = new ServiceCollection();

            // act
            var builder = new ResilienceHttpTypedClientBuilder<ITestClient, TestClient>(serviceCollection, name, optionsName);

            // assert
            Assert.Equal(name ?? nameof(ITestClient), builder.Name);
            Assert.Equal(optionsName ?? nameof(TestClient), builder.OptionsName);

            Assert.False(builder.Debug().IsPrimaryHandlerSet);

            Assert.Equal(0, builder.Debug().OptionsCount);
            Assert.Equal(0, builder.Debug().DelegatingHandlerCount);
            Assert.Equal(0, builder.Debug().PolicyCount);
        }

        [Fact]
        public void AddResilienceTypedClient_Returns_Instance()
        {
            // assign
            var serviceCollection = new ServiceCollection();

            var builder = serviceCollection.AddResilienceHttpClient<ITestClient, TestClient>();

            // act
            var services = serviceCollection.BuildServiceProvider();

            // assert
            Assert.Equal(nameof(ITestClient), builder.Name);
            Assert.Equal(nameof(TestClient), builder.OptionsName);

            Assert.NotNull(builder.HttpClientBuilder);

            Assert.False(builder.Debug().IsPrimaryHandlerSet);

            Assert.Equal(0, builder.Debug().OptionsCount);
            Assert.Equal(0, builder.Debug().DelegatingHandlerCount);
            Assert.Equal(0, builder.Debug().PolicyCount);
        }

        [Fact]
        public void AddResilienceTypedClient_Throws_Exception_When_The_Same_Type_Is_Added()
        {
            // assign
            var serviceCollection = new ServiceCollection();

            var builder = serviceCollection.AddResilienceHttpClient<ITestClient, TestClient>();

            Assert.Throws<InvalidOperationException>(() => serviceCollection.AddResilienceHttpClient<ITestClient, TestClient>());
        }

        [Fact]
        public void AddResilienceTypedClient_Configure_With_Default_Configurations()
        {
            // assign
            var serviceCollection = new ServiceCollection();

            var optionsName = TypeNameHelper.GetTypeDisplayName(typeof(TestClient), fullName: false);

            var httpClientOptions = new Dictionary<string, string>()
            {
                { $"{optionsName}:BaseAddress", "http://localhost.me" },
                { $"{optionsName}:Timeout", "00:05:00" },
                { $"{optionsName}:ContentType", "application/json" }
            };

            var configuration = new ConfigurationBuilder()
                                    .AddInMemoryCollection(httpClientOptions)
                                    .Build();

            serviceCollection.AddSingleton(_ => configuration as IConfiguration);

            // specify the value for the configureAction
            var builder = serviceCollection
                    .AddResilienceHttpClient<ITestClient, TestClient>()
                    .ConfigureHttpClientOptions(configureAction: o => o.Timeout = TimeSpan.FromMinutes(1));

            // act
            var services = serviceCollection.BuildServiceProvider();

            // update the value
            configuration.Providers.FirstOrDefault()?.Set($"{optionsName}:ContentType", "application/xml");

            // reload the configurations
            configuration.Reload();

            var client = services.GetRequiredService<ITestClient>();
            Assert.Equal(new Uri("http://localhost.me"), client.HttpClient.BaseAddress);

            var optionsMonitor = services.GetRequiredService<IOptionsMonitor<HttpClientOptions>>();
            var options = optionsMonitor.Get(optionsName);

            Assert.Equal(new Uri("http://localhost.me"), options.BaseAddress);
            Assert.Equal("application/xml", options.ContentType);
            Assert.Equal(TimeSpan.FromMinutes(1), options.Timeout);

            Assert.False(builder.Debug().IsPrimaryHandlerSet);

            Assert.Equal(1, builder.Debug().OptionsCount);
            Assert.Equal(0, builder.Debug().DelegatingHandlerCount);
            Assert.Equal(0, builder.Debug().PolicyCount);
        }

        [Fact]
        public void AddResilienceTypedClient_Configure_With_RootSection_Configurations()
        {
            // assign
            var serviceCollection = new ServiceCollection();

            var optionsName = TypeNameHelper.GetTypeDisplayName(typeof(TestClient), fullName: false);

            var httpClientOptions = new Dictionary<string, string>()
            {
                { $"Client:{optionsName}:BaseAddress", "http://localhost.me" },
                { $"Client:{optionsName}:Timeout", "00:05:00" },
                { $"Client:{optionsName}:ContentType", "application/json" }
            };

            var configuration = new ConfigurationBuilder()
                                    .AddInMemoryCollection(httpClientOptions)
                                    .Build();

            serviceCollection.AddSingleton(_ => configuration as IConfiguration);

            var builder = serviceCollection
                            .AddResilienceHttpClient<ITestClient, TestClient>()
                            .ConfigureHttpClientOptions("Client", o => o.Timeout = TimeSpan.FromMinutes(1))
                            .ConfigurePrimaryHandler();

            // act
            var services = serviceCollection.BuildServiceProvider();

            // update the value
            configuration.Providers.FirstOrDefault()?.Set($"Client:{optionsName}:ContentType", "application/xml");

            // reload the configurations
            configuration.Reload();

            var client = services.GetRequiredService<ITestClient>();
            Assert.Equal(new Uri("http://localhost.me"), client.HttpClient.BaseAddress);

            var optionsMonitor = services.GetRequiredService<IOptionsMonitor<HttpClientOptions>>();
            var options = optionsMonitor.Get(optionsName);

            Assert.Equal(new Uri("http://localhost.me"), options.BaseAddress);
            Assert.Equal("application/xml", options.ContentType);
            Assert.Equal(TimeSpan.FromMinutes(1), options.Timeout);

            Assert.True(builder.Debug().IsPrimaryHandlerSet);

            Assert.Equal(1, builder.Debug().OptionsCount);
            Assert.Equal(0, builder.Debug().DelegatingHandlerCount);
            Assert.Equal(0, builder.Debug().PolicyCount);
        }

        [Fact]
        public void AddResilienceTypedClient_Configure_With_RootSection_And_Client_Configurations()
        {
            // assign
            var serviceCollection = new ServiceCollection();

            var optionsName = "MyClient";

            var httpClientOptions = new Dictionary<string, string>()
            {
                { $"Client:{optionsName}:BaseAddress", "http://localhost.me" },
                { $"Client:{optionsName}:Timeout", "00:05:00" },
                { $"Client:{optionsName}:ContentType", "application/json" }
            };

            var configuration = new ConfigurationBuilder()
                            .AddInMemoryCollection(httpClientOptions)
                            .Build();

            serviceCollection.AddSingleton(_ => configuration as IConfiguration);

            serviceCollection.AddTransient<TestDelegatingHandler>(sp => new TestDelegatingHandler());

            var noOp = Policy.NoOpAsync().AsAsyncPolicy<HttpResponseMessage>();

            var builder = serviceCollection
                        .AddResilienceHttpClient<ITestClient, TestClient>()
                        .ConfigureHttpClientOptions(optionsName, "Client", o => o.Timeout = TimeSpan.FromMinutes(1))
                        .ConfigurePrimaryHandler()
                        .ConfigureHttpMessageHandler(sp => sp.GetRequiredService<TestDelegatingHandler>())
                        .ConfigurePolicy((sp, ms) => noOp);

            // act
            var services = serviceCollection.BuildServiceProvider();

            // update the value
            configuration.Providers.FirstOrDefault()?.Set($"Client:{optionsName}:ContentType", "application/xml");

            // reload the configurations
            configuration.Reload();

            var client = services.GetRequiredService<ITestClient>();
            Assert.Equal(new Uri("http://localhost.me"), client.HttpClient.BaseAddress);

            var optionsMonitor = services.GetRequiredService<IOptionsMonitor<HttpClientOptions>>();
            var options = optionsMonitor.Get(builder.OptionsName);

            Assert.Equal(new Uri("http://localhost.me"), options.BaseAddress);
            Assert.Equal("application/xml", options.ContentType);
            Assert.Equal(TimeSpan.FromMinutes(1), options.Timeout);

            Assert.True(builder.Debug().IsPrimaryHandlerSet);

            Assert.Equal(1, builder.Debug().OptionsCount);
            Assert.Equal(1, builder.Debug().DelegatingHandlerCount);
            Assert.Equal(1, builder.Debug().PolicyCount);
        }

        [Fact]
        public void AddResilienceTypedClient_Configure_Custom_With_Default_Configurations()
        {
            // assign
            var serviceCollection = new ServiceCollection();

            var optionsName = TypeNameHelper.GetTypeDisplayName(typeof(TestClient), fullName: false);

            var httpClientOptions = new Dictionary<string, string>()
            {
                { $"{optionsName}:BaseAddress", "http://localhost.me" },
                { $"{optionsName}:Timeout", "00:05:00" },
                { $"{optionsName}:ContentType", "application/json" },
                { $"{optionsName}:ExtraValue", "Extra" }
            };

            var configuration = new ConfigurationBuilder()
                            .AddInMemoryCollection(httpClientOptions)
                            .Build();

            serviceCollection.AddSingleton(_ => configuration as IConfiguration);

            var builder = serviceCollection
                    .AddResilienceHttpClient<ITestClient, TestClient>()
                    .ConfigureHttpClientOptions<TestClientOptions>(configureAction: o => o.Timeout = TimeSpan.FromMinutes(1));

            // act
            var services = serviceCollection.BuildServiceProvider();

            // update the value
            configuration.Providers.FirstOrDefault()?.Set($"{optionsName}:ContentType", "application/xml");

            // reload the configurations
            configuration.Reload();

            var client = services.GetRequiredService<ITestClient>();
            Assert.Equal(new Uri("http://localhost.me"), client.HttpClient.BaseAddress);

            var optionsMonitor = services.GetRequiredService<IOptionsMonitor<TestClientOptions>>();
            var options = optionsMonitor.CurrentValue;

            Assert.Equal(new Uri("http://localhost.me"), options.BaseAddress);
            Assert.Equal("application/xml", options.ContentType);
            Assert.Equal(TimeSpan.FromMinutes(1), options.Timeout);
            Assert.Equal("Extra", options.ExtraValue);

            Assert.False(builder.Debug().IsPrimaryHandlerSet);

            Assert.Equal(1, builder.Debug().OptionsCount);
            Assert.Equal(0, builder.Debug().DelegatingHandlerCount);
            Assert.Equal(0, builder.Debug().PolicyCount);
        }

        [Fact]
        public void AddResilienceTypedClient_Configure_Custom_With_OptionName_Configurations()
        {
            // assign
            var serviceCollection = new ServiceCollection();

            var optionsName = "CustomName";

            var httpClientOptions = new Dictionary<string, string>()
            {
                { $"{optionsName}:BaseAddress", "http://localhost.me" },
                { $"{optionsName}:Timeout", "00:05:00" },
                { $"{optionsName}:ContentType", "application/json" },
                { $"{optionsName}:ExtraValue", "Extra" }
            };

            var configuration = new ConfigurationBuilder()
                            .AddInMemoryCollection(httpClientOptions)
                            .Build();

            serviceCollection.AddSingleton(_ => configuration as IConfiguration);

            var builder = serviceCollection
                    .AddResilienceHttpClient<ITestClient, TestClient>()
                    .ConfigureHttpClientOptions<TestClientOptions>(optionsName, null, o => o.Timeout = TimeSpan.FromMinutes(1));

            // act
            var services = serviceCollection.BuildServiceProvider();

            // update the value
            configuration.Providers.FirstOrDefault()?.Set($"{optionsName}:ContentType", "application/xml");

            // reload the configurations
            configuration.Reload();

            var client = services.GetRequiredService<ITestClient>();
            Assert.Equal(new Uri("http://localhost.me"), client.HttpClient.BaseAddress);

            var optionsMonitor = services.GetRequiredService<IOptionsMonitor<TestClientOptions>>();
            var options = optionsMonitor.CurrentValue;

            Assert.Equal(new Uri("http://localhost.me"), options.BaseAddress);
            Assert.Equal("application/xml", options.ContentType);
            Assert.Equal(TimeSpan.FromMinutes(1), options.Timeout);
            Assert.Equal("Extra", options.ExtraValue);

            Assert.False(builder.Debug().IsPrimaryHandlerSet);

            Assert.Equal(1, builder.Debug().OptionsCount);
            Assert.Equal(0, builder.Debug().DelegatingHandlerCount);
            Assert.Equal(0, builder.Debug().PolicyCount);
        }
    }
}
