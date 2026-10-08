namespace Altinn.Authorization.RepoCtl.GitHub;

/// <summary>
/// Service for retrieving changed files from a CI context.
/// </summary>
public interface IChangedFilesService
{
    /// <summary>
    /// Retrieves the list of changed files.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An asynchronous stream of changed file paths.</returns>
    IAsyncEnumerable<string> GetChangedFiles(CancellationToken cancellationToken);
}
