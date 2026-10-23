using Altinn.Authorization.CommandLine;
using Altinn.Authorization.CommandLine.GitHub.Actions;
using Altinn.Authorization.CommandLine.Results;
using Altinn.Authorization.RepoCtl.GitHub;
using Altinn.Authorization.RepoCtl.Model;
using Octokit;
using Octokit.Webhooks.Events;
using Spectre.Console;

namespace Altinn.Authorization.RepoCtl.Commands.Ci;

internal sealed class PrAutolabelCommand(IChangedFilesService changedFiles, GitHubService github, IGitHubActionsService actions)
{
    public async Task<ICommandResult> Invoke(
        AltinnRepository repository,
        GitHubRepositoryContext context,
        [Option("--pr", Description = "Pull request number; defaults to the workflow's PullRequestEvent.")] int? pr = null,
        CancellationToken cancellationToken = default)
    {
        var number = pr ?? GetPullRequestNumber(actions.Context);
        if (number is null or <= 0)
        {
            return new RenderResult(new Markup("[red]Specify a positive --pr number or run with a PullRequestEvent.[/]\n")) { ReturnCode = 1 };
        }

        try
        {
            var paths = new HashSet<string>(StringComparer.Ordinal);
            await foreach (var path in changedFiles.GetPullRequestChangedFiles(context, number.Value, cancellationToken).WithCancellation(cancellationToken))
            {
                paths.Add(path);
            }

            var labels = GetLabels(repository, paths);
            await github.AddPullRequestLabels(context, number.Value, labels, cancellationToken);
            return new RenderResult(Markup.FromInterpolated($"Added {labels.Count} vertical labels to PR #{number.Value}.\n"));
        }
        catch (Exception ex) when (ex is ApiException or InvalidOperationException)
        {
            return new RenderResult(Markup.FromInterpolated($"[red]PR autolabeling failed: {ex.Message}[/]\n")) { ReturnCode = 1 };
        }
    }

    internal static int? GetPullRequestNumber(GitHubContext? context)
        => context?.Event switch
        {
            PullRequestEvent pullRequest => checked((int)pullRequest.Number),
            _ => null,
        };

    internal static IReadOnlyList<GitHubLabel> GetLabels(AltinnRepository repository, IReadOnlySet<string> paths)
    {
        var labels = new SortedDictionary<string, GitHubLabel>(StringComparer.OrdinalIgnoreCase);
        foreach (var vertical in repository.Verticals)
        {
            var prefix = $"{vertical.RelPath.TrimEnd('/')}/";
            if (!paths.Any(path => path.StartsWith(prefix, StringComparison.Ordinal)))
            {
                continue;
            }

            Add(vertical);
            if (vertical.Kind == AltinnVerticalKind.Library)
            {
                AddRange(vertical.Dependents.OfKind(AltinnVerticalKind.Application));
            }
        }

        return [.. labels.Values];

        void Add(AltinnVertical vertical)
        {
            var settings = repository.Config.Labels;
            var (label, description) = vertical.Kind switch
            {
                var kind when kind == AltinnVerticalKind.Application => (settings.App, "App"),
                var kind when kind == AltinnVerticalKind.Library => (settings.Library, "Library"),
                var kind when kind == AltinnVerticalKind.Package => (settings.Package, "Package"),
                var kind when kind == AltinnVerticalKind.Tool => (settings.Tool, "Tool"),
                _ => throw new InvalidOperationException($"Unsupported vertical kind '{vertical.Kind}'."),
            };

            var name = $"{label.Prefix}{vertical.Id.ToString("short-slug", null)}";
            labels.TryAdd(name, new GitHubLabel(name, label.Color, $"{description}: {vertical.DisplayName}"));
        }

        void AddRange(IEnumerable<AltinnVertical> verticals)
        {
            foreach (var vertical in verticals)
            {
                Add(vertical);
            }
        }
    }
}
