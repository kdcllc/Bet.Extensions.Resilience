using Bet.Extensions.Resilience.Abstractions.Options;

namespace Bet.AspNetCore.Resilience.UnitTest.PolicyBuilders
{
    public class CustomResilientOptions : PolicyOptions
    {
        public int CustomCount { get; set; } = 10;
    }
}
