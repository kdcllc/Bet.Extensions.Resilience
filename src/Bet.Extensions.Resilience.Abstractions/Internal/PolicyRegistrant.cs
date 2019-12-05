using System;
using System.Collections.Generic;

namespace Bet.Extensions.Resilience.Abstractions.Internal
{
    public sealed class PolicyRegistrant
    {
        public Dictionary<string, Type> RegisteredPolicies { get; }
            = new Dictionary<string, Type>();
    }
}
