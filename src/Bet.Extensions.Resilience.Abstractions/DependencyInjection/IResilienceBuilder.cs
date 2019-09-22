namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Resilience Builder for Dependency Injection registration.
    /// </summary>
    public interface IResilienceBuilder
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
