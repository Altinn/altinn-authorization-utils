using System.Runtime.CompilerServices;
using Altinn.Authorization.CommandLine.GitHub.Actions;
using Altinn.Authorization.CommandLine.Shell;
using CommunityToolkit.Diagnostics;
using Octokit;
using Octokit.Webhooks.Events;
using GitHubContext = Altinn.Authorization.CommandLine.GitHub.Actions.GitHubContext;
using PullRequestEvent = Octokit.Webhooks.Events.PullRequestEvent;
using PullRequestReviewEvent = Octokit.Webhooks.Events.PullRequestReviewEvent;

namespace Altinn.Authorization.RepoCtl.GitHub;

internal sealed class GitHubChangedFilesService(IGitHubActionsService actions, GitHubClient client)
    : IChangedFilesService
{
    public IAsyncEnumerable<string> GetChangedFiles(CancellationToken cancellationToken)
    {
        var context = actions.Context;

        if (context is null)
        {
            ThrowHelper.ThrowInvalidOperationException("GitHub context is not available.");
        }

        return context.EventName switch
        {
            Octokit.Webhooks.WebhookEventType.PullRequest or
            Octokit.Webhooks.WebhookEventType.PullRequestReview or
            Octokit.Webhooks.WebhookEventType.PullRequestReviewComment => GetPullRequestChangedFiles(context, cancellationToken),
            "pull_request_target" => ThrowHelper.ThrowNotSupportedException<IAsyncEnumerable<string>>("pull_request_target currently not supported."),
            _ => GetGitChangedFiles(context, cancellationToken),
        };
    }

    private async IAsyncEnumerable<string> GetPullRequestChangedFiles(
        GitHubContext context,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var number = context.Event switch
        {
            PullRequestEvent pullRequest => pullRequest.Number,
            PullRequestReviewEvent review => review.PullRequest.Number,
            PullRequestReviewCommentEvent comment => comment.PullRequest.Number,
            _ => ThrowHelper.ThrowInvalidOperationException<long>("The GitHub event does not contain a pull request."),
        };

        var seen = new HashSet<string>(StringComparer.Ordinal);
        for (var page = 1; ; page++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var files = await client.PullRequest.Files(
                context.Repository.RepositoryOwner,
                context.Repository.RepositoryName,
                checked((int)number),
                new ApiOptions { PageSize = 100, PageCount = 1, StartPage = page }).WaitAsync(cancellationToken);

            foreach (var file in files)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (seen.Add(file.FileName))
                {
                    yield return file.FileName;
                }

                // Moving a file between verticals affects both the source and destination.
                if (file.Status == "renamed" && !string.IsNullOrEmpty(file.PreviousFileName) && seen.Add(file.PreviousFileName))
                {
                    yield return file.PreviousFileName;
                }
            }

            if (files.Count < 100)
            {
                yield break;
            }

            // GitHub caps this endpoint at 3,000 files. Do not return an incomplete change set.
            if (page == 30)
            {
                ThrowHelper.ThrowInvalidOperationException("The pull request file list reached GitHub's 3,000-file limit. Use Git to detect changes for this pull request.");
            }
        }
    }

    private static async IAsyncEnumerable<string> GetGitChangedFiles(
        GitHubContext context,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var head = context.Sha;
        string[] arguments;
        if (context.Event is PushEvent push)
        {
            if (push.Deleted)
            {
                ThrowHelper.ThrowInvalidOperationException("Change detection is not supported for a deleted Git ref.");
            }

            head = push.After;
            await EnsureCommit(head, cancellationToken);
            var defaultBranch = push.Repository?.DefaultBranch;
            if (!string.IsNullOrEmpty(defaultBranch) && push.Ref != $"refs/heads/{defaultBranch}")
            {
                // Every feature-branch push includes all changes introduced by the branch.
                var mergeBase = await GetMergeBase(defaultBranch, head, cancellationToken);
                arguments = ["diff", "--no-renames", "--name-only", "-z", mergeBase, head, "--"];
            }
            else if (IsNullSha(push.Before))
            {
                // An initial push of the default branch has no previous tree.
                arguments = ["ls-tree", "-r", "--name-only", "-z", head];
            }
            else
            {
                await EnsureCommit(push.Before, cancellationToken);
                arguments = ["diff", "--no-renames", "--name-only", "-z", push.Before, head, "--"];
            }
        }
        else if (context.Event is MergeGroupEvent mergeGroup)
        {
            var group = mergeGroup.MergeGroup;
            await EnsureCommit(group.BaseSha, cancellationToken);
            await EnsureCommit(group.HeadSha, cancellationToken);
            arguments = ["diff", "--no-renames", "--name-only", "-z", group.BaseSha, group.HeadSha, "--"];
        }
        else
        {
            await EnsureCommit(head, cancellationToken);
            var defaultBranch = context.Event?.Repository?.DefaultBranch;
            if (!string.IsNullOrEmpty(defaultBranch) && context.Ref != $"refs/heads/{defaultBranch}")
            {
                var mergeBase = await GetMergeBase(defaultBranch, head, cancellationToken);
                arguments = ["diff", "--no-renames", "--name-only", "-z", mergeBase, head, "--"];
            }
            else
            {
                // A shallow boundary looks like a root commit to diff-tree. Fetch its parent first.
                var shallow = await RunGit(["rev-parse", "--is-shallow-repository"], cancellationToken);
                if (shallow.Output.Trim() == "true")
                {
                    await RunGit(["fetch", "--no-tags", "--depth=2", "origin", head], cancellationToken);
                }

                arguments = ["diff-tree", "--root", "--no-commit-id", "-r", "-m", "--no-renames", "--name-only", "-z", head, "--"];
            }
        }

        var result = await RunGit(arguments, cancellationToken);
        foreach (var path in result.Output.Split('\0', StringSplitOptions.RemoveEmptyEntries).Distinct(StringComparer.Ordinal))
        {
            cancellationToken.ThrowIfCancellationRequested();
            yield return path;
        }
    }

    private static bool IsNullSha(string sha)
        => sha.Length > 0 && sha.All(c => c == '0');

    private static async Task<string> GetMergeBase(string defaultBranch, string head, CancellationToken cancellationToken)
    {
        var baseRef = $"refs/remotes/origin/{defaultBranch}";
        await RunGit(["fetch", "--no-tags", "origin", $"+refs/heads/{defaultBranch}:{baseRef}"], cancellationToken);
        var mergeBase = await RunGit(["merge-base", baseRef, head], cancellationToken, allowFailure: true);
        if (mergeBase.ExitCode != 0)
        {
            var shallow = await RunGit(["rev-parse", "--is-shallow-repository"], cancellationToken);
            if (shallow.Output.Trim() != "true")
            {
                ThrowHelper.ThrowInvalidOperationException("No merge base exists between the workflow commit and the default branch.");
            }

            for (var iteration = 0; iteration < 10 && mergeBase.ExitCode != 0; iteration++)
            {
                await RunGit(["fetch", "--no-tags", "--deepen=50", "origin"], cancellationToken);
                mergeBase = await RunGit(["merge-base", baseRef, head], cancellationToken, allowFailure: true);
            }

            if (mergeBase.ExitCode != 0)
            {
                ThrowHelper.ThrowInvalidOperationException("No merge base was found between the workflow commit and the default branch after 10 fetches deepening history by 50 commits each.");
            }
        }

        return mergeBase.Output.Trim();
    }

    private static async Task EnsureCommit(string sha, CancellationToken cancellationToken)
    {
        var result = await RunGit(["cat-file", "-e", $"{sha}^{{commit}}"], cancellationToken, allowFailure: true);
        if (result.ExitCode != 0)
        {
            await RunGit(["fetch", "--no-tags", "--depth=1", "origin", sha], cancellationToken);
            await RunGit(["cat-file", "-e", $"{sha}^{{commit}}"], cancellationToken);
        }
    }

    private static async Task<(int ExitCode, string Output)> RunGit(
        string[] arguments,
        CancellationToken cancellationToken,
        bool allowFailure = false)
    {
        try
        {
            var result = await Command.Create("git", arguments)
                .WithWorkingDirectory(Environment.GetEnvironmentVariable("GITHUB_WORKSPACE") ?? Environment.CurrentDirectory)
                .ExecuteCaptured(cancellationToken);
            return (result.ExitCode, result.StandardOutput);
        }
        catch (ProcessFailedException exception) when (allowFailure)
        {
            // Commit-existence and merge-base probes use non-zero exits to report absence.
            using var process = exception.Process;
            return (process.ExitCode, string.Empty);
        }
    }
}
