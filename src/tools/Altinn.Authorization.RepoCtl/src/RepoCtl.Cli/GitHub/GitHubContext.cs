namespace Altinn.Authorization.RepoCtl.GitHub;

internal sealed record GitHubContext
{
    public required string RepositoryOwner { get; init; }

    public required string RepositoryName { get; init; }
}
