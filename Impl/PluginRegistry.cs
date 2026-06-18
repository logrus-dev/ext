namespace Logrus.Ext.Impl;

internal class PluginRegistry
{
    private readonly List<PluginRegistration> _registrations = [];

    public IReadOnlyList<PluginRegistration> Registrations => _registrations;

    public void Add(string key, Type serviceType)
    {
        _registrations.Add(new(key, serviceType));
    }
}
internal record PluginRegistration(string Key, Type ServiceType);
