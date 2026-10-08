using System.Globalization;
using Altinn.Authorization.CommandLine.Console;
using Altinn.Authorization.CommandLine.GitHub.Actions;
using Altinn.Authorization.RepoCtl.Model;
using Spectre.Console;

namespace Altinn.Authorization.RepoCtl.Commands.Ci;

internal sealed partial class ExportVerticalCommand(IGitHubActionsService actions)
{
    public async Task Invoke(
        AltinnVertical vertical,
        IConsole console,
        CancellationToken cancellationToken = default)
    {
        if (!actions.IsGitHubActions)
        {
            console.MarkupLineInterpolated($"[red]This command can only be run within GitHub Actions.[/]");
        }

        await actions.SetEnvironmentVariable("VERTICAL_DIR", vertical.Directory.FullName, cancellationToken);
        await actions.SetEnvironmentVariable("VERTICAL_KIND", vertical.Kind.ToString(), cancellationToken);
        await actions.SetEnvironmentVariable("VERTICAL_NAME", vertical.Id.Name, cancellationToken);
        await actions.SetEnvironmentVariable("VERTICAL_ID", vertical.Id.ToString(null, formatProvider: CultureInfo.InvariantCulture), cancellationToken);
        await actions.SetEnvironmentVariable("VERTICAL_DISPLAY_NAME", vertical.DisplayName, cancellationToken);
        await actions.SetEnvironmentVariable("VERTICAL_SLUG", vertical.Id.ToString("slug", formatProvider: CultureInfo.InvariantCulture), cancellationToken);
        await actions.SetEnvironmentVariable("VERTICAL_SHORT_SLUG", vertical.Id.ToString("short-slug", formatProvider: CultureInfo.InvariantCulture), cancellationToken);
        await actions.SetEnvironmentVariable("VERTICAL_VERSION", vertical.Version.ToString(), cancellationToken);
    }
}
