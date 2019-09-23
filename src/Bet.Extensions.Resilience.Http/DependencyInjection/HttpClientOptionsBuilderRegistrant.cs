using System.Collections.Generic;

namespace Microsoft.Extensions.DependencyInjection
{
    internal class HttpClientOptionsBuilderRegistrant
    {
        public Dictionary<string, HttpClientOptionsBuilder> RegisteredHttpClientBuilders { get; } = new Dictionary<string, HttpClientOptionsBuilder>();
    }
}
