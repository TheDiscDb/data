namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTheDiscDbClient(this IServiceCollection services, Uri baseAddress)
    {
        services
        .AddTheDiscDbClient()
        .ConfigureHttpClient(client => client.BaseAddress = baseAddress);

        return services;
    }
}
