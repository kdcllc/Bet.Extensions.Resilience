using Bet.Extensions.Resilience.Abstractions.Options;

namespace Bet.Extensions.Resilience.Http.Options;

public class HttpFallbackPolicyOptions : FallbackPolicyOptions
{
    public int StatusCode { get; set; } = 500;
}
