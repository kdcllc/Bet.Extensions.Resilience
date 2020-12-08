using System;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Bet.AspNetCore.Resilience.UnitTest.MessageHandlers
{
    internal class TestStartup
    {
        public virtual void ConfigureServices(IServiceCollection services)
        {
        }

        public void Configure(IApplicationBuilder app)
        {
            if (app is null)
            {
                throw new ArgumentNullException(nameof(app));
            }
        }
    }
}
