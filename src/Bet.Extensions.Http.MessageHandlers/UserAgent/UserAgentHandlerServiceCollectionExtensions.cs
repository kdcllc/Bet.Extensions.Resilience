using Bet.Extensions.Http.MessageHandlers.UserAgent;

using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection;

public static class UserAgentHandlerServiceCollectionExtensions
{
    public static IHttpClientBuilder AddUserAgentHander(this IHttpClientBuilder builder)
    {
        builder.Services.TryAddTransient<UserAgentHandler>();

        return builder.AddHttpMessageHandler(sp => sp.GetRequiredService<UserAgentHandler>());
    }
}
