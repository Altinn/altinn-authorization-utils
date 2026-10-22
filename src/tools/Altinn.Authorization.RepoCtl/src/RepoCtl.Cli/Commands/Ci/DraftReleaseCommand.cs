using System.Globalization;
using Altinn.Authorization.CommandLine;
using Altinn.Authorization.CommandLine.GitHub.Actions;
using Altinn.Authorization.CommandLine.Results;
using Altinn.Authorization.CommandLine.Shell;
using Altinn.Authorization.RepoCtl.Changelog;
using Altinn.Authorization.RepoCtl.GitHub;
using Altinn.Authorization.RepoCtl.Model;
using Altinn.Authorization.RepoCtl.Utils;
using Octokit;
using Spectre.Console;

namespace Altinn.Authorization.RepoCtl.Commands.Ci;

internal sealed class DraftReleaseCommand(GitHubService github, IChangelogGenerator changelog, TimeProvider timeProvider)
{
    public ICommandResult Invoke(
        AltinnRepository repository,
        AltinnVertical vertical,
        GitHubRepositoryContext context,
        [Option("--version", Description = "Calendar version override; defaults to today's UTC date.")] string? version = null,
        [Option("--sha", DefaultFromEnvironment = "GITHUB_SHA", Description = "Commit or ref to release.")] string? sha = null)
    {
        if (vertical.Kind != AltinnVerticalKind.Application)
        {
            return Error($"Vertical {vertical.Id} is not an application.");
        }

        version = string.IsNullOrEmpty(version)
            ? timeProvider.GetUtcNow().ToString("yyyy.MM.dd", CultureInfo.InvariantCulture)
            : version;

        if (!CalendarVersion.TryValidate(version, out var error))
        {
            return Error(error);
        }

        if (string.IsNullOrWhiteSpace(sha))
        {
            return Error("Specify the source commit with --sha or GITHUB_SHA.");
        }

        var prefix = vertical.Id.ToString("tag-prefix", CultureInfo.InvariantCulture);
        return new DraftReleaseResult(repository, vertical, context, prefix, version, sha, github, changelog);
    }

    private static ICommandResult Error(string message)
        => new RenderResult(Markup.FromInterpolated($"[red]{message}[/]\n")) { ReturnCode = 1 };

    private sealed class DraftReleaseResult(
        AltinnRepository repository,
        AltinnVertical vertical,
        GitHubRepositoryContext githubContext,
        string prefix,
        string version,
        string source,
        GitHubService github,
        IChangelogGenerator changelog)
        : ICommandResult
    {
        public async Task Execute(CommandInvocationContext context, CancellationToken cancellationToken = default)
        {
            var tag = $"{prefix}-v{version}";
            try
            {
                var sha = await github.ResolveCommit(githubContext, source, cancellationToken);
                if (await github.TagExists(githubContext, tag, cancellationToken))
                {
                    await Error($"Tag already exists: {tag}").Execute(context, cancellationToken);
                    return;
                }

                var drafts = await github.GetDraftReleaseTags(githubContext, $"{prefix}-v", cancellationToken);
                if (drafts.Count > 0)
                {
                    await Error($"Existing draft release(s) for {vertical.Id}: {string.Join(", ", drafts)}").Execute(context, cancellationToken);
                    return;
                }

                var notes = await changelog.Generate(repository, vertical, tag, sha, cancellationToken);
                var release = await github.CreateDraftRelease(githubContext, tag, sha, $"{prefix}: {version}", notes, cancellationToken);

                context.Console.StdErr.Write(Markup.FromInterpolated($"Created draft release [green]{tag}[/]: {release.HtmlUrl}\n"));
            }
            catch (ApiException ex)
            {
                await Error($"Failed to create draft release {tag}: {ex.Message}")
                    .Execute(context, cancellationToken);
            }
            catch (ProcessFailedException ex)
            {
                context.Console.StdErr.Write(Markup.FromInterpolated($"[red]Changelog generation failed for {tag}.[/]\n"));
                context.ReturnCode = ex.Process.ExitCode;
            }
        }
    }
}
