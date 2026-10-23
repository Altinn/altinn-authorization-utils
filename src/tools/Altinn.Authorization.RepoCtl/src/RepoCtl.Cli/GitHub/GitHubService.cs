using System.Collections.Immutable;
using Altinn.Authorization.CommandLine;
using Altinn.Authorization.CommandLine.GitHub.Actions;
using Altinn.Authorization.CommandLine.Results;
using Altinn.Authorization.RepoCtl.Retry;
using Altinn.Authorization.RepoCtl.Utils;
using Microsoft.Extensions.FileSystemGlobbing;
using Octokit;
using Spectre.Console;

namespace Altinn.Authorization.RepoCtl.GitHub;

internal sealed class GitHubService(AltinnRepositoryAccessor repositoryAccessor, GitHubClient githubClient)
{
    public async Task AddPullRequestLabels(
        GitHubRepositoryContext repository,
        int number,
        IReadOnlyList<GitHubLabel> labels,
        CancellationToken cancellationToken = default)
    {
        if (labels.Count == 0)
        {
            return;
        }

        cancellationToken.ThrowIfCancellationRequested();
        var repositoryLabels = await githubClient.Issue.Labels.GetAllForRepository(
            repository.RepositoryOwner,
            repository.RepositoryName,
            new ApiOptions { PageSize = 100 }).WaitAsync(cancellationToken);
        var existingLabels = repositoryLabels.Select(label => label.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var label in labels)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (existingLabels.Contains(label.Name))
            {
                continue;
            }

            try
            {
                await githubClient.Issue.Labels.Create(repository.RepositoryOwner, repository.RepositoryName, new NewLabel(label.Name, label.Color.TrimStart('#'))
                {
                    Description = label.Description,
                }).WaitAsync(cancellationToken);
            }
            catch (ApiValidationException)
            {
                // Another run may have created the label after our lookup.
                await githubClient.Issue.Labels.Get(repository.RepositoryOwner, repository.RepositoryName, label.Name)
                    .WaitAsync(cancellationToken);
            }

            existingLabels.Add(label.Name);
        }

        cancellationToken.ThrowIfCancellationRequested();
        await githubClient.Issue.Labels.AddToIssue(repository.RepositoryOwner, repository.RepositoryName, number, labels.Select(label => label.Name).ToArray())
            .WaitAsync(cancellationToken);
    }

    public async Task<string> ResolveCommit(GitHubRepositoryContext context, string reference, CancellationToken cancellationToken = default)
    {
        var commit = await githubClient.Repository.Commit.Get(context.RepositoryOwner, context.RepositoryName, reference)
            .WaitAsync(cancellationToken);

        return commit.Sha;
    }

    public async Task<bool> TagExists(GitHubRepositoryContext context, string tag, CancellationToken cancellationToken = default)
    {
        try
        {
            await githubClient.Git.Reference.Get(context.RepositoryOwner, context.RepositoryName, $"tags/{tag}")
                .WaitAsync(cancellationToken);

            return true;
        }
        catch (NotFoundException)
        {
            return false;
        }
    }

    public async Task<IReadOnlyList<string>> GetDraftReleaseTags(GitHubRepositoryContext context, string prefix, CancellationToken cancellationToken = default)
    {
        var releases = await githubClient.Repository.Release
            .GetAll(context.RepositoryOwner, context.RepositoryName, new ApiOptions { PageSize = 100 })
            .WaitAsync(cancellationToken);

        return releases.Where(release => release.Draft && release.TagName.StartsWith(prefix, StringComparison.Ordinal))
            .Select(static release => release.TagName)
            .ToList();
    }

    public Task<Release> CreateDraftRelease(GitHubRepositoryContext context, string tag, string sha, string title, string notes, CancellationToken cancellationToken = default)
        => githubClient.Repository.Release.Create(context.RepositoryOwner, context.RepositoryName, new NewRelease(tag)
        {
            Draft = true,
            TargetCommitish = sha,
            Name = title,
            Body = notes,
            MakeLatest = MakeLatestQualifier.False,
        })
        .WaitAsync(cancellationToken);

    public ICommandResult CreateDeployment(GitHubRepositoryContext context, string sha, string shortSlug, string environment)
        => new CreateDeploymentResult(githubClient, context, sha, shortSlug, environment);

    public async Task<ICommandResult> UploadArtifactsToRelease(
        GitHubRepositoryContext context,
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

        var release = await githubClient.Repository.Release.Get(context.RepositoryOwner, context.RepositoryName, releaseId);
        return new UploadPackagesToReleaseResult(githubClient, release, files);
    }

    private sealed class CreateDeploymentResult(
        GitHubClient githubClient,
        GitHubRepositoryContext repository,
        string sha,
        string shortSlug,
        string environment)
        : ICommandResult
    {
        public async Task Execute(CommandInvocationContext context, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var deploymentEnvironment = $"{shortSlug}/{environment}";
            var request = new NewDeployment(sha)
            {
                Environment = deploymentEnvironment,
                AutoMerge = false,
                RequiredContexts = [],
                ProductionEnvironment = environment == "prod",
                Description = "OCI artifact promotion requested; awaiting promotion and Flux reconciliation",
            };

            try
            {
                var deployment = await githubClient.Repository.Deployment.Create(repository.RepositoryOwner, repository.RepositoryName, request);
                context.Console.StdErr.Write(Markup.FromInterpolated($"Created pending deployment [green]{deployment.Id}[/] for [cyan]{deploymentEnvironment}[/].\n"));
            }
            catch (ApiException ex)
            {
                context.Console.StdErr.Write(Markup.FromInterpolated($"[red]Failed to create deployment for {deploymentEnvironment}: {ex.Message}[/]\n"));
                context.ReturnCode = 1;
            }
        }
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
