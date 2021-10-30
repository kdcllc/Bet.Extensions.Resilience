using System.Net;

using Microsoft.Extensions.Logging;

namespace Bet.Extensions.Http.MessageHandlers.CookieAuthentication
{
    public sealed class CookieAuthenticationHandler : DelegatingHandler
    {
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
        private readonly ILogger _logger;
        private readonly bool _ownsHandler;
        private readonly CookieGenerator _cookieGenerator;
        private IEnumerable<string>? _cookies;

        public CookieAuthenticationHandler(CookieAuthenticationHandlerOptions options, ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            InnerHandler = options.InnerHandler ?? new HttpClientHandler();
            _ownsHandler = options.InnerHandler == null;

            _cookieGenerator = new CookieGenerator(options.Options, () => new HttpClient(InnerHandler, false));
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing && _ownsHandler)
            {
                InnerHandler?.Dispose();
                _semaphore?.Dispose();
            }
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            try
            {
                // this httpclient never requested cookies.
                if (_cookies == null)
                {
                    var cookie = await GetCookieResponse(cancellationToken);
                    if (!string.IsNullOrEmpty(cookie))
                    {
                        request.Headers.Add("Cookie", cookie);
                    }

                    _logger.LogDebug("{name} created cookie header", nameof(CookieAuthenticationHandler));
                }

                var response = await base.SendAsync(request, cancellationToken);

                if (response.StatusCode != HttpStatusCode.Unauthorized)
                {
                    return response;
                }

                {
                    var cookie = await GetCookieResponse(cancellationToken);
                    if (!string.IsNullOrEmpty(cookie))
                    {
                        request.Headers.Add("Cookie", cookie);

                        response = await base.SendAsync(request, cancellationToken);

                        _logger.LogDebug("{name} created cookie header", nameof(CookieAuthenticationHandler));
                    }
                }

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                throw;
            }
        }

        private async Task<string?> GetCookieResponse(CancellationToken cancellationToken)
        {
            try
            {
                _semaphore.Wait(cancellationToken);
                if (cancellationToken.IsCancellationRequested)
                {
                    return null;
                }

                _cookies = await _cookieGenerator.GetCookies(cancellationToken);
                if (_cookies != null)
                {
                    return _cookies.Flatten(";");
                }

                return null;
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }
}
