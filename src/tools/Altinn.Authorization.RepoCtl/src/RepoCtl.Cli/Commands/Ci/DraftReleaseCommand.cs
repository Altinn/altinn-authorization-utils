using System.Globalization;
using System.Text.RegularExpressions;
using Altinn.Authorization.CommandLine;
using Altinn.Authorization.CommandLine.GitHub.Actions;
using Altinn.Authorization.CommandLine.Results;
using Altinn.Authorization.CommandLine.Shell;
using Altinn.Authorization.RepoCtl.Changelog;
using Altinn.Authorization.RepoCtl.GitHub;
using Altinn.Authorization.RepoCtl.Model;
using Octokit;
using Spectre.Console;

namespace Altinn.Authorization.RepoCtl.Commands.Ci;

internal sealed partial class DraftReleaseCommand(GitHubService github, IChangelogGenerator changelog, TimeProvider timeProvider)
{
    [GeneratedRegex(@"^[0-9]{4}\.[0-9]{2}\.[0-9]{2}(\.[1-9][0-9]{2})?$", RegexOptions.CultureInvariant)]
    private static partial Regex CalendarVersionPattern { get; }

    public ICommandResult Invoke(
        AltinnRepository repository,
        AltinnVertical vertical,
        GitHubRepositoryContext context,
        [Option("--version", Description = "Calendar version override; defaults to today's UTC date.")] string? version = null,
        [Option("--sha", Description = "Commit or ref to release; defaults to GITHUB_SHA.")] string? sha = null)
    {
        if (vertical.Kind != AltinnVerticalKind.Application)
        {
            return Error($"Vertical {vertical.Id} is not an application.");
        }

        version = string.IsNullOrEmpty(version)
            ? timeProvider.GetUtcNow().ToString("yyyy.MM.dd", CultureInfo.InvariantCulture)
            : version;

        var versionMatch = CalendarVersionPattern.Match(version);
        if (!versionMatch.Success || versionMatch.Length != version.Length)
        {
            return Error($"Invalid calendar version: {version}");
        }

        if (!DateOnly.TryParseExact(version.AsSpan(0, 10), "yyyy.MM.dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
        {
            return Error($"Invalid calendar date in version: {version}");
        }

        sha ??= Environment.GetEnvironmentVariable("GITHUB_SHA");
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
