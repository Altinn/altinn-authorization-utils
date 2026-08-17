namespace Altinn.Authorization.CommandLine.PreProcessing;

/// <summary>
/// Defines a resolver for URIs used in command line arguments.
/// </summary>
public interface IArgumentUriResolver
{
    /// <summary>
    /// Tries to resolve the specified URI to a corresponding value.
    /// </summary>
    /// <param name="uri">The URI to resolve.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The resolved value, or null if it could not be resolved.</returns>
    ValueTask<string?> TryResolve(Uri uri, CancellationToken cancellationToken = default);
}
