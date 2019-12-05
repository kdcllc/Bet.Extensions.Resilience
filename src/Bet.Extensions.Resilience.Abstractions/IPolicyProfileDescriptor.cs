using Polly;

namespace Bet.Extensions.Resilience.Abstractions
{
    public interface IPolicyProfileDescriptor
    {
        IsPolicy GetPolicy(string policyName);
    }
}
