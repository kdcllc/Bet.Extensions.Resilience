namespace Bet.Extensions.Resilience.WebApp.Sample.Models;

public class BibleQuoteModel
{
    public string Search { get; set; } = string.Empty;

    public VerseReference? Result { get; set; }
}
