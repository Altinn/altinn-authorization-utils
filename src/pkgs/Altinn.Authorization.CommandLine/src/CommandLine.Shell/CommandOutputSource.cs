namespace Altinn.Authorization.CommandLine.Shell;

/// <summary>
/// Represents the source of a command output line.
/// </summary>
public enum CommandOutputSource
    : byte
{
    /// <summary>
    /// Standard output.
    /// </summary>
    Stdout = 1,

    /// <summary>
    /// Standard error.
    /// </summary>
    Stderr = 2,
}
