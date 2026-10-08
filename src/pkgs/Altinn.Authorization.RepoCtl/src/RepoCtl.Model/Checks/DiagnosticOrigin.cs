using CommunityToolkit.Diagnostics;

namespace Altinn.Authorization.RepoCtl.Model.Checks;

/// <summary>
/// Represents the origin of a diagnostic, including its name and position in the source code.
/// </summary>
public sealed record DiagnosticOrigin
{
    /// <summary>
    /// Gets a <see cref="DiagnosticOrigin"/> representing a source file.
    /// </summary>
    /// <param name="fullPath">The full path to the source file.</param>
    /// <returns>A <see cref="DiagnosticOrigin"/> representing the source file.</returns>
    public static DiagnosticOrigin File(string fullPath)
        => new DiagnosticOrigin(fullPath, start: default, end: default);

    /// <summary>
    /// Gets a <see cref="DiagnosticOrigin"/> representing a command.
    /// </summary>
    /// <param name="command">The command string.</param>
    /// <returns>A <see cref="DiagnosticOrigin"/> representing the command.</returns>
    public static DiagnosticOrigin Command(string command)
        => new DiagnosticOrigin(command, start: default, end: default);

    /// <summary>
    /// Gets the name of the origin. This is typically either a tool name, or a source file path.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the start position in the source code.
    /// </summary>
    public LinePosition Start { get; }

    /// <summary>
    /// Gets the end position in the source code.
    /// </summary>
    public LinePosition End { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="DiagnosticOrigin"/> class.
    /// </summary>
    /// <param name="name">The tool name or source file path.</param>
    /// <param name="start">The start position.</param>
    /// <param name="end">The end position.</param>
    public DiagnosticOrigin(string name, LinePosition start, LinePosition end)
    {
        Guard.IsNotNullOrEmpty(name);

        Name = name;
        Start = start;
        End = end;
    }
}
