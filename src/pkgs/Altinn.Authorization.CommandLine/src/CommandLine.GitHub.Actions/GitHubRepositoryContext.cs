namespace Altinn.Authorization.CommandLine.GitHub.Actions;

/// <summary>
/// Represents the context of a GitHub repository, including its owner and name.
/// </summary>
public sealed record GitHubRepositoryContext
{
    /// <summary>
    /// Gets the owner of the GitHub repository.
    /// </summary>
    public required string RepositoryOwner { get; init; }

    /// <summary>
    /// Gets the name of the GitHub repository.
    /// </summary>
    public required string RepositoryName { get; init; }
}
