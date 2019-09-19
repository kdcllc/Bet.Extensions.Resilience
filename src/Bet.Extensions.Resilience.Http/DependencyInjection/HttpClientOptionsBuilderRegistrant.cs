using System.Collections.Generic;

namespace Microsoft.Extensions.DependencyInjection
{
    internal class ResilienceHttpClientOptions
    {
        public Dictionary<string, HttpClientOptionsBuilder> RegisteredBuilders { get; } = new Dictionary<string, HttpClientOptionsBuilder>();
    }
}
