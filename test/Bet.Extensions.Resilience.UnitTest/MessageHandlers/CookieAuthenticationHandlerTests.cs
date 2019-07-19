using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Bet.Extensions.MessageHandlers.CookieAuthentication;
using System.Net.Http;

namespace Bet.Extensions.Resilience.UnitTest.MessageHandlers
{
    public class CookieAuthenticationHandlerTests
    {
        public CookieAuthenticationHandlerTests(Xunit.Abstractions.ITestOutputHelper output)
        {
            Output = output;
        }

        public ITestOutputHelper Output { get; }

        public async Task Authenticate_Successful()
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
