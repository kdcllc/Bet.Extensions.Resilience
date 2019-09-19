using System;

namespace Bet.Extensions.Resilience.Http.Options
{
    public class HttpJitterRetryOptions
    {
        public int MaxRetries { get; set; } = 2;

        public TimeSpan MaxDelay { get; set; } = TimeSpan.FromSeconds(200);
    }
}
