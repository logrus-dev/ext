using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Logrus.Ext;

internal sealed class ModuleRunnerHostedService(IServiceProvider serviceProvider) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        foreach (var module in serviceProvider.GetServices<IModule>())
        {
            await module.RunServices(serviceProvider);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
