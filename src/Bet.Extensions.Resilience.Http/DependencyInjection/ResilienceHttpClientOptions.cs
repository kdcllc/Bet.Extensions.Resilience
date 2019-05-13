using System.Collections.Generic;

namespace Microsoft.Extensions.DependencyInjection
{
    internal class ResilienceHttpClientOptions
    {
        public Dictionary<string, HttpClientOptionsBuilder> ClientOptions { get; } = new Dictionary<string, HttpClientOptionsBuilder>();
    }
}
