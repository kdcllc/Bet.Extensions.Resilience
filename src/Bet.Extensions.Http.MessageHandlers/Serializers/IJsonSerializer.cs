namespace Bet.Extensions.Http.MessageHandlers.Serializers;

public interface IJsonSerializer
{
    T? Deserialize<T>(string value);
}
