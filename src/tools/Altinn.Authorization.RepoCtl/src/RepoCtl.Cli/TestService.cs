using Altinn.Authorization.CommandLine.Results;
using Altinn.Authorization.CommandLine.Shell;
using Altinn.Authorization.RepoCtl.Model;
using Spectre.Console;

namespace Altinn.Authorization.RepoCtl;

internal class TestService
{
    public ICommandResult RunTests(AltinnVertical vertical, string[] args)
    {
        if (!vertical.Projects.Any(p => p.Type is AltinnProjectType.Test))
        {
            return new RenderResult(Markup.FromInterpolated($"Vertical [blue]{vertical.Kind}[/]:[cyan]{vertical.FullName}[/] has no tests.\n"));
        }

        return Command.Create("dotnet", ["test", vertical.SolutionFile.FullName, .. args]).ToResult();
    }
}
