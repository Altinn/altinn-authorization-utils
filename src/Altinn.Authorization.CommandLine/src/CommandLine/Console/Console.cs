using Spectre.Console;
using Spectre.Console.Rendering;

namespace Altinn.Authorization.CommandLine.Console;

internal sealed class Console
    : IConsole
{
    private readonly IAnsiConsole _stdOut;
    private readonly IAnsiConsole _stdErr;

    public Console(IExclusivityMode exclusivityMode)
    {
        _stdOut = AnsiConsole.Create(new AnsiConsoleSettings
        {
            Ansi = AnsiSupport.Detect,
            ColorSystem = ColorSystemSupport.Detect,
            ExclusivityMode = exclusivityMode,
            Out = new AnsiConsoleOutput(System.Console.Out),
        });

        _stdErr = AnsiConsole.Create(new AnsiConsoleSettings
        {
            Ansi = AnsiSupport.Detect,
            ColorSystem = ColorSystemSupport.Detect,
            ExclusivityMode = exclusivityMode,
            Out = new AnsiConsoleOutput(System.Console.Error),
        });
    }

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

    void IAnsiConsole.Clear(bool home)
        => _stdOut.Clear(home);

    void IAnsiConsole.Write(IRenderable renderable)
        => _stdOut.Write(renderable);

    void IAnsiConsole.WriteAnsi(Action<AnsiWriter> action)
        => _stdOut.WriteAnsi(action);
}
