using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System.CommandLine;

namespace Altinn.Cli.Jwks;

internal sealed class ConsoleHostedService
    : IHostedService
    , IDisposable
{
    private readonly RootCommand _cmd;
    private readonly ConsoleService _console;
    private readonly IHostApplicationLifetime _lifetime;
    private readonly IOptionsMonitor<ParserConfiguration> _parserOptions;
    private readonly IOptionsMonitor<InvocationConfiguration> _invocationOptions;
    private CancellationTokenSource? _cts;

    public ConsoleHostedService(
        RootCommand cmd,
        ConsoleService console,
        IHostApplicationLifetime lifetime,
        IOptionsMonitor<ParserConfiguration> parserOptions,
        IOptionsMonitor<InvocationConfiguration> invocationOptions)
    {
        _cmd = cmd;
        _console = console;
        _lifetime = lifetime;
        _parserOptions = parserOptions;
        _invocationOptions = invocationOptions;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        var parsed = _cmd.Parse(_console.Args, _parserOptions.CurrentValue);
        var result = await parsed.InvokeAsync(_invocationOptions.CurrentValue, _cts.Token);

        _console.SetResult(result);
        _lifetime.StopApplication();
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        if (_cts is null)
        {
            return Task.CompletedTask;
        }

        return _cts.CancelAsync().WaitAsync(cancellationToken);
    }

    public void Dispose()
    {
        _cts?.Dispose();
    }
}
