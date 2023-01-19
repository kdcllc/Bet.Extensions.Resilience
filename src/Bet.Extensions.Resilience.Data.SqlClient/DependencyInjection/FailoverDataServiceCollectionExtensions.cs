namespace Microsoft.Extensions.DependencyInjection;

public static class FailoverDataServiceCollectionExtensions
{
    public static IServiceCollection AddFailoverDbConnection(this IServiceCollection services)
    {
        return services;
    }
}
