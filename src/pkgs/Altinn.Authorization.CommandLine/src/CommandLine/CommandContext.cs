using System.CommandLine;
using Altinn.Authorization.CommandLine.Commands;

namespace Altinn.Authorization.CommandLine;

/// <summary>
/// Represents the context of a command, including the command itself.
/// </summary>
public class CommandContext
{
    private readonly CommandExtensions _extensions;

    internal CommandContext(Command command)
    {
        Command = command;

        _extensions = CommandExtensions.For(command);
    }

    /// <summary>
    /// Gets the command associated with this context.
    /// </summary>
    public Command Command { get; }

    /// <summary>
    /// Gets the metadata associated with this context.
    /// </summary>
    public IReadOnlyList<object> Metadata
        => _extensions.Metadata;
}
