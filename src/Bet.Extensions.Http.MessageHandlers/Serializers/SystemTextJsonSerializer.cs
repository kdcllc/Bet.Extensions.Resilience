using System.Text.Json;
using System.Text.Json.Serialization;

namespace Bet.Extensions.Http.MessageHandlers.Serializers
{
    public class SystemTextJsonSerializer : IJsonSerializer
    {
        public SystemTextJsonSerializer() : this(DefaultJsonSerializerOptions) { }

        public SystemTextJsonSerializer(JsonSerializerOptions options)
        {
            Options = options;
        }

        public static JsonSerializerOptions DefaultJsonSerializerOptions => new()
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            Converters = { new JsonStringEnumConverter() },
            NumberHandling = JsonNumberHandling.AllowReadingFromString | JsonNumberHandling.AllowNamedFloatingPointLiterals,

            WriteIndented = true,
        };

        public JsonSerializerOptions Options { get; }

        public T? Deserialize<T>(string value)
        {
            return JsonSerializer.Deserialize<T>(value, Options);
        }
    }
}
