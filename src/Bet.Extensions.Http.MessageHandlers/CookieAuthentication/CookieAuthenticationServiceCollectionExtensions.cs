﻿using System;

using Bet.Extensions.Http.MessageHandlers.Timeout;

using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class CookieAuthenticationServiceCollectionExtensions
    {
        public static IServiceCollection AddTimeoutHandler(this IServiceCollection services, Action<TimeoutHandlerOptions> configure = null)
        {
            services.Configure<TimeoutHandlerOptions>(opt => configure?.Invoke(opt));

            services.TryAddTransient<TimeoutHandler>();

            return services;
        }
    }
}