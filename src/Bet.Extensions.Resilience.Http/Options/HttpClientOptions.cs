using System;
using System.Net.Http;

namespace Bet.Extensions.Resilience.Http.Options
{
    /// <summary>
    /// The options for <see cref="HttpClient"/>.
    /// </summary>
    public class HttpClientOptions
    {
        /// <summary>
        /// The base address uri for the <see cref="HttpClient"/>.
        /// </summary>
        public Uri BaseAddress { get; set; }

        /// <summary>
        /// The timespan before the <see cref="HttpClient"/> timeouts.
        /// </summary>
        public TimeSpan Timeout { get; set; } = TimeSpan.FromMinutes(2);

        /// <summary>
        /// The content type of the <see cref="HttpClient"/> request.
        /// </summary>
        public string ContentType { get; set; }
    }
}
