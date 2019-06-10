using System;

namespace Bet.Extensions.Resilience.Http.Options
{
    public class RequestTimeoutOptions
    {
        public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(3);
    }
}
