using System.Collections.Generic;

namespace Microsoft.Extensions.DependencyInjection
{
    internal class HttpClientOptionsBuilderRegistrant
    {
        public Dictionary<string, HttpClientOptionsBuilder> RegisteredBuilders { get; } = new Dictionary<string, HttpClientOptionsBuilder>();
    }
}
