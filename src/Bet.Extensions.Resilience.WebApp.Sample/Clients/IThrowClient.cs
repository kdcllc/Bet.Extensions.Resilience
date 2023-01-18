namespace Bet.Extensions.Resilience.WebApp.Sample.Clients
{
    public interface IThrowClient
    {
        Task<string> GetStatusAsync(CancellationToken cancellationToken);
    }
}