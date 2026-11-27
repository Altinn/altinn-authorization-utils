using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Altinn.Authorization.ServiceDefaults.HttpClient.MaskinPorten;

internal sealed class MaskinPortenStartupValidator(
    IMaskinPortenClient client,
    IOptions<MaskinPortenStartupValidator.Options> options)
    : IHostedLifecycleService
{
    public async Task StartingAsync(CancellationToken cancellationToken)
    {
        foreach (var clientName in options.Value.ClientNames)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await client.GetAccessToken(clientName, cancellationToken);
        }
    }

    public Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StartedAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StoppingAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StoppedAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    internal sealed class Options
    {
        public HashSet<string> ClientNames { get; } = new(StringComparer.Ordinal);
    }
}
