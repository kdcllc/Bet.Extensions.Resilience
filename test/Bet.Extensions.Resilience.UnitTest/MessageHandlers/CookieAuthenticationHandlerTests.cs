using System.Collections.Generic;
using System.Net.Http;

using Bet.Extensions.MessageHandlers.CookieAuthentication;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Xunit.Abstractions;

namespace Bet.Extensions.Resilience.UnitTest.MessageHandlers
{
    public class CookieAuthenticationHandlerTests
    {
        public CookieAuthenticationHandlerTests(Xunit.Abstractions.ITestOutputHelper output)
        {
            Output = output;
        }

        public ITestOutputHelper Output { get; }

        public void Authenticate_Successful()
        {
            // arrange
            var services = new ServiceCollection();

            var dic = new Dictionary<string, string>
            {
                { "ChavahHttpClient:BaseAddress", "https://example" },
                { "ChavahHttpClient:Timeout", "00:01:40" },
                { "ChavahHttpClient:Username", "username" },
                { "ChavahHttpClient:Password", "password" }
            };

            var config = new ConfigurationBuilder().AddInMemoryCollection(dic).Build();
            services.AddSingleton<IConfiguration>(config);

            services.AddLogging(builder => builder.AddProvider(new XunitLoggerProvider(Output)));

            services.AddTransient((sp) =>
            {
                var logger = sp.GetRequiredService<ILogger<CookieAuthenticationHandler>>();
                var options = new CookieAuthenticationHandlerOptions { InnerHandler = new HttpClientHandler { UseCookies = true } };

                return new CookieAuthenticationHandler(options, logger);
            });
        }
    }
}
