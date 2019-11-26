using System.Collections.Generic;

using Bet.Extensions.Resilience.Abstractions.Options;

using Microsoft.Extensions.Primitives;

namespace Bet.Extensions.Resilience.Abstractions
{
    /// <summary>
    /// The interface responsible for managing the Options Changed Event for registered options configurations.
    /// </summary>
    /// <typeparam name="TOptions"></typeparam>
    public interface IPolicyOptionsConfigurator<TOptions> where TOptions : PolicyOptions
    {
        IReadOnlyDictionary<string, TOptions> GetAllOptions { get; }

        /// <summary>
        /// Get the named policy option instance.
        /// </summary>
        /// <param name="optionsName"></param>
        /// <returns></returns>
        TOptions GetOptions(string optionsName);

        /// <summary>
        /// Returns change token for the refresh of the configurations.
        /// </summary>
        /// <returns></returns>
        IChangeToken GetChangeToken();
    }
}
