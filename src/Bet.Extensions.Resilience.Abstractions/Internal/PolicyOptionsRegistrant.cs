using System;
using System.Collections.Generic;

namespace Bet.Extensions.Resilience.Abstractions.Internal
{
    public sealed class PolicyOptionsRegistrant
    {
        public Dictionary<string, (string sectionName, Type optionsType)> RegisteredPolicyOptions { get; }
            = new Dictionary<string, (string sectionName, Type optionsType)>();
    }
}
