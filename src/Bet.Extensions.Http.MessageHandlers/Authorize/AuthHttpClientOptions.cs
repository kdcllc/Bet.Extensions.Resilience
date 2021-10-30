using Bet.Extensions.Resilience.Http.Abstractions.Options;

namespace Bet.Extensions.Http.MessageHandlers.Authorize
{
    public class AuthHttpClientOptions : HttpClientOptions
    {
        /// <summary>
        /// The username to authenticate with.
        /// </summary>
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// The password to authenticate with.
        /// </summary>
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// Authentication Uri i.e. accesstoken?grant_type=client_credentials.
        /// </summary>
        public string RequestUri { get; set; } = string.Empty;
    }
}
