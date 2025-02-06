using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Logrus.Ext;

public interface IModule
{
    void RegisterServices(IServiceCollection services, IConfiguration configuration);
    Task RunServices(IServiceProvider services);
}
