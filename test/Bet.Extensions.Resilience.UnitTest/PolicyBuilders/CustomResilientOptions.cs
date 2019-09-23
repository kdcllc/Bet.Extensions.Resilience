﻿using Bet.Extensions.Resilience.Http.Options;

namespace Bet.Extensions.Resilience.UnitTest.PolicyBuilders
{
    public class CustomResilientOptions : HttpPolicyOptions
    {
        public int CustomCount { get; set; } = 10;
    }
}
