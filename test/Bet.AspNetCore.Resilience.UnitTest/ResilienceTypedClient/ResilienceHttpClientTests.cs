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
                    .AddResilienceHttpClient<ICustomTypedClientWithOptions, CustomTypedClientWithOptions>()
                    .ConfigureHttpClientOptions<CustomHttpClientOptions>("TestHttpClient")
                    .ConfigurePrimaryHandler((sp) => handler)
                    .ConfigureDefaultPolicies();

                services.AddHttpDefaultResiliencePolicies();

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
                    .AddResilienceHttpClient<ICustomTypedClient, CustomTypedClient>()
                    .ConfigurePrimaryHandler((sp) => new DefaultHttpClientHandler())
                    .ConfigurePrimaryHandler((sp) => new DefaultHttpClientHandler()));
            }
        }
    }
}
