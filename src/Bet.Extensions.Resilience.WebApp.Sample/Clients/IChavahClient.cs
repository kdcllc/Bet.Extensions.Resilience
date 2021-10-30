using Bet.Extensions.Resilience.WebApp.Sample.Clients.Models;

namespace Bet.Extensions.Resilience.WebApp.Sample.Clients
{
    public interface IChavahClient
    {
        Task<IEnumerable<Song>> GetPopular(int count);
    }
}
