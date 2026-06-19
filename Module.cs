using Logrus.Ext.Api;
using Logrus.Ext.Impl;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Logrus.Ext;

public class Module: IModule
{
    public void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddHostedService<ModuleRunnerHostedService>();
        services.AddTransient(typeof(IPluginCollection<>), typeof(PluginCollection<>));
        services.AddSingleton(new PluginRegistry());
        services.AddDynamicModules(configuration);
    }

    public Task RunServices(IServiceProvider services) => Task.CompletedTask;
}
