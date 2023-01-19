namespace Bet.Extensions.Http.MessageHandlers.Serializers;

public static class JsonSerializerExtensions
{
    public static TOptions AndReturn<TOptions>(this Action<TOptions> configure, TOptions options)
    {
        configure(options);
        return options;
    }
}
