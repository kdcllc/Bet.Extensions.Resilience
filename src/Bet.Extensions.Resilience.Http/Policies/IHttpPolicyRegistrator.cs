namespace Bet.Extensions.Resilience.Http.Policies
{
    public interface IHttpPolicyRegistrator
    {
        void ConfigurePolicies();
    }
}