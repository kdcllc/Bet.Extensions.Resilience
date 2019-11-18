using System;
using System.Collections.Generic;

namespace Bet.Extensions.Resilience.Abstractions
{
    internal sealed class PolicyRegistrant
    {
        public Dictionary<string, Type> RegisteredPolicies { get; } = new Dictionary<string, Type>();
    }
}
