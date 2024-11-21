using System.Diagnostics.CodeAnalysis;

namespace Altinn.Cli.Jwks.Commands;

[ExcludeFromCodeCoverage]
internal class ExportCommand
    : BaseCommand
{
    public ExportCommand()
        : base("export", "Export key sets")
    {
        AddCommand(new ExportKeyCommand());
        AddCommand(new ExportMaskinportenCommand());
        AddCommand(new ExportPemCommand());
    }
}
