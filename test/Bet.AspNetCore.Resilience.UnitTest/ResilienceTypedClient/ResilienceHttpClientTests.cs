using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

using Bet.AspNetCore.Resilience.UnitTest.ResilienceTypedClient.Clients;
using Bet.Extensions.Http.MessageHandlers;
using Bet.Extensions.Resilience.Abstractions.DependencyInjection;
using Bet.Extensions.Testing.Logging;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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
                { "TestHttpClient:Timeout", "00:05:00" },
                { "TestHttpClient:ContentType", "application/json" },
                { "TestHttpClient:Id", id }
            };

            var configurationBuilder = new ConfigurationBuilder().AddInMemoryCollection(dic1);

            services.AddLogging(builder =>
            {
                builder.AddXunit(Output);
            });

            services.AddSingleton<IConfiguration>(configurationBuilder.Build());

            using var server = new ResilienceTypedClientTestServerBuilder(Output).GetSimpleServer();
            var handler = server.CreateHandler();

            var clientBuilder = services
                .AddResilienceHttpClient<ICustomTypedClientWithOptions, CustomTypedClientWithOptions>()
                .ConfigureHttpClientOptions<CustomHttpClientOptions>(
                    optionsSectionName: "TestHttpClient",
                    configureAction: (op) => op.BaseAddress = server.BaseAddress)
                .ConfigurePrimaryHandler((sp) => handler)
                .ConfigureDefaultPolicies();

            services.AddHttpDefaultResiliencePolicies();

            var provider = services.BuildServiceProvider();

            // simulates registrations for the policies.
            var registration = provider.GetRequiredService<PolicyBucketConfigurator>();
            registration.Register();

            var client = provider.GetRequiredService<ICustomTypedClientWithOptions>();

            var result = await client.SendRequestAsync();

            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        }

        [Fact]
        public void Should_Throw_InvalideOptionException_When_More_Than_One_PrimaryHandler_Added()
        {
            // Assign
            var serviceCollection = new ServiceCollection();

            serviceCollection.AddLogging(builder =>
            {
                builder.AddXunit(Output);
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
