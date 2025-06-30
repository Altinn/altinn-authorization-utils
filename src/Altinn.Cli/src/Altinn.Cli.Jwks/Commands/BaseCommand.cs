using Altinn.Cli.Jwks.Options;
using System.CommandLine;
using System.Diagnostics.CodeAnalysis;

namespace Altinn.Cli.Jwks.Commands;

[ExcludeFromCodeCoverage]
internal abstract class BaseCommand
    : Command
{
    public static StoreOption StoreOption { get; }
        = new StoreOption();

    protected BaseCommand(string name, string description)
         : base(name, description)
    {
    }
}
