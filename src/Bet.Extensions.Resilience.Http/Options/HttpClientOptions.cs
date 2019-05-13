using System;

namespace Bet.Extensions.Resilience.Http.Options
{
    public class HttpClientOptions
    {
        public Uri BaseAddress { get; set; }

        public TimeSpan Timeout { get; set; }

        public string ContentType { get; set; }
    }
}
