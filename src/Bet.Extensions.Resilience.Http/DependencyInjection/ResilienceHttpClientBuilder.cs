namespace Microsoft.Extensions.DependencyInjection
{
    /// <inheritdoc/>
    public class ResilienceHttpClientBuilder : IResilienceHttpClientBuilder
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ResilienceHttpClientBuilder"/> class.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="name"></param>
        public ResilienceHttpClientBuilder(IServiceCollection services, string name)
        {
            Services = services;
            Name = name;
        }

        /// <inheritdoc/>
        public string Name { get; }

        /// <inheritdoc/>
        public IServiceCollection Services { get; }
    }
}
