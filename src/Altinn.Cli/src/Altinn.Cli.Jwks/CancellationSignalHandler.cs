using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace Altinn.Cli.Jwks;

[ExcludeFromCodeCoverage]
internal class CancellationSignalHandler
    : IDisposable
{
    private readonly CancellationTokenSource _cancellationTokenSource;
    private readonly PosixSignalRegistration _sigInt;
    private readonly PosixSignalRegistration _sigTerm;
    private readonly PosixSignalRegistration _sigQuit;
    private readonly Action<PosixSignalContext> _onSignal;

    public CancellationSignalHandler()
    {
        _cancellationTokenSource = new CancellationTokenSource();
        _onSignal = CreateSignalCallback(_cancellationTokenSource);

        _sigInt = PosixSignalRegistration.Create(PosixSignal.SIGINT, _onSignal);
        _sigTerm = PosixSignalRegistration.Create(PosixSignal.SIGTERM, _onSignal);
        _sigQuit = PosixSignalRegistration.Create(PosixSignal.SIGQUIT, _onSignal);
    }

    public CancellationToken Token => _cancellationTokenSource.Token;

    public void Dispose()
    {
        _sigQuit.Dispose();
        _sigTerm.Dispose();
        _sigInt.Dispose();
        _cancellationTokenSource.Dispose();
    }

    private static Action<PosixSignalContext> CreateSignalCallback(CancellationTokenSource source)
    {
        return ctx =>
        {
            ctx.Cancel = true;
            source.Cancel();
        };
    }
}
