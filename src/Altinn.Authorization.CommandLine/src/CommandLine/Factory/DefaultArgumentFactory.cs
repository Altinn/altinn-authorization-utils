using System.Collections.Immutable;
using System.CommandLine;
using System.Runtime.CompilerServices;

namespace Altinn.Authorization.CommandLine.Factory;

internal sealed class DefaultArgumentFactory
    : ArgumentFactory
{
    private readonly ImmutableArray<IConfigureArgument> _configurators;

    public DefaultArgumentFactory(IEnumerable<IConfigureArgument> configurators)
    {
        _configurators = configurators.ToImmutableArray();
    }

    public override Argument<T> Create<T>(string name, string? description, StrongBox<T>? defaultValueBox)
    {
        var argument = new Argument<T>(name)
        {
            Description = description,
        };

        if (defaultValueBox is not null)
        {
            var value = defaultValueBox.Value!;
            argument.DefaultValueFactory = _ => value;
        }

        foreach (var configurator in _configurators)
        {
            configurator.Configure(argument);
        }

        return argument;
    }
}
