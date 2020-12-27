using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Bet.Extensions.Http.MessageHandlers.HttpTimeout
{
    /// <summary>
    /// <see cref="HttpClient"/> used to throw a <see cref="TimeoutException"/> when a request times out instead of
    /// an <see cref="TaskCanceledException"/> that it does by default.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <see cref="HttpClient"/> bubbles timeouts up as task cancellation so you can catch that exception but
    /// you won't know if it was cased by the client canceling the request or because of a timeout. See these
    /// links that were used for inspiration in this code:
    /// https://thomaslevesque.com/2018/02/25/better-timeout-handling-with-httpclient/.
    /// https://cm.engineering/transient-timeouts-and-the-retry-rabbit-hole-net-4-5-f406cebbf194.
    /// </para>
    /// <para>
    /// In order to maintain this handler's functionality without colliding with <see cref="HttpClient"/>.Timeout,
    /// we need to configure <see cref="HttpClient"/>.Timeout to a higher value than DefaultTimeout and usually
    /// its easiest to just set it to Timespan.Infinite...otherwise <see cref="HttpClient"/> would timeout first
    /// and bubble up a <see cref="TaskCanceledException"/> instead of <see cref="TimeoutException"/>.
    /// </para>
    /// <para>
    /// When this handler is used along with a Polly timeout policy, be careful to understand how they
    /// interact together. If a Polly timeout policy is used as the outer wrapper policy (not counting fallback
    /// policy) of a policy chain  then it will function mostly the same as <see cref="HttpTimeoutHandler"/>.
    /// Having both in this scenario it is expected that you want the timeout handled by the Polly policy and
    /// <see cref="HttpTimeoutHandler"/> would only be used as a backup to kick in due to misconfiguration or other
    /// unexpected scenario. Beware that in this configuration, the Polly timeout policy's timeout needs to be
    /// at least a few seconds less than the timeout for <see cref="HttpTimeoutHandler"/> since there will be some
    /// overhead time spent inside Polly.
    /// </para>
    /// <para>
    /// Note: If a Polly timeout policy is added inside a Polly retry policy then that timeout will count towards
    /// each try/retry instead of all tries/retries as a whole.
    /// </para>
    /// </remarks>
    public sealed class HttpTimeoutHandler : DelegatingHandler
    {
        private readonly bool _ownsHandler;
        private readonly ILogger<HttpTimeoutHandler> _logger;
        private TimeSpan _defaultTimeout;

        public HttpTimeoutHandler(
            IOptionsMonitor<HttpTimeoutHandlerOptions> optionsMonitor,
            ILogger<HttpTimeoutHandler> logger)
        {
            var options = optionsMonitor.CurrentValue;

            optionsMonitor.OnChange((newValues) =>
            {
                _defaultTimeout = newValues.DefaultTimeout;
            });

            InnerHandler = options.InnerHandler ?? new HttpClientHandler();
            _ownsHandler = options.InnerHandler == null;

            _defaultTimeout = options.DefaultTimeout;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            using (var cts = GetCancellationTokenSource(request, cancellationToken))
            {
                var sw = ValueStopwatch.StartNew();

                try
                {
                    return await base.SendAsync(request, cts?.Token ?? cancellationToken);
                }
                catch (OperationCanceledException ex) when (!cancellationToken.IsCancellationRequested)
                {
                    // getting here we know that the timeout cancellation token was canceled and not the token sent in from the client
                    const string handlerName = nameof(HttpTimeoutHandler);

                    _logger?.LogCritical($"throwing {handlerName} after {sw.GetElapsedTime().TotalMilliseconds}) ms.");

                    throw new TimeoutException($"{handlerName} timed out after {_defaultTimeout}", ex);
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing && _ownsHandler)
            {
                InnerHandler?.Dispose();
            }
        }

        private CancellationTokenSource? GetCancellationTokenSource(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var timeout = request.GetTimeout() ?? _defaultTimeout;
            if (timeout == Timeout.InfiniteTimeSpan)
            {
                // No need to create a CTS if there's no timeout
                return null;
            }
            else
            {
                var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                cts.CancelAfter(timeout);
                return cts;
            }
        }
    }
}
