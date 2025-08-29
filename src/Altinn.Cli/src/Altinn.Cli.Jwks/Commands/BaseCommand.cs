using Altinn.Cli.Jwks.Console;
using Altinn.Cli.Jwks.Options;
using System.CommandLine;
using System.Diagnostics.CodeAnalysis;

namespace Altinn.Cli.Jwks.Commands;

[ExcludeFromCodeCoverage]
internal abstract class BaseCommand
    : Command
{
    private readonly IConsole _console;

    public static StoreOption StoreOption { get; }
        = new StoreOption();

    protected IConsole Console => _console;

    protected BaseCommand(IConsole console, string name, string description)
         : base(name, description)
    {
        _console = console;
    }
}
