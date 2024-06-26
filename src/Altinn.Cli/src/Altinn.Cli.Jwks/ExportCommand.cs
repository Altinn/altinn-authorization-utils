using System.CommandLine;
using System.Diagnostics.CodeAnalysis;

namespace Altinn.Cli.Jwks;

[ExcludeFromCodeCoverage]
internal class ExportCommand
{
    public static Command Command => CreateCommand();

    private static Command CreateCommand()
    {
        var cmd = new Command("export", "Export key sets");

        cmd.AddCommand(ExportMaskinportenCommand.Command);

        return cmd;
    }
}
