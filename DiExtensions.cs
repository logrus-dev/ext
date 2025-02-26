using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Logrus.Ext;

public static class DiExtensions
{
    public static TSetting AddSettings<TSetting>(this IServiceCollection services, IConfiguration configuration)
        where TSetting : class
    {
        var settings = configuration.GetRequiredSection(typeof(TSetting).Name).Get<TSetting>()
            ?? throw new ArgumentException($"Settings not found: {typeof(TSetting)}");
        services.AddSingleton(settings);

        return settings;
    }

    public static void AddModules(this IServiceCollection services, IConfiguration configuration, params Type[] moduleTypes)
    {
        var modules = moduleTypes.Select(Activator.CreateInstance).Cast<IModule>();
        foreach (var module in modules)
        {
            services.AddSingleton(module);
            module.RegisterServices(services, configuration);
        }
    }

    public static void AddPlugin<TApi, TImpl>(this IServiceCollection services, string code)
        where TApi : class where TImpl : class, TApi
    {
        services.AddKeyedTransient<TApi, TImpl>(code);
    }

    public static async Task RunModules(this IServiceProvider services)
    {
        foreach (var module in services.GetServices<IModule>())
        {
            await module.RunServices(services);
        }
    }
}
