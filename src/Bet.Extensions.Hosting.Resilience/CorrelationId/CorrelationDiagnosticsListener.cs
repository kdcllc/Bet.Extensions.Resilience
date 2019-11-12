﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using Bet.Extensions.Http.MessageHandlers.CorrelationId;

using Microsoft.Extensions.DiagnosticAdapter;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;

namespace Bet.Extensions.Hosting.Resilience.CorrelationId
{
    /// <summary>
    /// https://github.com/dotnet/corefx/blob/194b2eb174bcf0683ce3146f1286765cdb897f74/src/System.Net.Http/src/HttpDiagnosticsGuide.md.
    /// </summary>
    internal class CorrelationDiagnosticsListener : IHostedService, IObserver<DiagnosticListener>, IDisposable, IObserver<KeyValuePair<string, object>>
    {
        private readonly List<IDisposable> _subscriptions = new List<IDisposable>();
        private readonly CorrelationIdOptions _options;
        private readonly ICorrelationContextFactory _correlationContextFactory;
        private readonly ILogger<DiagnosticListener> _logger;
        private readonly IDisposable _reference;

        public CorrelationDiagnosticsListener(
            ICorrelationContextFactory correlationContextFactory,
            IOptions<CorrelationIdOptions> options,
            ILogger<DiagnosticListener> logger)
        {
            _correlationContextFactory = correlationContextFactory ?? throw new ArgumentNullException(nameof(correlationContextFactory));

            _options = options.Value;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _reference = DiagnosticListener.AllListeners.Subscribe(this);
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            HostingEventSource.Log.HostStop();

            _logger.LogDebug("Started");

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            HostingEventSource.Log.HostStop();

            _logger.LogDebug("Stopped");

            return Task.CompletedTask;
        }

        public void OnCompleted()
        {
        }

        public void OnError(Exception error)
        {
        }

        public void OnNext(DiagnosticListener value)
        {
            _logger.LogInformation(value.Name);

            if (value.Name == "HttpHandlerDiagnosticListener"
                || value.Name == "Microsoft.AspNetCore")
            {
                _subscriptions.Add(value.SubscribeWithAdapter(this));
            }
        }

        public void OnNext(KeyValuePair<string, object> value)
        {
            _logger.LogInformation($"{value.Key} - {value.Value}");
        }

        [DiagnosticName("System.Net.Http.HttpRequestOut")]
        public virtual void IsEnabled()
        {
            // this method is required to be present for the diagnostics to work.
        }

        [DiagnosticName("System.Net.Http.HttpRequestOut.Start")]
        public virtual void OnHttpRequestOutStart(HttpRequestMessage request)
        {
            _logger.LogDebug("System.Net.Http.HttpRequestOut.Start");

            var correlationId = SetCorrelationId(request);

            _correlationContextFactory.Create(correlationId, _options.Header);

            Activity.Current.AddTag(_options.Header, correlationId);

            if (!request.Headers.TryGetValues(_options.Header, out var values))
            {
                request.Headers.Add(_options.Header, new string[] { correlationId });
            }
        }

        [DiagnosticName("System.Net.Http.Request")]
        public virtual void OnRequest(HttpRequestMessage request)
        {
            _logger.LogWarning("System.Net.Http.Request");
        }

        [DiagnosticName("System.Net.Http.HttpRequestOut.Stop")]
        public virtual void OnHttpRequestOutStop(
            HttpRequestMessage request,
            HttpResponseMessage response,
            TaskStatus requestTaskStatus)
        {
            _logger.LogWarning("System.Net.Http.HttpRequestOut.Stop");

            if (_options.IncludeInResponse)
            {
                if (request.Headers.TryGetValues(_options.Header, out var values))
                {
                    response.Headers.Add(_options.Header, values);

                    Activity.Current.AddTag(_options.Header, string.Join(", ", values));
                }
            }
        }

        [DiagnosticName("System.Net.Http.Response")]
        public virtual void OnResponse(HttpResponseMessage response)
        {
            _logger.LogWarning("System.Net.Http.Response");
        }

        public void Dispose()
        {
            _reference?.Dispose();

            foreach (var sub in _subscriptions)
            {
                sub?.Dispose();
            }
        }

        private static bool RequiresGenerationOfCorrelationId(bool idInHeader, StringValues idFromHeader)
        {
            return !idInHeader || StringValues.IsNullOrEmpty(idFromHeader);
        }

        private StringValues SetCorrelationId(HttpRequestMessage request)
        {
            var correlationIdFoundInRequestHeader = request.Headers.TryGetValues(_options.Header, out var results);
            var correlationId = results?.FirstOrDefault() ?? string.Empty;

            if (RequiresGenerationOfCorrelationId(correlationIdFoundInRequestHeader, correlationId))
            {
                correlationId = GenerateCorrelationId(Activity.Current.Id);
            }

            return correlationId;
        }

        private StringValues GenerateCorrelationId(string traceIdentifier)
        {
            return _options.UseGuidForCorrelationId || string.IsNullOrEmpty(traceIdentifier) ? Guid.NewGuid().ToString() : traceIdentifier;
        }
    }
}