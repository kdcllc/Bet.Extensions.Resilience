using Bet.Extensions.Resilience.Abstractions.Options;

using Polly;

namespace Microsoft.Extensions.DependencyInjection
{
    public interface IResilienceBuilder<TPolicy, TOptions> where TPolicy : IsPolicy where TOptions : PolicyOptions
    {
        IServiceCollection Services { get; }

        string PolicyName { get; }

        List<string> PolicyNames { get; }
    }
}
