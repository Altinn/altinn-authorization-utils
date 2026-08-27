using Spectre.Console;
using Spectre.Console.Rendering;

namespace Altinn.Authorization.CommandLine.Formatting;

/// <summary>
/// Default implementation of <see cref="IFormatWriter"/> that writes formatted output to an <see cref="IAnsiConsole"/>.
/// </summary>
internal sealed class FormatWriter
    : IFormatWriter
{
    private readonly IAnsiConsole _console;

    public FormatWriter(IAnsiConsole console, IServiceProvider applicationServices)
    {
        _console = console;
        ApplicationServices = applicationServices;
    }

    public IServiceProvider ApplicationServices { get; }

    public Profile Profile
        => _console.Profile;

    public void Write(IRenderable renderable)
        => _console.Write(renderable);

    public void WriteAnsi(Action<AnsiWriter> action)
        => _console.WriteAnsi(action);
}
