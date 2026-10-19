using System.Globalization;
using Altinn.Authorization.CommandLine;
using Altinn.Authorization.CommandLine.GitHub.Actions;
using Altinn.Authorization.CommandLine.Results;
using Altinn.Authorization.RepoCtl.GitHub;
using Altinn.Authorization.RepoCtl.Model;
using Altinn.Authorization.RepoCtl.Utils;
using Microsoft.Extensions.Primitives;
using Spectre.Console;

namespace Altinn.Authorization.RepoCtl.Commands.Ci;

internal sealed class PromoteAppCommand(GitHubService github, ContainerService containers)
{
    public ICommandResult Invoke(
        AltinnRepository repository,
        GitHubRepositoryContext context,
        [Option("--verticals", Description = "Application vertical IDs, separated by whitespace.")] StringValues verticals,
        [Option("--environments", Description = "Target environment names, separated by whitespace.")] StringValues environments,
        [Option("--sha", Description = "Commit SHA of the built manifest artifacts; defaults to GITHUB_SHA.")] string? sha = null,
        [Option("--registry", Description = "Registry repository prefix for manifest images.")] string registry = "ghcr.io/altinn/altinn-auth")
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

        sha ??= Environment.GetEnvironmentVariable("GITHUB_SHA");
        // The source is both a GitHub ref and a Docker tag. Reject option-like and malformed tags.
        if (string.IsNullOrWhiteSpace(sha) || sha.Length > 128 || sha[0] is '.' or '-'
            || StringHelpers.ContainsInvalidImageTagCharacters(sha))
        {
            return Error("Specify a valid source image tag with --sha or GITHUB_SHA.");
        }

        registry = registry.TrimEnd('/');
        if (string.IsNullOrWhiteSpace(registry) || registry.StartsWith('-') || registry.Any(char.IsWhiteSpace))
        {
            return Error("Specify a valid registry repository prefix.");
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
            var toPromote = new List<string>();
            foreach (var app in apps)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (!Directory.Exists(Path.Combine(app.Directory.FullName, "manifests")))
                {
                    context.Console.StdErr.Write(Markup.FromInterpolated($"No manifests directory found for [cyan]{app.Id}[/], skipping promotion.\n"));
                    continue;
                }

                var shortSlug = app.Id.ToString("short-slug", CultureInfo.InvariantCulture);
                foreach (var environment in environments)
                {
                    await github.CreateDeployment(githubContext, sha, shortSlug, environment).Execute(context, cancellationToken);
                }

                toPromote.Add($"{registry}/{shortSlug}-manifests");
            }

            // if we failed to mark any for promotion, we stop here
            if (context.ReturnCode != 0)
            {
                return;
            }

            foreach (var manifest in toPromote)
            {
                await containers.PromoteManifests(repository, manifest, sha, environments).Execute(context, cancellationToken);
            }

            // if any failed, returncode should be non-0 at this point, so returning will signify a failure to the caller
        }
    }
}
