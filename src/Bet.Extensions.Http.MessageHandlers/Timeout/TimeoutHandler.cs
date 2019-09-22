using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Options;

namespace Bet.Extensions.MessageHandlers.Timeout
{
    public sealed class TimeoutHandler : DelegatingHandler
    {
        private readonly bool _ownsHandler;
        private readonly TimeSpan _defaultTimeout;

        public TimeoutHandler(IOptionsMonitor<TimeoutHandlerOptions> optionsMonitor)
        {
            var options = optionsMonitor.CurrentValue;

            InnerHandler = options.InnerHandler ?? new HttpClientHandler();
            _ownsHandler = options.InnerHandler == null;

            _defaultTimeout = options.DefaultTimeout;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            using (var cts = GetCancellationTokenSource(request, cancellationToken))
            {
                try
                {
                    return await base.SendAsync(request, cts?.Token ?? cancellationToken);
                }
                catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
                {
                    throw new TimeoutException();
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

        private CancellationTokenSource GetCancellationTokenSource(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var timeout = request.GetTimeout() ?? _defaultTimeout;
            if (timeout == System.Threading.Timeout.InfiniteTimeSpan)
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
