using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Altinn.Authorization.CommandLine.Console;
using Altinn.Authorization.CommandLine.Shell;
using Altinn.Authorization.RepoCtl.Model;
using Spectre.Console;

namespace Altinn.Authorization.RepoCtl.Changelog;

internal sealed class GitCliffChangelogGenerator(IConsole console)
    : IChangelogGenerator
{
    public async Task<string> Generate(AltinnRepository repository, AltinnVertical vertical, string tag, string sha, CancellationToken cancellationToken = default)
    {
        var (_, head, _) = await Command.Create("git", ["rev-parse", "HEAD"])
            .WithWorkingDirectory(repository.RootDirectory.FullName).ExecuteCaptured(cancellationToken);
        if (!string.Equals(head.Trim(), sha, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException($"Check out the release commit {sha} before generating its changelog (current HEAD: {head.Trim()}).");
        }

        var changelog = new StringBuilder();
        await foreach (var output in CreateCommand(repository, vertical, tag).ExecuteLineCaptured(cancellationToken))
        {
            if (output.Source == CommandOutputSource.Stdout)
            {
                changelog.Append(output.Line).Append('\n');
            }
            else
            {
                console.StdErr.Write(new Text(output.Line + "\n"));
            }
        }

        return changelog.ToString();
    }

    internal static Command CreateCommand(AltinnRepository repository, AltinnVertical vertical, string tag)
    {
        var prefix = vertical.Id.ToString("tag-prefix", CultureInfo.InvariantCulture);
        List<string> args = ["cliff", "--config", "cliff.toml", "--tag-pattern", $"^{Regex.Escape(prefix)}-v.*$"];

        AddPath(vertical);
        foreach (var dependency in vertical.AllDependencies.OfKind(AltinnVerticalKind.Library))
        {
            AddPath(dependency);
        }

        args.AddRange(["--unreleased", "--tag", tag]);
        return Command.Create("git", [.. args]).WithWorkingDirectory(repository.RootDirectory.FullName);

        void AddPath(AltinnVertical item)
        {
            var path = Path.GetRelativePath(repository.RootDirectory.FullName, item.Directory.FullName).Replace(Path.DirectorySeparatorChar, '/');
            args.Add("--include-path");
            args.Add($"{path}/**");
        }
    }
}
