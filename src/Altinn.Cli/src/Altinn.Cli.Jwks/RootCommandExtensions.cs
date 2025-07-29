using System.CommandLine;
using System.CommandLine.Invocation;
using System.Diagnostics.CodeAnalysis;

namespace Altinn.Cli.Jwks;

[ExcludeFromCodeCoverage]
internal static class RootCommandExtensions
{
    public static RootCommand AddOptionMiddleware<TOption>(this RootCommand command, Func<TOption, SynchronousCommandLineAction> factory)
        where TOption : Option
    {
        var option = command.Options.Single(x => x.GetType() == typeof(TOption));
        option.Action = factory((TOption)option);
        
        return command;
    }
}
