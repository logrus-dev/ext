using System.Runtime.Loader;
using Logrus.Ext.Impl;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Logrus.Ext;

public static class DiExtensions
{
    public static TSetting AddSettings<TSetting>(this IServiceCollection services, IConfiguration configuration, string prefix = "")
        where TSetting : class
    {
        var settings = configuration.GetRequiredSection(prefix + typeof(TSetting).Name).Get<TSetting>()
            ?? configuration.GetRequiredSection(prefix + ":" + typeof(TSetting).Name).Get<TSetting>()
            ?? throw new ArgumentException($"Settings not found: {typeof(TSetting)}");
        services.AddSingleton(settings);

        return settings;
    }

    public static IServiceCollection AddModule(this IServiceCollection services, IConfiguration configuration, IModule module)
    {
        return services.AddModules(configuration, module);
    }

    public static IHostBuilder AddModule(this IHostBuilder builder, IModule module)
    {
        return builder.AddModules(module);
    }

    public static IHostBuilder AddModule<TModule>(this IHostBuilder builder) where TModule : IModule
    {
        return builder.AddModule(typeof(TModule));
    }

    public static IServiceCollection AddModule<TModule>(this IServiceCollection services, IConfiguration configuration) where TModule : IModule
    {
        return services.AddModule(configuration, typeof(TModule));
    }

    internal static IServiceCollection AddDynamicModules(this IServiceCollection services, IConfiguration configuration)
    {
        var dynamicModules = configuration.GetSection("Logrus:Ext:DynamicModules");
        if (dynamicModules.Exists())
        {
            foreach (var assemblyPath in dynamicModules.Get<string[]>() ?? [])
            {
                var moduleTypes = AssemblyLoadContext.Default
                    .LoadFromAssemblyPath(assemblyPath)
                    .GetTypes()
                    .Where(x => x.IsAssignableTo(typeof(IModule)));
                foreach (var moduleType in moduleTypes)
                {
                    services.AddModule(configuration, moduleType);
                }
            }
        }
        return services;
    }

    public static IHostBuilder AddModule(this IHostBuilder builder, Type moduleType)
    {
        builder.ConfigureServices((ctx, services) => services.AddModule(ctx.Configuration, moduleType));

        return builder;
    }

    public static IServiceCollection AddModule(this IServiceCollection services, IConfiguration configuration, Type moduleType)
    {
        var module = (IModule) (Activator.CreateInstance(moduleType) ?? throw new NullReferenceException($"Could not create an instance of {moduleType.FullName}"));

        services.AddModule(configuration, module);
        return services;
    }

    public static IHostBuilder AddModules(this IHostBuilder builder, params IModule[] modules)
    {
        builder.ConfigureServices((ctx, services) => services.AddModules(ctx.Configuration, modules));
        return builder;
    }

    public static IServiceCollection AddModules(this IServiceCollection services, IConfiguration configuration, params IModule[] modules)
    {
        foreach (var module in modules)
        {
            services.AddSingleton(module);
            module.RegisterServices(services, configuration);
        }
        return services;
    }

    public static IServiceCollection AddPlugin<TApi, TImpl>(this IServiceCollection services, string code)
        where TApi : class where TImpl : class, TApi
    {
        var registryDescriptor = services.Single(x => x.ServiceType == typeof(PluginRegistry));

        var registry = (PluginRegistry) (registryDescriptor.ImplementationInstance ?? throw new NullReferenceException("Plugin registry is missing in service collection."));

        services.AddKeyedTransient<TApi, TImpl>(code);
        registry.Add(code, typeof(TApi));
        return services;
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
