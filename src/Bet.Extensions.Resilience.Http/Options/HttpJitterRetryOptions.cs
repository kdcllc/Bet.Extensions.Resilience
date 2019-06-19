using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bet.Extensions.Resilience.Http.Options
{
    public class HttpJitterRetryOptions
    {
        public int MaxRetries { get; set; } = 2;
        public TimeSpan MaxDelay { get; set; } = TimeSpan.FromSeconds(200);
    }
}
