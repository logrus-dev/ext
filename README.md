# Extensibility primitives for modular applications

A small set of very simple helpers to split an application into multiple modules and/or plugins.

## Modules

Add a `Module.cs` to the root of your project:
```
public class Module: IModule
{
    public void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddTransient<IServiceApi, ServiceImpl>();
    }

    public Task RunServices(IServiceProvider services) => Task.CompletedTask;
}
```

Then, use this to inject your module to the host app:
```
var services = new ServiceCollection();
services.AddModules(configuration, typeof(Module));

await using var serviceProvider = services.BuildServiceProvider();
await serviceProvider.RunModules();

```

## Plugins

Statically linked plugins can be registered using standard .NET keyed service registration helper:
```
services.AddKeyedTransient<IServiceApi, ServiceImpl>("service-name");
```

Then, next convenient wrapper can be used to obtain the plugin

```
class Consumer(IPluginCollection<IService> plugins)
{
    public void AMethod(string pluginKey)
    {
        var plugin = services[pluginKey];
    }
}
```

