using System.Globalization;
using Altinn.Authorization.CommandLine;
using Altinn.Authorization.CommandLine.GitHub.Actions;
using Altinn.Authorization.CommandLine.Results;
using Altinn.Authorization.RepoCtl.GitHub;
using Altinn.Authorization.RepoCtl.Model;
using Altinn.Authorization.RepoCtl.Utils;
using Microsoft.Extensions.Primitives;
using Octokit;
using Spectre.Console;

namespace Altinn.Authorization.RepoCtl.Commands.Ci;

internal sealed class PromoteAppCommand(GitHubService github, ContainerService containers)
{
    public ICommandResult Invoke(
        AltinnRepository repository,
        GitHubRepositoryContext context,
        [Option("--verticals", Description = "Application vertical IDs, separated by whitespace.")] StringValues verticals,
        [Option("--environments", Description = "Target environment names, separated by whitespace.")] StringValues environments,
        [Option("--sha", DefaultFromEnvironment = "GITHUB_SHA", Description = "Commit SHA of the built manifest artifacts.")] string? sha = null,
        [Option("--registry", DefaultFromEnvironment = "REPOCTL_OCI_REGISTRY", Description = "Registry repository prefix for manifest images.")] string? registry = null)
    {
        var ids = verticals.Distinct(StringComparer.Ordinal).ToArray();
        var targets = environments.Select(value => value ?? string.Empty).Distinct(StringComparer.Ordinal).ToArray();

        if (ids.Length == 0)
        {
            return Error("At least one application vertical ID is required.");
        }

        if (targets.Length == 0 || targets.Any(static name => name.Length == 0 || StringHelpers.ContainsNonLowercaseAlphanumeric(name)))
        {
            return Error("Environments must use lowercase letters and digits.");
        }

        // The source is both a GitHub ref and a Docker tag. Reject option-like and malformed tags.
        if (string.IsNullOrWhiteSpace(sha) || sha.Length > 128 || sha[0] is '.' or '-'
            || StringHelpers.ContainsInvalidImageTagCharacters(sha))
        {
            return Error("Specify a valid source image tag with --sha or GITHUB_SHA.");
        }

        registry = registry?.TrimEnd('/');
        if (string.IsNullOrWhiteSpace(registry) || registry.StartsWith('-') || registry.Any(char.IsWhiteSpace))
        {
            return Error("Specify a valid registry repository prefix with --registry or REPOCTL_OCI_REGISTRY.");
        }

        // Resolve the entire selection before creating any deployments or promoting any images.
        var apps = new List<AltinnVertical>();
        foreach (var id in ids)
        {
            if (!AltinnVerticalId.TryParse(id, CultureInfo.InvariantCulture, out var verticalId)
                || verticalId.Kind != AltinnVerticalKind.Application
                || !repository.Verticals.TryGet(verticalId, out var vertical))
            {
                return Error($"Unknown application vertical: {id}");
            }

            apps.Add(vertical);
        }

        return new PromoteAppResult(repository, context, apps, targets, sha, registry, github, containers);
    }

    private static ICommandResult Error(string message)
        => new RenderResult(Markup.FromInterpolated($"[red]{message}[/]\n")) { ReturnCode = 1 };

    private sealed class PromoteAppResult(
        AltinnRepository repository,
        GitHubRepositoryContext githubContext,
        IReadOnlyList<AltinnVertical> apps,
        IReadOnlyList<string> environments,
        string sha,
        string registry,
        GitHubService github,
        ContainerService containers)
        : ICommandResult
    {
        public async Task Execute(CommandInvocationContext context, CancellationToken cancellationToken = default)
        {
            var toPromote = new List<(string Manifest, List<(Deployment Deployment, string Description)> Deployments)>();
            foreach (var app in apps)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (!Directory.Exists(Path.Combine(app.Directory.FullName, "manifests")))
                {
                    context.Console.StdErr.Write(Markup.FromInterpolated($"No manifests directory found for [cyan]{app.Id}[/], skipping promotion.\n"));
                    continue;
                }

                var shortSlug = app.Id.ToString("short-slug", CultureInfo.InvariantCulture);
                var deployments = new List<(Deployment Deployment, string Description)>();
                foreach (var environment in environments)
                {
                    var deployment = await github.CreateDeployment(githubContext, sha, shortSlug, environment).Execute(context, cancellationToken);
                    if (deployment is not null)
                    {
                        var description = $"Tagged {shortSlug}-manifests:{sha} as {environment}";
                        if (description.Length > 140)
                        {
                            description = description[..135] + "…";
                        }

                        deployments.Add((deployment, description));
                    }
                }

                toPromote.Add(($"{registry}/{shortSlug}-manifests", deployments));
            }

            // if we failed to create github deployment objects, we stop here
            if (context.ReturnCode != 0)
            {
                return;
            }

            foreach (var (manifest, deployments) in toPromote)
            {
                // Note: There is a theoretical race condition here, where the deployment can be picked up by flux
                // immediately after "PromoteManifests" is called before the deployment status is set to queued.
                // This could result in Flux posting back that the deployment is done before it is actually marked as queued.
                // Given that this is picked up by a separate github job, this race condition is unlikely to actually occur
                // in practice, so we choose to accept this minor risk.
                var child = context.CreateChildContext();
                await containers.PromoteManifests(repository, manifest, sha, environments).Execute(child, cancellationToken);

                if (child.ReturnCode == 0)
                {
                    foreach (var (deployment, description) in deployments)
                    {
                        await github.SetDeploymentStatus(githubContext, deployment, DeploymentState.Queued, description)
                            .Execute(child, cancellationToken);
                    }
                }

                if (child.ReturnCode != 0)
                {
                    context.ReturnCode = child.ReturnCode;
                }
            }
        }
    }
}
