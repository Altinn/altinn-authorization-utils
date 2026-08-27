namespace Altinn.Authorization.CommandLine.Results;

/// <summary>
/// Represents metadata for a command result type, which is used to determine how to process and write the result of a command execution to the console.
/// </summary>
public sealed class ResultTypeMetadata
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ResultTypeMetadata"/> class with the specified command result type.
    /// </summary>
    /// <param name="resultType">The command result type.</param>
    internal ResultTypeMetadata(Type resultType)
    {
        ResultType = resultType;
    }

    /// <summary>
    /// Gets the command result type that is used to determine how to process and write the result of a command execution to the console.
    /// </summary>
    public Type ResultType { get; }
}
