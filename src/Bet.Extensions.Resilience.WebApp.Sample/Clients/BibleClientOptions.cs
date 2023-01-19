using Bet.Extensions.Resilience.Http.Abstractions.Options;

namespace Bet.Extensions.Resilience.WebApp.Sample.Clients;

public class BibleClientOptions : HttpClientOptions
{
    public string SomeValue { get; set; } = string.Empty;
}
