using System;
using System.Collections.Generic;

namespace Bet.Extensions.Resilience.Http.Policies
{
    internal class ResilienceHttpPolicyRegistrant
    {
        public Dictionary<string, Type> RegisteredPolicies { get; } = new Dictionary<string, Type>();
    }
}
