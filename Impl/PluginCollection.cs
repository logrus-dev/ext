using System.Collections;
using System.Diagnostics.CodeAnalysis;
using Logrus.Ext.Api;
using Microsoft.Extensions.DependencyInjection;

namespace Logrus.Ext.Impl;

public class PluginCollection<T>(IServiceProvider sp): IPluginCollection<T> where T : notnull
{
    private readonly PluginRegistry _pluginRegistry = sp.GetRequiredService<PluginRegistry>();

    public T this[string code] => sp.GetKeyedService<T>(code)
        ?? throw new ArgumentException($"Plugin {typeof(T).Name} with code {code} not found.");

    public IEnumerable<string> Keys => _pluginRegistry.Registrations.Where(x => x.ServiceType == typeof(T)).Select(x => x.Key);

    public IEnumerable<T> Values => Keys.Select(code => sp.GetKeyedService<T>(code) ?? throw new InvalidOperationException($"Plugin {typeof(T).Name} with code {code} not found."));

    public int Count => Keys.Count();

    public bool ContainsKey(string key) => Keys.Contains(key);

    public IEnumerator<KeyValuePair<string, T>> GetEnumerator() => Keys
        .Select(code => KeyValuePair.Create(code, this[code]))
        .GetEnumerator();

    public bool TryGetValue(string key, [MaybeNullWhen(false)] out T value)
    {
        if (ContainsKey(key))
        {
            value = this[key];
            return true;
        }

        value = default;
        return false;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
