using Bet.Extensions.Resilience.Abstractions.Options;

namespace Bet.Extensions.Resilience.Abstractions.Policies
{
    /// <summary>
    /// The Policy Registration Interface.
    /// The <see cref="IPolicyRegistrator"/> utilizes this interface to register all of the policies.
    /// </summary>
    /// <typeparam name="TOptions">The type of the options.</typeparam>
    public interface IPolicy<TOptions>
        : IPolicyDescriptor<TOptions>, IPolicyConfigurator<TOptions> where TOptions : PolicyOptions
    {
    }

    /// <summary>
    /// The Policy Registration Interface.
    /// The <see cref="IPolicyRegistrator"/> utilizes this interface to register all of the policies.
    /// </summary>
    /// <typeparam name="TOptions">The options type.</typeparam>
    /// <typeparam name="TResult">The policy return type.</typeparam>
    public interface IPolicy<TOptions, TResult>
        : IPolicyDescriptor<TOptions>, IPolicyConfigurator<TOptions, TResult> where TOptions : PolicyOptions
    {
    }
}
