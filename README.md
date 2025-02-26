# Extensibility primitives for modular applications

A small set of very simple extensions which help to split an application into multiple modules and/or plugins.

## Modules

Module - a lightweight alternative to [Autofac's modules](https://docs.autofac.org/en/latest/configuration/modules.html) or [Ninject's modules](https://github.com/ninject/Ninject/wiki/Modules-and-the-Kernel#modules) for native .NET DI.
Basically, a group of DI component registrations. Usually, there is one module per project.

To create a module - add `Module.cs` to the root of the project:
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

Then, use `AddModules` extension method to inject the module to the host app.
```
var services = new ServiceCollection(); // Or use one from the host builder
services.AddModules(configuration, typeof(Module));

await using var serviceProvider = services.BuildServiceProvider();
await serviceProvider.RunModules();
```

Optionally, modules can be _run_. This is suitable for all kinds of startup logic, e.g., cache warmup or DB migrations.

```
await using var serviceProvider = services.BuildServiceProvider();
await serviceProvider.RunModules();
```

## Plugins

Plugin - a service which has multiple implementations and one of them is used depending on some conditions. Pretty much an implementation of [Plugin pattern](https://martinfowler.com/eaaCatalog/plugin.html).

Plugins are added to the application statically using `AddPlugin` extension (optionally - inside a module).
```
services.AddPlugin<IStrategy, OneOfManyStrategyImplementations>("code-name-of-the-strategy");
```

Then, `IPluginCollection<>` can be used to obtain the plugin.

```
class Consumer(IPluginCollection<IStrategy> plugins)
{
    public void ProceedUsingStrategy(string pluginName)
    {
        var plugin = services[pluginName];
    }
}
```
Under the hood, `AddPlugin` and `IPluginCollection` are just tiny wrappers on the top of the standard [Keyed services](https://learn.microsoft.com/en-us/dotnet/core/extensions/dependency-injection#keyed-services).
