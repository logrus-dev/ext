namespace Logrus.Ext.Api;

public interface IPluginCollection<out T>
{
    T this[string code] { get; }
}
