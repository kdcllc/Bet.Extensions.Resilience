using System;
using System.Collections.Generic;

namespace Bet.Extensions.Resilience.Http.Policies
{
    internal class HttpPolicyRegistrant
    {
        public Dictionary<string, Type> RegisteredPolicies { get; } = new Dictionary<string, Type>();
    }
}
