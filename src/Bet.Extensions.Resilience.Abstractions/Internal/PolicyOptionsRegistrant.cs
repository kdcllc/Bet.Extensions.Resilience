using System;
using System.Collections.Generic;

namespace Bet.Extensions.Resilience.Abstractions.Internal
{
    /// <summary>
    /// This interface is a glue between <see cref="IPolicyOptionsConfigurator{TOptions}"/> and <see cref="IPolicyRegistryConfigurator"/>
    /// </summary>
    internal sealed class PolicyOptionsRegistrant
    {
        public Dictionary<string, (string sectionName, Type optionsType)> RegisteredPolicyOptions { get; }
            = new Dictionary<string, (string sectionName, Type optionsType)>();
    }
}
