using Spectre.Console;

namespace Altinn.Cli.Jwks.Console;

internal interface IConsole
    : IAnsiConsole
{
    public IAnsiConsole StdOut { get; }

    public IAnsiConsole StdErr { get; }
}
