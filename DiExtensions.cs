using System.Reflection;
using System.Runtime.Loader;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

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

    public static IHostBuilder AddModules(this IHostBuilder builder, params object[] modulesOrTypes)
    {
        return builder.ConfigureServices((ctx, services) => services.AddModules(ctx.Configuration, modulesOrTypes));
    }

    public static void AddModules(this IServiceCollection services, IConfiguration configuration, params object[] modulesOrTypes)
    {
        var arguments = modulesOrTypes.ToList();

        var dynamicModules = configuration.GetSection("Logrus:Ext:DynamicModules");
        if (dynamicModules.Exists())
        {
            foreach (var assemblyPath in dynamicModules.Get<string[]>() ?? [])
            {
                var moduleTypes = AssemblyLoadContext.Default
                    .LoadFromAssemblyPath(assemblyPath)
                    .GetTypes()
                    .Where(x => x.IsAssignableTo(typeof(IModule)))
                    .ToArray();
                arguments.AddRange(moduleTypes);
            }
        }

        var modules = arguments
            .Select(x => x is Type moduleType ? Activator.CreateInstance(moduleType)! : x)
            .Cast<IModule>()
            .Distinct()
            .ToArray();

        services.AddModules(configuration, modules);
    }

    public static void AddModules(this IServiceCollection services, IConfiguration configuration, params IModule[] modules)
    {
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

    public static Task RunModules(this IHost host) => host.Services.RunModules();

    public static async Task RunModules(this IServiceProvider services)
    {
        foreach (var module in services.GetServices<IModule>())
        {
            await module.RunServices(services);
        }
    }
}
