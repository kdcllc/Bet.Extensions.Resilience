using System.Threading.Tasks;

using Bet.Extensions.Http.MessageHandlers.CorrelationId;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

using Xunit;

namespace Bet.AspNetCore.Resilience.UnitTest.Middleware
{
    public class CorrelationIdMiddlewareTests
    {
        [Fact]
        public async Task Should_Return_CorrelationId_By_Default()
        {
            var builder = new WebHostBuilder()
               .Configure(app => app.UseCorrelationId())
               .ConfigureServices(sc => sc.AddCorrelationId());

            var server = new TestServer(builder);

            var response = await server.CreateClient().GetAsync(string.Empty);

            var expectedHeaderName = new CorrelationIdOptions().Header;

            var header = response.Headers.GetValues(expectedHeaderName);

            Assert.NotNull(header);
        }
    }
}
