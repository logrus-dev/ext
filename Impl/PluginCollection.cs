using Logrus.Ext.Api;
using Microsoft.Extensions.DependencyInjection;

namespace Logrus.Ext.Impl;

public class PluginCollection<T>(IServiceProvider sp): IPluginCollection<T> where T : notnull
{
    public T this[string code] => sp.GetKeyedService<T>(code)
        ?? throw new ArgumentException($"Plugin {typeof(T).Name} with code {code} not found.");
}
