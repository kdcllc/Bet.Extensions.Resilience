﻿using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Bet.AspNetCore.Resilience.UnitTest.MessageHandlers;

internal class MessageHandlersStartup
{
    public virtual void ConfigureServices(IServiceCollection services)
    {
    }

    public void Configure(IApplicationBuilder app)
    {
    }
}
