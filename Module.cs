using Logrus.Ext.Api;
using Logrus.Ext.Impl;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Logrus.Ext;

public class Module: IModule
{
    public void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddTransient(typeof(IPluginCollection<>), typeof(PluginCollection<>));
    }

    public Task RunServices(IServiceProvider services) => Task.CompletedTask;
}
