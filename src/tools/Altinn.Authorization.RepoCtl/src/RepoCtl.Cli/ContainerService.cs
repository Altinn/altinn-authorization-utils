using Altinn.Authorization.CommandLine.Results;
using Altinn.Authorization.CommandLine.Shell;
using Altinn.Authorization.RepoCtl.Model;
using Spectre.Console;

namespace Altinn.Authorization.RepoCtl;

internal class ContainerService
{
    // method is virtual for testing purposes
    public virtual ICommandResult PromoteManifests(AltinnRepository repository, string image, string sourceTag, IReadOnlyList<string> environments)
        => CreateManifestPromotionCommand(repository.RootDirectory.FullName, image, sourceTag, environments).ToResult();

    internal static Command CreateManifestPromotionCommand(string workingDirectory, string image, string sourceTag, IReadOnlyList<string> environments)
    {
        List<string> args = ["buildx", "imagetools", "create"];
        foreach (var environment in environments)
        {
            args.Add("-t");
            args.Add($"{image}:{environment}");
        }

        args.Add($"{image}:{sourceTag}");

        return Command.Create("docker", [.. args])
            .WithWorkingDirectory(workingDirectory);
    }

    public ICommandResult BuildContainers(AltinnRepository repository, AltinnVertical vertical, bool push, string? tag)
    {
        if (vertical.Kind != AltinnVerticalKind.Application)
        {
            return new RenderResult(Markup.FromInterpolated($"Vertical [blue]{vertical.Kind}[/]:[cyan]{vertical.FullName}[/] is not an application and cannot have containers built.\n"))
            {
                ReturnCode = 1,
            };
        }

        List<string> buildArgs = ["buildx", "bake", "--file", Path.Combine(vertical.Directory.FullName, "docker-bake.hcl")];
        if (push)
        {
            buildArgs.Add("--push");
        }

        if (!string.IsNullOrWhiteSpace(tag))
        {
            buildArgs.Add("--var");
            buildArgs.Add($"TAG={tag}");
        }

        var command = Command.Create("docker", [.. buildArgs])
            .WithWorkingDirectory(repository.RootDirectory.FullName);

        return command.ToResult();
    }
}
