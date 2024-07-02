using Altinn.Cli.Jwks.Binders;
using Altinn.Cli.Jwks.Options;
using System.CommandLine;
using System.CommandLine.Binding;
using System.CommandLine.Invocation;
using System.Diagnostics.CodeAnalysis;

namespace Altinn.Cli.Jwks.Commands;

[ExcludeFromCodeCoverage]
internal abstract class BaseCommand
    : Command
{
    public static StoreOption StoreOption { get; }
        = new StoreOption();

    public static IValueDescriptor<IConsole> Console { get; }
        = new RequiredServiceDescriptor<IConsole>();

    public static IValueDescriptor<CancellationToken> CancellationToken { get; }
        = new RequiredServiceDescriptor<CancellationToken>();

    protected static T? GetValueForHandlerParameter<T>(
        IValueDescriptor<T> symbol,
        InvocationContext context)
    {
        if (symbol is IValueSource source
            && source.TryGetValue(symbol, context.BindingContext, out var boundValue)
            && boundValue is T value)
        {
            return value;
        }

        return symbol switch
        {
            Argument<T> argument => context.ParseResult.GetValueForArgument(argument),
            Option<T> option => context.ParseResult.GetValueForOption(option),
            _ => throw new ArgumentOutOfRangeException(nameof(symbol)),
        };
    }

    protected BaseCommand(string name, string description)
         : base(name, description)
    {
    }
}
