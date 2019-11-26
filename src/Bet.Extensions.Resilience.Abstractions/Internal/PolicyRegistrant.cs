using System;
using System.Collections.Generic;

namespace Bet.Extensions.Resilience.Abstractions.Internal
{
    internal sealed class PolicyRegistrant
    {
        public Dictionary<string, (Type optionsType, Type? resultType)> RegisteredPolicies { get; }
            = new Dictionary<string, (Type optionsType, Type? resultType)>();
    }
}
