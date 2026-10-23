using System.Globalization;
using Altinn.Authorization.CommandLine.Console;
using Altinn.Authorization.CommandLine.Results;
using Altinn.Authorization.CommandLine.Shell;
using Altinn.Authorization.RepoCtl.Model;
using Altinn.Authorization.RepoCtl.Containers;
using Spectre.Console;
using CommunityToolkit.Diagnostics;
using System.Diagnostics;

namespace Altinn.Authorization.RepoCtl;

internal class ContainerService
{
    public async Task TagReleaseImages(AltinnRepository repository, AltinnVertical vertical, string sha, string version, string registry, IConsole console, CancellationToken cancellationToken = default)
    {
        var (_, bake, _) = await Command.Create("docker", ["buildx", "bake", "--file", Path.Combine(vertical.Directory.FullName, "docker-bake.hcl"), "--var", $"TAG={sha}", "--print"])
            .WithWorkingDirectory(repository.RootDirectory.FullName)
            .ExecuteCaptured(cancellationToken);

        var images = GetReleaseImages(BakeOutput.Deserialize(bake), sha);
        var manifestImage = $"{registry}/{vertical.Id.ToString("short-slug", CultureInfo.InvariantCulture)}-manifests";

        foreach (var command in CreateReleaseTaggingCommands(repository.RootDirectory.FullName, images, manifestImage, sha, version))
        {
            console.StdErr.Write(Markup.FromInterpolated($"Running {command:display}\n"));
            await command.Execute(cancellationToken);
        }
    }

    internal static IEnumerable<Command> CreateReleaseTaggingCommands(
        string workingDirectory,
        IReadOnlyList<string> images,
        string manifestImage,
        string sha,
        string version)
    {
        var releaseTag = $"v{version}";
        foreach (var image in images.Distinct(StringComparer.Ordinal))
        {
            // all images are guaranteed to end with `:{sha}` at this point
            Debug.Assert(image.EndsWith($":{sha}", StringComparison.Ordinal));

            var releaseImage = $"{image.AsSpan(0, image.Length - sha.Length)}{releaseTag}";
            yield return Command.Create("docker", ["buildx", "imagetools", "create", "-t", releaseImage, image])
                .WithWorkingDirectory(workingDirectory);
        }

        yield return CreateManifestPromotionCommand(workingDirectory, manifestImage, sha, [releaseTag]);
    }

    // method is virtual for testing purposes
    public virtual ICommandResult PromoteManifests(AltinnRepository repository, string image, string sourceTag, IReadOnlyList<string> environments)
        => CreateManifestPromotionCommand(repository.RootDirectory.FullName, image, sourceTag, environments).ToResult();

    internal static Command CreateManifestPromotionCommand(string workingDirectory, string image, string sourceTag, IReadOnlyList<string> targetTags)
    {
        List<string> args = ["tag", "artifact", $"oci://{image}:{sourceTag}"];
        foreach (var tag in targetTags)
        {
            args.Add("--tag");
            args.Add(tag);
        }

        return Command.Create("flux", [.. args])
            .WithWorkingDirectory(workingDirectory);
    }

    internal static List<string> GetReleaseImages(BakeOutput bake, string sha)
    {
        var images = new SortedSet<string>(StringComparer.Ordinal);
        var visiting = new HashSet<string>(StringComparer.Ordinal);
        Traverse(bake, "default", visiting, images, sha);

        if (images.Count == 0)
        {
            throw new InvalidOperationException("The default bake group contains no SHA-tagged images.");
        }

        return [.. images];

        static void Traverse(BakeOutput bake, string name, HashSet<string> visiting, SortedSet<string> images, string sha)
        {
            if (bake.Groups.TryGetValue(name, out var group))
            {
                if (!visiting.Add(name))
                {
                    ThrowHelper.ThrowInvalidOperationException($"Cycle in bake groups at '{name}'.");
                }

                foreach (var child in group.Targets)
                {
                    Traverse(bake, child, visiting, images, sha);
                }

                visiting.Remove(name);
                return;
            }

            if (!bake.Targets.TryGetValue(name, out var target))
            {
                ThrowHelper.ThrowInvalidOperationException($"Bake target '{name}' has no SHA-tagged image.");
            }

            using var matches = target.Tags.Where(tag => tag.EndsWith(sha, StringComparison.Ordinal) && tag.LastIndexOf(':') == tag.Length - sha.Length - 1).GetEnumerator();
            if (!matches.MoveNext())
            {
                ThrowHelper.ThrowInvalidOperationException($"Bake target '{name}' has no SHA-tagged image.");
            }

            do
            {
                images.Add(matches.Current);
            } while (matches.MoveNext());
        }
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
