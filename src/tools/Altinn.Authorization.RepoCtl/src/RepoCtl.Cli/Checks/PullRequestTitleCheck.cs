using System.Text.RegularExpressions;
using Altinn.Authorization.CommandLine.GitHub.Actions;
using Altinn.Authorization.RepoCtl.Model;
using Altinn.Authorization.RepoCtl.Model.Checks;
using Octokit.Webhooks.Events;

namespace Altinn.Authorization.RepoCtl.Checks;

internal sealed partial class PullRequestTitleCheck(IGitHubActionsService actions)
    : IRepositoryCheck
{
    public string CheckId => "pull-request-title";

    public string CheckDisplayName => "Pull request title";

    public IAsyncEnumerable<Diagnostic> Check(AltinnRepository repository, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (actions.Context?.Event is PullRequestEvent pullRequest
            && !TitlePattern.IsMatch(pullRequest.PullRequest.Title))
        {
            return new[] { Diagnostics.InvalidPullRequestTitle() }.ToAsyncEnumerable();
        }

        return AsyncEnumerable.Empty<Diagnostic>();
    }

    [GeneratedRegex(@"\A[a-z]+(?:\((?:[a-z][a-z]*-)*[a-z]+\))?!?: [^\s\r\n][^\r\n]*\z", RegexOptions.CultureInvariant)]
    private static partial Regex TitlePattern { get; }
}
