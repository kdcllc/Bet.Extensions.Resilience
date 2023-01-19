namespace Bet.Extensions.Resilience.Http.Abstractions.Options;

/// <summary>
/// The options for <see cref="HttpClient"/>.
/// </summary>
public class HttpClientOptions
{
    /// <summary>
    /// The base address uri for the <see cref="HttpClient"/>.
    /// </summary>
    public Uri BaseAddress { get; set; } = default!;

    /// <summary>
    /// The timespan before the <see cref="HttpClient"/> timeouts. The default value is 100 seconds or 1:40 min.
    /// </summary>
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(100);

    /// <summary>
    /// The content type of the <see cref="HttpClient"/> request.
    /// </summary>
    public string ContentType { get; set; } = string.Empty;
}
