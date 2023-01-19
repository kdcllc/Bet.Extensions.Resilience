using System.Diagnostics;

using Bet.Extensions.Http.MessageHandlers.CorrelationId;

using Microsoft.Extensions.DiagnosticAdapter;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;

namespace Bet.Extensions.Hosting.Resilience.CorrelationId;

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
    private Activity? _activity;

    public CorrelationDiagnosticsListener(
        ICorrelationContextFactory correlationContextFactory,
        IOptions<CorrelationIdOptions> options,
        ILogger<DiagnosticListener> logger)
    {
        _correlationContextFactory = correlationContextFactory ?? throw new ArgumentNullException(nameof(correlationContextFactory));
        _options = options.Value;
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _reference = DiagnosticListener.AllListeners.Subscribe(this);

        Interlocked.Exchange(ref _activity, new Activity("Start").Start());
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogDebug("Started");
        HostingEventSource.Log.HostStart();
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogDebug("Stopped");
        HostingEventSource.Log.HostStop();
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
        _logger.LogDebug(value.Name);

        if (value.Name == "HttpHandlerDiagnosticListener"
            || value.Name == "Microsoft.AspNetCore")
        {
            _subscriptions.Add(value.SubscribeWithAdapter(this));
        }
    }

    public void OnNext(KeyValuePair<string, object> value)
    {
        _logger.LogDebug($"{value.Key} - {value.Value}");
    }

    [DiagnosticName("System.Net.Http.HttpRequestOut")]
    public virtual void IsEnabled()
    {
        // this method is required to be present for the diagnostics to work.
    }

    [DiagnosticName("System.Net.Http.HttpRequestOut.Start")]
    public virtual void OnHttpRequestOutStart(HttpRequestMessage request)
    {
        var parentActivity = Activity.Current.GetBaggageItem(_options.Header);

        Interlocked.Exchange(ref _activity, new Activity("Request").Start());

        _logger.LogDebug("System.Net.Http.HttpRequestOut.Start");

        var correlationId = string.IsNullOrEmpty(parentActivity) ? _activity?.GetBaggageItem(_options.Header) : parentActivity;

        if (string.IsNullOrEmpty(correlationId))
        {
            correlationId = SetCorrelationId(request);
            _activity?.AddBaggage(_options.Header, correlationId);

            _correlationContextFactory.Create(correlationId, _options.Header);
        }

        if (!request.Headers.TryGetValues(_options.Header, out var values))
        {
            request.Headers.Add(_options.Header, new string[] { correlationId ?? Guid.NewGuid().ToString() });
        }

        _logger.LogInformation("Request CorrelationId: {correlationId}", correlationId);
    }

    [DiagnosticName("System.Net.Http.Request")]
    public virtual void OnRequest(HttpRequestMessage request)
    {
        _logger.LogDebug("System.Net.Http.Request");
    }

    [DiagnosticName("System.Net.Http.HttpRequestOut.Stop")]
    public virtual void OnHttpRequestOutStop(
        HttpRequestMessage request,
        HttpResponseMessage response,
        TaskStatus requestTaskStatus)
    {
        _logger.LogDebug("System.Net.Http.HttpRequestOut.Stop");

        if (_options.IncludeInResponse)
        {
            if (request.Headers.TryGetValues(_options.Header, out var values))
            {
                response.Headers.Add(_options.Header, values);

                _logger.LogInformation("Response CorrelationId: {correlationId}", values);
            }
        }

        _activity?.Stop();
    }

    [DiagnosticName("System.Net.Http.Response")]
    public virtual void OnResponse(HttpResponseMessage response)
    {
        _logger.LogDebug("System.Net.Http.Response");
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
            correlationId = GenerateCorrelationId();
        }

        return correlationId;
    }

    private StringValues GenerateCorrelationId()
    {
        if (_options.UseGuidForCorrelationId || string.IsNullOrEmpty(_activity?.Id))
        {
            return Guid.NewGuid().ToString();
        }
        else
        {
            return _activity?.Id;
        }
    }
}
