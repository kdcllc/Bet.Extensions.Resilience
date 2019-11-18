namespace Bet.Extensions.Resilience.Abstractions
{
    /// <summary>
    /// <see cref="Polly"/> policies registration mechanism.
    /// </summary>
    public interface IPolicyRegistrator
    {
        /// <summary>
        /// Registers policies.
        /// </summary>
        void ConfigurePolicies();
    }
}
