using System.Collections.Immutable;
using Altinn.Authorization.CommandLine;
using Altinn.Authorization.CommandLine.Console;
using Altinn.Authorization.CommandLine.Results;
using Altinn.Authorization.RepoCtl.Utils;
using CommunityToolkit.Diagnostics;
using Microsoft.Extensions.FileSystemGlobbing;
using Octokit;
using Spectre.Console;

namespace Altinn.Authorization.RepoCtl.GitHub;

internal sealed class GitHubService(AltinnRepositoryAccessor repositoryAccessor)
{
    public async Task<ICommandResult> UploadArtifactsToRelease(
        GitHubContext context,
        long releaseId,
        string glob)
    {
        var repository = await repositoryAccessor.GetRepository();

        var matcher = new Matcher();
        matcher.AddInclude(glob);

        var files = matcher.GetResultsInFullPath(repository.RootDirectory.FullName).ToImmutableArray();

        if (files.IsEmpty)
        {
            return new RenderResult(Markup.FromInterpolated($"No files found in [blue]{repository.RootDirectory.FullName}[/] that matched glob [cyan]\"{glob}\"[/].\n"));
        }

        var githubClient = new GitHubClient(new ProductHeaderValue("repoctl"));
        var tokenString = Environment.GetEnvironmentVariable("GITHUB_TOKEN");
        if (string.IsNullOrEmpty(tokenString))
        {
            ThrowHelper.ThrowInvalidOperationException("GITHUB_TOKEN environment variable is not set.");
        }

        githubClient.Credentials = new Credentials(tokenString);
        var release = await githubClient.Repository.Release.Get(context.RepositoryOwner, context.RepositoryName, releaseId);

        if (release is null)
        {
            ThrowHelper.ThrowInvalidOperationException($"Release with ID {releaseId} not found in repository {context.RepositoryOwner}/{context.RepositoryName}.");
        }

        return new UploadPackagesToReleaseResult(githubClient, release, files);
    }

    private sealed class UploadPackagesToReleaseResult(GitHubClient githubClient, Release release, ImmutableArray<string> files)
        : ICommandResult
    {
        public async Task Execute(CommandInvocationContext context, CancellationToken cancellationToken = default)
        {
            await context.Console.StdErr.Status()
                .StartAsync("Uploading packages to GitHub release...", async status =>
                {
                    await UploadPackages(context.Console, status, cancellationToken);
                });
        }

        private async Task UploadPackages(IConsole console, StatusContext status, CancellationToken cancellationToken)
        {
            foreach (var file in files)
            {
                cancellationToken.ThrowIfCancellationRequested();

                await using var fs = File.OpenRead(file);
                var fileName = Path.GetFileName(file);
                status.Status($"Uploading [blue]{fileName}[/]...");

                var assetUplod = new ReleaseAssetUpload(fileName, contentType: "application/octet-stream", fs, timeout: null);
                var asset = await githubClient.Repository.Release.UploadAsset(release, assetUplod, cancellationToken);

                console.Write(Markup.FromInterpolated($"Uploaded [blue]{fileName}[/] to release [cyan]{release.TagName}[/] as asset [green]{asset.Name}[/].\n"));
            }
        }
    }

}
