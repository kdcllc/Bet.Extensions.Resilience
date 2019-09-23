using System;
using System.Collections.Generic;

namespace Bet.Extensions.Resilience.Http.Policies
{
    internal class ResilienceHttpPolicyRegistrant
    {
        public List<Type> RegisteredPolicies { get; } = new List<Type>();
    }
}
