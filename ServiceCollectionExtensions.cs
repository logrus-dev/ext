using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Logrus.Ext;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddLogrusExt(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddModule<Module>(configuration);

        return services;
    }
}