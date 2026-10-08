namespace Altinn.Authorization.CommandLine.Shell;

/// <summary>
/// Represents a line of output from a command, including its source (standard output or standard error) and the content of the line.
/// </summary>
/// <param name="Source">The source of the output line (standard output or standard error).</param>
/// <param name="Line">The content of the output line.</param>
public readonly record struct CommandOutputLine(CommandOutputSource Source, string Line);
