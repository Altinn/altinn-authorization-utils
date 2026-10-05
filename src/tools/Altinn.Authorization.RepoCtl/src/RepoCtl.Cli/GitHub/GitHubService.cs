using System.Collections.Immutable;
using Altinn.Authorization.CommandLine;
using Altinn.Authorization.CommandLine.Results;
using Altinn.Authorization.RepoCtl.Retry;
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
                    await UploadPackages(context, status, cancellationToken);
                });
        }

        private async Task UploadPackages(CommandInvocationContext context, StatusContext status, CancellationToken cancellationToken)
        {
            var console = context.Console;

            foreach (var file in files)
            {
                var fileName = Path.GetFileName(file);

                status.Status = $"Uploading [blue]{fileName}[/] to release [cyan]{release.TagName}[/]...";
                var uploadResult = new UploadPackageResult(githubClient, release, file);
                await uploadResult.Retry(5).Execute(context, cancellationToken);

                if (context.ReturnCode is not 0)
                {
                    return;
                }
            }
        }
    }

    private sealed class UploadPackageResult(GitHubClient githubClient, Release release, string file)
        : ICommandResult
    {
        public async Task Execute(CommandInvocationContext context, CancellationToken cancellationToken = default)
        {
            var fileName = Path.GetFileName(file);
            await using var fs = File.OpenRead(file);
            var assetUpload = new ReleaseAssetUpload(fileName, contentType: "application/octet-stream", fs, timeout: null);

            try
            {
                var asset = await githubClient.Repository.Release.UploadAsset(release, assetUpload, cancellationToken);

                context.Console.StdErr.Write(Markup.FromInterpolated($"Uploaded [blue]{fileName}[/] to release [cyan]{release.TagName}[/] as asset [green]{asset.Name}[/].\n"));
            }
            catch (ApiException)
            {
                context.Console.StdErr.Write(Markup.FromInterpolated($"[red]Failed to upload [blue]{fileName}[/].[/]\n"));
                context.ReturnCode = 1;
            }
        }
    }
}
