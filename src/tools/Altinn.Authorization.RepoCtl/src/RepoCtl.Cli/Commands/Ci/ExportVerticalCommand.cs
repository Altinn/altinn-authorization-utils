using System.Globalization;
using Altinn.Authorization.CommandLine.Console;
using Altinn.Authorization.CommandLine.GitHub.Actions;
using Altinn.Authorization.RepoCtl.Model;
using Spectre.Console;

namespace Altinn.Authorization.RepoCtl.Commands.Ci;

internal sealed partial class ExportVerticalCommand(IGitHubActionsService actions)
{
    public async Task Invoke(
        AltinnRepository repository,
        AltinnVertical vertical,
        IConsole console,
        CancellationToken cancellationToken = default)
    {
        if (!actions.IsGitHubActions)
        {
            console.MarkupLineInterpolated($"[red]This command can only be run within GitHub Actions.[/]");
        }

        var libDeps = vertical.AllDependencies.OfKind(AltinnVerticalKind.Library);
        var changelogPaths = new List<string>();
        changelogPaths.Add(RelPath(repository, vertical));

        foreach (var libDep in libDeps)
        {
            changelogPaths.Add(RelPath(repository, libDep));
        }

        var changelogPathsString = string.Join(" ", changelogPaths);

        await Export("VERTICAL_DIR", "dir", vertical.Directory.FullName, cancellationToken);
        await Export("VERTICAL_KIND", "kind", vertical.Kind.ToString(), cancellationToken);
        await Export("VERTICAL_NAME", "name", vertical.Id.Name, cancellationToken);
        await Export("VERTICAL_ID", "id", vertical.Id.ToString(null, formatProvider: CultureInfo.InvariantCulture), cancellationToken);
        await Export("VERTICAL_DISPLAY_NAME", "displayName", vertical.DisplayName, cancellationToken);
        await Export("VERTICAL_SLUG", "slug", vertical.Id.ToString("slug", formatProvider: CultureInfo.InvariantCulture), cancellationToken);
        await Export("VERTICAL_SHORT_SLUG", "shortSlug", vertical.Id.ToString("short-slug", formatProvider: CultureInfo.InvariantCulture), cancellationToken);
        await Export("VERTICAL_TAG_PREFIX", "tagPrefix", vertical.Id.ToString("tag-prefix", formatProvider: CultureInfo.InvariantCulture), cancellationToken);
        await Export("VERTICAL_VERSION", "version", vertical.Version.ToString(), cancellationToken);

        await Export("VERTICAL_CHANGELOG_PATHS", "changelogPaths", changelogPathsString, cancellationToken);

        async Task Export(string environmentName, string outputName, string value, CancellationToken cancellationToken)
        {
            await actions.SetEnvironmentVariable(environmentName, value, cancellationToken);
            await actions.SetOutput(outputName, value, cancellationToken);
        }

        static string RelPath(AltinnRepository repository, AltinnVertical vertical)
            => Path.GetRelativePath(repository.RootDirectory.FullName, vertical.Directory.FullName);
    }
}
