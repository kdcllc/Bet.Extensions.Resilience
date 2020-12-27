using System;
using System.Net.Http;

namespace Bet.Extensions.Http.MessageHandlers.Authorize
{
    public class AuthorizeHandlerConfiguration<TResponse> where TResponse : new()
    {
        public AuthorizeHandlerConfiguration(
            Func<AuthHttpClientOptions, HttpRequestMessage> configureAuthorizationMessage,
            Func<TResponse, (string token, DateTimeOffset expiration)> configureToken)
        {
            ConfigureAuthorizationMessage = configureAuthorizationMessage ?? throw new ArgumentNullException(nameof(ConfigureAuthorizationMessage));
            ConfigureAccessToken = configureToken ?? throw new ArgumentNullException(nameof(configureToken));
        }

        /// <summary>
        /// Configuration of the Authorization request when authorization token is not created or expired.
        /// </summary>
        public Func<AuthHttpClientOptions, HttpRequestMessage> ConfigureAuthorizationMessage { get; }

        /// <summary>
        /// Configure the authorization token.
        /// </summary>
        public Func<TResponse, (string token, DateTimeOffset expiration)> ConfigureAccessToken { get; }
    }
}
