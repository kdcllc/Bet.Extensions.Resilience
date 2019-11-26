using Bet.Extensions.Resilience.Abstractions.Options;

namespace Bet.Extensions.Resilience.Abstractions.Policies
{
    /// <summary>
    /// The Polly Policy Descriptor interface for common properties.
    /// </summary>
    /// <typeparam name="TOptions"></typeparam>
    public interface IPolicyDescriptor<TOptions> where TOptions : PolicyOptions
    {
        /// <summary>
        /// Policy Options contains.
        /// Policy Name and Policy Options Name.
        /// </summary>
        PolicyOptions PolicyOptions { get; }

        /// <summary>
        /// The suffix to be used to identify the Async vs. Sync policies in <see cref="Polly.Registry.IPolicyRegistry{T}"/>.
        /// </summary>
        string PolicyNameSuffix { get; }

        /// <summary>
        /// Gets all of the options Configurations that has been registered for the type.
        /// </summary>
        IPolicyOptionsConfigurator<TOptions> OptionsConfigurator { get; }

        /// <summary>
        /// Gets Policy Registration Configurator.
        /// </summary>
        IPolicyRegistryConfigurator PolicyRegistryConfigurator { get; }

        /// <summary>
        /// Gets options.
        /// </summary>
        /// <returns></returns>
        TOptions Options { get; }
    }
}
