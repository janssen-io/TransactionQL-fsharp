using Microsoft.Extensions.DependencyInjection;

namespace TransactionQL.DesktopApp.Application;

internal static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTql(this IServiceCollection services)
    {
        return services;
    }
}
