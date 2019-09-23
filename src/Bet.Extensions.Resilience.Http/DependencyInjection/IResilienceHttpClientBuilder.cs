using System.Net.Http;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Provides builder pattern for <see cref="HttpClient"/> registration.
    /// </summary>
    public interface IResilienceHttpClientBuilder : IResilienceBuilder
    {
    }
}
