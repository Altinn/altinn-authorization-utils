using System.Collections.Immutable;
using Altinn.Authorization.CommandLine;
using Altinn.Authorization.CommandLine.Results;
using Altinn.Authorization.CommandLine.Shell;
using Altinn.Authorization.RepoCtl.Utils;
using Microsoft.Extensions.FileSystemGlobbing;
using Spectre.Console;

namespace Altinn.Authorization.RepoCtl.NuGet;

internal sealed class NuGetService(AltinnRepositoryAccessor repositoryAccessor)
{
    public async Task<ICommandResult> PublishPackages(
        string glob,
        string[] args)
    {
        var repository = await repositoryAccessor.GetRepository();

        var matcher = new Matcher();
        matcher.AddInclude(glob);

        var files = matcher.GetResultsInFullPath(repository.RootDirectory.FullName).ToImmutableArray();

        if (files.IsEmpty)
        {
            return new RenderResult(Markup.FromInterpolated($"No files found in [blue]{repository.RootDirectory.FullName}[/] that matched glob [cyan]\"{glob}\"[/].\n"));
        }

        return new PublishPackagesResult(files, args.ToImmutableArray());
    }

    private sealed class PublishPackagesResult(ImmutableArray<string> files, ImmutableArray<string> args)
        : ICommandResult
    {
        public async Task Execute(CommandInvocationContext context, CancellationToken cancellationToken = default)
        {
            var console = context.Console;

            foreach (var file in files)
            {
                var cmd = Command.Create("dotnet", ["nuget", "push", "--skip-duplicate", file, .. args]);
                console.Write(Markup.FromInterpolated($"Publishing [cyan]\"{file}\"[/]\n"));

                await cmd.ToResult(echo: false).Execute(context, cancellationToken);
                if (context.ReturnCode is not 0)
                {
                    return;
                }
            }
        }
    }
}
