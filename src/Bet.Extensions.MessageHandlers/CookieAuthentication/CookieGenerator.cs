using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace Bet.Extensions.MessageHandlers.CookieAuthentication
{
    internal class CookieGenerator
    {
        private readonly CookieGeneratorOptions _options;
        private readonly Func<HttpClient> _createHttpClient;

        public CookieGenerator(CookieGeneratorOptions options, Func<HttpClient> createHttpClient)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _createHttpClient = createHttpClient ?? throw new ArgumentNullException(nameof(createHttpClient));
        }

        public CookieGenerator(CookieGeneratorOptions options) : this(options, () => new HttpClient())
        {
        }

        public async Task<IEnumerable<string>> GetCookies(CancellationToken cancellationToken)
        {
            using (var client = _createHttpClient())
            {
                client.DefaultRequestHeaders.Add("accept", _options.HttpOptions?.ContentType ?? "application/json");
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(_options.HttpOptions?.ContentType ?? "application/json"));

                var request = _options.AuthenticationRequest(_options.HttpOptions);

                var response = await client.SendAsync(request, cancellationToken);

                if (cancellationToken.IsCancellationRequested)
                {
                    return null;
                }

                if (response.IsSuccessStatusCode
                    && response.Headers.TryGetValues("Set-Cookie", out var responseHeaderCookies))
                {
                    return responseHeaderCookies;
                }
                else
                {
                    var httpCode = response.StatusCode;
                    var message = await response.Content.ReadAsStringAsync();

                    if (_options.OnError != null)
                    {
                        _options.OnError(httpCode, message);
                    }
                    else
                    {
                        throw new HttpException(httpCode, message);
                    }
                }

                return null;
            }
        }
    }
}
