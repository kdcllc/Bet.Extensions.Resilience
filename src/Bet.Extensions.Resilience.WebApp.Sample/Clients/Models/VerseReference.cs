using System.Text.Json.Serialization;

using Bet.Extensions.Resilience.WebApp.Sample.Clients.Models;

public class VerseReference
{
    [JsonPropertyName("reference")]
    public string Reference { get; set; } = string.Empty;

    [JsonPropertyName("verses")]
    public Verse[] Verses { get; set; } = Array.Empty<Verse>();

    [JsonPropertyName("text")]
    public string Text { get; set; } = string.Empty;

    [JsonPropertyName("translation_id")]
    public string TranslationId { get; set; } = string.Empty;

    [JsonPropertyName("translation_name")]
    public string TranslationName { get; set; } = string.Empty;

    [JsonPropertyName("translation_note")]
    public string TranslationNote { get; set; } = string.Empty;
}
