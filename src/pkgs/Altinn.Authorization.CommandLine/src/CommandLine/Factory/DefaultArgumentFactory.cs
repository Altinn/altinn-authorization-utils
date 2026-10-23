using System.Collections.Immutable;
using System.CommandLine;

namespace Altinn.Authorization.CommandLine.Factory;

internal sealed class DefaultArgumentFactory
    : ArgumentFactory
{
    private readonly ImmutableArray<IConfigureArgument> _configurators;

    public DefaultArgumentFactory(IEnumerable<IConfigureArgument> configurators)
    {
        _configurators = configurators.ToImmutableArray();
    }

    public override Argument<T> Create<T>(string name, string? description, DefaultValueProvider<T>? defaultValueProvider)
    {
        var argument = new Argument<T>(name)
        {
            Description = description,
        };

        DefaultValueProviderBinding.Apply(argument, defaultValueProvider);

        foreach (var configurator in _configurators)
        {
            configurator.Configure(argument);
        }

        return argument;
    }
}
