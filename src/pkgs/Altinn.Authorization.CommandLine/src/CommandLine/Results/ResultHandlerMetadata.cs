namespace Altinn.Authorization.CommandLine.Results;

/// <summary>
/// Represents metadata for a command result handler, which is used to process and write the result of a command execution to the console.
/// </summary>
public sealed class ResultHandlerMetadata
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ResultHandlerMetadata"/> class with the specified command result handler.
    /// </summary>
    /// <param name="handler">The command result handler.</param>
    internal ResultHandlerMetadata(ICommandResultHandler handler)
    {
        Handler = handler;
    }

    /// <summary>
    /// Gets the command result handler that is used to process and write the result of a command execution to the console.
    /// </summary>
    public ICommandResultHandler Handler { get; }
}
