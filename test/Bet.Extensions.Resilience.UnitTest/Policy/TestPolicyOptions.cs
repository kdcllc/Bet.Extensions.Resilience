using Bet.Extensions.Resilience.Abstractions.Options;

namespace Bet.Extensions.Resilience.UnitTest.Policy
{
    public class TestPolicyOptions : PolicyOptions
    {
        public int Count { get; set; } = 10;
    }
}
