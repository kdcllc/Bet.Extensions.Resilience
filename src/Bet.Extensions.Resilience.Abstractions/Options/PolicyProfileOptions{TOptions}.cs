using System;

using Microsoft.Extensions.Logging;

using Polly;

namespace Bet.Extensions.Resilience.Abstractions.Options
{
    public class PolicyProfileOptions<TOptions> where TOptions : PolicyOptions
    {
        public string Name { get; set; } = string.Empty;

        public IServiceProvider? ServiceProvider { get; set; }

        public Func<TOptions, ILogger, IsPolicy> ConfigurePolicy { get; set; } = (options, logger) => Policy.NoOpAsync();
    }
}
