using Spectre.Console;

namespace Altinn.Authorization.CommandLine.Console;

/// <summary>
/// Represents a console abstraction for the command host.
/// </summary>
public interface IConsole
    : IAnsiConsole
{
    /// <summary>
    /// Gets the standard output console.
    /// </summary>
    public IAnsiConsole StdOut { get; }

    /// <summary>
    /// Gets the standard error console.
    /// </summary>
    public IAnsiConsole StdErr { get; }
}
