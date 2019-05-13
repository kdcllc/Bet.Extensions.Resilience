namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Resilience HttpClient Builder for Dependency Injection registration.
    /// </summary>
    public interface IResilienceHttpClientBuilder
    {
        /// <summary>
        /// Gets the name of the client configured by this builder.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the application service collection.
        /// </summary>
        IServiceCollection Services { get; }
    }
}
