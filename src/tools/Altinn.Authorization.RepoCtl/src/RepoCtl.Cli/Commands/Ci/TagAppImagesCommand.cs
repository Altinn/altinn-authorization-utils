using Altinn.Authorization.CommandLine;
using Altinn.Authorization.CommandLine.Results;
using Altinn.Authorization.CommandLine.Shell;
using Altinn.Authorization.RepoCtl.Model;
using Altinn.Authorization.RepoCtl.Utils;
using Spectre.Console;

namespace Altinn.Authorization.RepoCtl.Commands.Ci;

internal sealed class TagAppImagesCommand(ContainerService containers)
{
    public ICommandResult Invoke(
        AltinnRepository repository,
        AltinnVertical vertical,
        [Option("--version", Description = "Release version, without the v prefix.")] string version,
        [Option("--sha", Description = "Source image tag; defaults to GITHUB_SHA.")] string? sha = null,
        [Option("--registry", Description = "Registry repository prefix for manifest images; defaults to REPOCTL_OCI_REGISTRY.")] string? registry = null)
    {
        if (vertical.Kind != AltinnVerticalKind.Application)
        {
            return Error($"Vertical {vertical.Id} is not an application.");
        }

        sha ??= Environment.GetEnvironmentVariable("GITHUB_SHA");
        if (string.IsNullOrWhiteSpace(sha) || sha.Length > 128 || sha[0] is '.' or '-'
            || StringHelpers.ContainsInvalidImageTagCharacters(sha))
        {
            return Error("Specify a valid source image tag with --sha or GITHUB_SHA.");
        }

        if (!CalendarVersion.TryValidate(version, out var error))
        {
            return Error(error);
        }

        registry ??= Environment.GetEnvironmentVariable("REPOCTL_OCI_REGISTRY");
        registry = registry?.TrimEnd('/');
        if (string.IsNullOrWhiteSpace(registry) || registry.StartsWith('-') || registry.Any(char.IsWhiteSpace))
        {
            return Error("Specify a valid registry repository prefix with --registry or REPOCTL_OCI_REGISTRY.");
        }

        return new TagImagesResult(repository, vertical, sha, version, registry, containers);
    }

    private static ICommandResult Error(string message)
        => new RenderResult(Markup.FromInterpolated($"[red]{message}[/]\n")) { ReturnCode = 1 };

    private sealed class TagImagesResult(AltinnRepository repository, AltinnVertical vertical, string sha, string version, string registry, ContainerService containers)
        : ICommandResult
    {
        public async Task Execute(CommandInvocationContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                await containers.TagReleaseImages(repository, vertical, sha, version, registry, context.Console, cancellationToken);
            }
            catch (ProcessFailedException ex)
            {
                await Error(ex.Message).Execute(context, cancellationToken);
                context.ReturnCode = ex.Process.ExitCode;
            }
            catch (Exception ex) when (ex is InvalidOperationException or System.Text.Json.JsonException)
            {
                await Error(ex.Message).Execute(context, cancellationToken);
            }
        }
    }
}
