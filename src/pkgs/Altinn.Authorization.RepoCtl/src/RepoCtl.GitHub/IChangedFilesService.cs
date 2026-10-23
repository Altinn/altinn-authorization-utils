using Altinn.Authorization.CommandLine.GitHub.Actions;

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

    /// <summary>Retrieves changed paths for a specific pull request, including both sides of renames.</summary>
    /// <param name="repository">The GitHub repository containing the pull request.</param>
    /// <param name="number">The pull request number.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An asynchronous stream of changed file paths.</returns>
    IAsyncEnumerable<string> GetPullRequestChangedFiles(GitHubRepositoryContext repository, int number, CancellationToken cancellationToken);
}
