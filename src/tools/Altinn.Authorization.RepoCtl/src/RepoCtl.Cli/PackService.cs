using Altinn.Authorization.CommandLine;
using Altinn.Authorization.CommandLine.Results;
using Altinn.Authorization.CommandLine.Shell;
using Altinn.Authorization.RepoCtl.Model;
using Spectre.Console;

namespace Altinn.Authorization.RepoCtl;

internal class PackService
{
    public ICommandResult Pack(AltinnVertical vertical, string[] args)
    {
        if (!vertical.Projects.Any(p => p.Type.IsPackable))
        {
            return new RenderResult(Markup.FromInterpolated($"Vertical [blue]{vertical.Kind}[/]:[cyan]{vertical.FullName}[/] has no packable projects.\n"));
        }

        return new PackResult(vertical, args);
    }

    private sealed class PackResult(AltinnVertical vertical, string[] args)
        : ICommandResult
    {
        public async Task Execute(CommandInvocationContext context, CancellationToken cancellationToken = default)
        {
            foreach (var project in vertical.Projects.Where(p => p.Type.IsPackable))
            {
                var command = Command.Create("dotnet", ["pack", project.ProjectFile.FullName, .. args]);
                await command.ToResult().Execute(context, cancellationToken);
                if (context.ReturnCode != 0)
                {
                    return;
                }
            }
        }
    }

}
