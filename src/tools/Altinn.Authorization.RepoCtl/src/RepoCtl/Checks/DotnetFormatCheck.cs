using Altinn.Authorization.CommandLine.Shell;
using Altinn.Authorization.RepoCtl.Model;

namespace Altinn.Authorization.RepoCtl.Checks;

internal sealed class DotnetFormatCheck
    : ExternalToolCheck
{
    public override string CheckId
        => "dotnet-format";

    public override string CheckDisplayName
        => "Dotnet code formatting";

    protected override Command CreateCommand(AltinnRepository repository)
        => Command.Create("dotnet", "format", "--verify-no-changes")
            .WithWorkingDirectory(repository.RootDirectory.FullName);
}
