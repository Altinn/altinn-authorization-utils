namespace Altinn.Authorization.CommandLine.PreProcessing;

/// <summary>
/// Defines a preprocessor for command line arguments.
/// </summary>
public interface IArgumentPreProcessor
{
    /// <summary>
    /// Preprocesses the arguments before they are parsed.
    /// </summary>
    /// <param name="args">The arguments to preprocess.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    public ValueTask Preprocess(IList<string> args, CancellationToken cancellationToken = default);
}
