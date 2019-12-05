using Polly;

namespace Bet.Extensions.Resilience.Abstractions
{
    public interface IPolicyBucketLabel
    {
        IsPolicy GetPolicy(string policyName);
    }
}
