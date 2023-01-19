using System.Text.Json.Serialization;

namespace Bet.Extensions.Resilience.WebApp.Sample.Clients.Models;

public class Verse
{
    [JsonPropertyName("book_id")]
    public string BookId { get; set; } = string.Empty;

    [JsonPropertyName("book_name")]
    public string BookName { get; set; } = string.Empty;

    [JsonPropertyName("chapter")]
    public int Chapter { get; set; }

    [JsonPropertyName("verse")]
    public int VerseNumber { get; set; }

    [JsonPropertyName("text")]
    public string Text { get; set; } = string.Empty;
}
