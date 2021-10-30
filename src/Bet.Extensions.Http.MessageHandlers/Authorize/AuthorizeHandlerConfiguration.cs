using Bet.Extensions.Http.MessageHandlers.Serializers;

namespace Bet.Extensions.Http.MessageHandlers.Authorize
{
    public class AuthorizeHandlerConfiguration<THttpClientOptions, TResponse>
        where THttpClientOptions : AuthHttpClientOptions
        where TResponse : new()
    {
        public AuthorizeHandlerConfiguration(
            Func<THttpClientOptions, HttpRequestMessage> configureAuthorizationMessage,
            Func<TResponse, (string token, DateTimeOffset? expiration)> configureToken)
        {
            ConfigureAuthorizationMessage = configureAuthorizationMessage ?? throw new ArgumentNullException(nameof(ConfigureAuthorizationMessage));
            ConfigureAccessToken = configureToken ?? throw new ArgumentNullException(nameof(configureToken));
        }

        /// <summary>
        /// Configure the authorization token.
        /// </summary>
        public Func<TResponse, (string token, DateTimeOffset? expiration)> ConfigureAccessToken { get; }

        /// <summary>
        /// Configuration of the Authorization request when authorization token is not created or expired.
        /// </summary>
        public Func<THttpClientOptions, HttpRequestMessage> ConfigureAuthorizationMessage { get; }

        public IJsonSerializer JsonSerializer { get; set; } = new SystemTextJsonSerializer();
    }
}
