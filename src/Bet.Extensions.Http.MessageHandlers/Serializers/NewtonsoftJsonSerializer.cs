using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Bet.Extensions.Http.MessageHandlers.Serializers
{
    public class NewtonsoftJsonSerializer : IJsonSerializer
    {
        public NewtonsoftJsonSerializer() : this(DefaultJsonSerializerSettings) { }

        public NewtonsoftJsonSerializer(Action<JsonSerializerSettings> configure) : this(configure.AndReturn(DefaultJsonSerializerSettings)) { }

        public NewtonsoftJsonSerializer(JsonSerializerSettings jsonSerializerSettings)
        {
            JsonSerializerSettings = jsonSerializerSettings;
        }

        public static JsonSerializerSettings DefaultJsonSerializerSettings => new ()
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver { IgnoreIsSpecifiedMembers = true },
            MissingMemberHandling = MissingMemberHandling.Ignore
        };

        public JsonSerializerSettings JsonSerializerSettings { get; }

        public T? Deserialize<T>(string value)
        {
            return JsonConvert.DeserializeObject<T>(value);
        }
    }
}
