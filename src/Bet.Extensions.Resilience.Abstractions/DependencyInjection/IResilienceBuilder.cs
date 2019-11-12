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
        /// Name of the Options registered with the client.
        /// It can be the same as the Name or different.
        /// </summary>
        string OptionsName { get; }

        /// <summary>
        /// Gets the application service collection.
        /// </summary>
        IServiceCollection Services { get; }
    }
}
