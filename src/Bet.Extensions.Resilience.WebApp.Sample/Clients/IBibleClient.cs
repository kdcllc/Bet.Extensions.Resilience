namespace Bet.Extensions.Resilience.WebApp.Sample.Clients;

public interface IBibleClient
{
    Task<VerseReference?> GetQuoteAsync(string search, CancellationToken cancellationToken);
}
