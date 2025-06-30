using CommunityToolkit.Diagnostics;
using Spectre.Console;
using Spectre.Console.Rendering;
using System.Diagnostics.CodeAnalysis;

namespace Altinn.Cli.Jwks.Console;

[ExcludeFromCodeCoverage]
internal sealed class Console
    : IConsole
    , IExclusivityMode
    , IDisposable
{
    private readonly SemaphoreSlim _semaphore;
    private readonly IAnsiConsole _stdOut;
    private readonly IAnsiConsole _stdErr;

    IAnsiConsole IConsole.StdOut 
        => _stdOut;

    IAnsiConsole IConsole.StdErr 
        => _stdErr;

    Profile IAnsiConsole.Profile 
        => _stdOut.Profile;

    IAnsiConsoleCursor IAnsiConsole.Cursor 
        => _stdOut.Cursor;

    IAnsiConsoleInput IAnsiConsole.Input 
        => _stdOut.Input;

    IExclusivityMode IAnsiConsole.ExclusivityMode 
        => _stdOut.ExclusivityMode;

    RenderPipeline IAnsiConsole.Pipeline 
        => _stdOut.Pipeline;

    public Console()
    {
        _semaphore = new SemaphoreSlim(1, 1);
        
        _stdOut = AnsiConsole.Create(new AnsiConsoleSettings
        {
            Ansi = AnsiSupport.Detect,
            ColorSystem = ColorSystemSupport.Detect,
            ExclusivityMode = this,
            Out = new AnsiConsoleOutput(System.Console.Out),
        });

        _stdErr = AnsiConsole.Create(new AnsiConsoleSettings
        {
            Ansi = AnsiSupport.Detect,
            ColorSystem = ColorSystemSupport.Detect,
            ExclusivityMode = this,
            Out = new AnsiConsoleOutput(System.Console.Error),
        });
    }

    T IExclusivityMode.Run<T>(Func<T> func)
    {
        // Try acquiring the exclusivity semaphore
        if (!_semaphore.Wait(0))
        {
            ThrowExclusivityException();
        }

        try
        {
            return func();
        }
        finally
        {
            _semaphore.Release(1);
        }
    }

    async Task<T> IExclusivityMode.RunAsync<T>(Func<Task<T>> func)
    {
        // Try acquiring the exclusivity semaphore
        if (!await _semaphore.WaitAsync(0).ConfigureAwait(false))
        {
            ThrowExclusivityException();
        }

        try
        {
            return await func().ConfigureAwait(false);
        }
        finally
        {
            _semaphore.Release(1);
        }
    }

    private static void ThrowExclusivityException()
    {
        ThrowHelper.ThrowInvalidOperationException(
            "Trying to run one or more interactive functions concurrently. " +
            "Operations with dynamic displays (e.g. a prompt and a progress display) " +
            "cannot be running at the same time.");
    }

    public void Dispose()
        => _semaphore.Dispose();

    void IAnsiConsole.Clear(bool home)
        => _stdOut.Clear(home);

    void IAnsiConsole.Write(IRenderable renderable)
        => _stdOut.Write(renderable);
}
