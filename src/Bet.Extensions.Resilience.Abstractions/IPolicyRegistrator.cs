namespace Bet.Extensions.Resilience.Abstractions
{
    public interface IPolicyRegistrator
    {
        /// <summary>
        /// Registers policies.
        /// </summary>
        void ConfigurePolicies();
    }
}
