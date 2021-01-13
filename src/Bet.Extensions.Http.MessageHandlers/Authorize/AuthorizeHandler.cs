using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using Newtonsoft.Json;

namespace Bet.Extensions.Http.MessageHandlers.Authorize
{
    public class AuthorizeHandler<THttpClientOptions, TReponse> : DelegatingHandler
        where THttpClientOptions : AuthHttpClientOptions
        where TReponse : new()
    {
        private readonly THttpClientOptions _httpClientOptions;
        private readonly AuthorizeHandlerConfiguration<TReponse> _handlerConfiguration;
        private readonly AuthType _authType;
        private readonly ILogger<AuthorizeHandler<THttpClientOptions, TReponse>> _logger;
        private readonly SemaphoreSlim _sem = new SemaphoreSlim(1);
        private string _accessToken;
        private DateTimeOffset _expirationTime;

        public AuthorizeHandler(
            THttpClientOptions httpClientOptions,
            AuthorizeHandlerConfiguration<TReponse> handlerConfiguration,
            AuthType authType,
            ILogger<AuthorizeHandler<THttpClientOptions, TReponse>> logger)
        {
            _httpClientOptions = httpClientOptions;
            _handlerConfiguration = handlerConfiguration;
            _authType = authType;
            _logger = logger;
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(_accessToken))
            {
                _logger.LogInformation("{tokenType} Authentication token is null. Attempting to Authenticate.", _authType);
                await _sem.WaitAsync(cancellationToken).ConfigureAwait(false);
                await AuthorizeAsync(request, cancellationToken).ConfigureAwait(false);
            }
            else if (_expirationTime.Subtract(DateTimeOffset.UtcNow) < TimeSpan.FromMinutes(1))
            {
                _logger.LogInformation("{tokenType} Authentication token will expire in less than a minute. Attempting to Authenticate.", _authType);
                await _sem.WaitAsync(cancellationToken).ConfigureAwait(false);

                await AuthorizeAsync(request, cancellationToken).ConfigureAwait(false);
            }

            // adds authorization header
            request.Headers.Remove("Authorization");
            request.Headers.Add("Authorization", $"{_authType} {_accessToken}");

            var response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);

            // test for 403 and actual bearer token in initial request
            var wasTokenInvalid = response.StatusCode == HttpStatusCode.Unauthorized
                && request.Headers.Where(header => header.Key == "Authorization")
                  .Select(header => header.Value)
                  .Any(c => c.Any(val => val.StartsWith(_authType.ToString(), StringComparison.InvariantCultureIgnoreCase)));

            if (wasTokenInvalid)
            {
                // Should only get here if the expiration time we have is somehow out of sync with the server by
                // more than a minute or the authentication server is returning invalid tokens
                _logger.LogInformation("The API returned 401 using the {tokenType} Authentication token. Attempting to Authenticate.", _authType);

                // going to request refresh token: enter or start wait
                await _sem.WaitAsync(cancellationToken).ConfigureAwait(false);

                // retry do to token request
                await AuthorizeAsync(request, cancellationToken).ConfigureAwait(false);

                // retry actual request with new tokens
                response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
            }

            return response;
        }

        private async Task AuthorizeAsync(HttpRequestMessage request, CancellationToken cancellation)
        {
            try
            {
                var requestMessage = _handlerConfiguration.ConfigureAuthorizationMessage(_httpClientOptions);
                var response = await base.SendAsync(requestMessage, cancellation).ConfigureAwait(false);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError(
                       "{tokenType} Authentication token failed to authenticate with the API at {requestUri}. Response returned was {statusCode}",
                       _authType,
                       requestMessage.RequestUri,
                       response.StatusCode);

                    throw new AuthorizeHandlerException(response.StatusCode, "Failed to authenticate with API. Please check your credentials.");
                }

                var rawResponse = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                var authResponse = JsonConvert.DeserializeObject<TReponse>(rawResponse);

                _logger.LogInformation("{tokenType} Authentication token authenticated successfully. Retrying request.", _authType);

                (_accessToken, _expirationTime) = _handlerConfiguration.ConfigureAccessToken(authResponse);

                request.Headers.Remove("Authorization");
                request.Headers.Add("Authorization", $"{_authType} {_accessToken}");
            }
            finally
            {
                _sem.Release();
            }
        }
    }
}
