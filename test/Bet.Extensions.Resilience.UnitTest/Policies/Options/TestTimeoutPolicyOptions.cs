using Bet.Extensions.Resilience.Abstractions.Options;

namespace Bet.Extensions.Resilience.UnitTest.Policies.Options;

public class TestTimeoutPolicyOptions : TimeoutPolicyOptions
{
    public string CustomValue { get; set; } = string.Empty;
}
