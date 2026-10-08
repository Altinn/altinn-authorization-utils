using Octokit.Webhooks;

namespace Altinn.Authorization.CommandLine.GitHub.Actions;

/// <summary>
/// Represents the context of a GitHub Actions workflow, including the event name and payload.
/// </summary>
public sealed record GitHubContext
{
    /// <summary>
    /// Gets the name of the GitHub Actions event.
    /// </summary>
    public required string EventName { get; init; }

    /// <summary>
    /// Gets the GitHub Actions event payload.
    /// </summary>
    public required WebhookEvent? Event { get; init; }

    /// <summary>
    /// Gets the owner and repository name from <c>GITHUB_REPOSITORY</c>.
    /// </summary>
    public required GitHubRepositoryContext Repository { get; init; }

    /// <summary>
    /// Gets the commit SHA that triggered the workflow from <c>GITHUB_SHA</c>.
    /// </summary>
    /// <remarks>
    /// The commit depends on the event and can be a pull request merge commit rather than its head commit.
    /// </remarks>
    public required string Sha { get; init; }

    /// <summary>
    /// Gets the fully formed branch or tag ref that triggered the workflow from <c>GITHUB_REF</c>, if available.
    /// </summary>
    /// <remarks>
    /// For pull request events, this can be a merge ref such as <c>refs/pull/123/merge</c>.
    /// </remarks>
    public required string? Ref { get; init; }
}
