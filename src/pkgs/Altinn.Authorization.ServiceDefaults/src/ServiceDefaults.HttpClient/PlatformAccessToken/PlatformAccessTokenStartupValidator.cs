using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Altinn.Authorization.ServiceDefaults.HttpClient.PlatformAccessToken;

internal sealed class PlatformAccessTokenStartupValidator(
    IServiceScopeFactory scopeFactory,
    IHttpClientFactory httpClientFactory,
    IOptions<PlatformAccessTokenStartupValidator.Options> options)
    : IHostedLifecycleService
{
    public async Task StartingAsync(CancellationToken cancellationToken)
    {
        foreach (var clientName in options.Value.ClientNames)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await using var scope = scopeFactory.CreateAsyncScope();
            var provider = scope.ServiceProvider.GetRequiredService<IPlatformAccessTokenProvider>();
            using var client = httpClientFactory.CreateClient(clientName);
            using var request = new HttpRequestMessage(HttpMethod.Get, client.BaseAddress);
            await provider.GetPlatformAccessToken(request, cancellationToken);
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
