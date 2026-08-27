using System.Collections.Immutable;
using System.CommandLine;
using System.Runtime.CompilerServices;

namespace Altinn.Authorization.CommandLine.Factory;

internal sealed class DefaultOptionFactory
    : OptionFactory
{
    private readonly ImmutableArray<IConfigureOption> _configurators;

    public DefaultOptionFactory(IEnumerable<IConfigureOption> configurators)
    {
        _configurators = configurators.ToImmutableArray();
    }

    public override Option<T> Create<T>(string name, ReadOnlySpan<string> aliases, string? description, bool isRequired, StrongBox<T>? defaultValueBox)
    {
        var option = new Option<T>(name, [.. aliases])
        {
            Description = description,
            Required = isRequired,
        };

        if (defaultValueBox is not null)
        {
            var value = defaultValueBox.Value!;
            option.DefaultValueFactory = _ => value;
        }

        foreach (var configurator in _configurators)
        {
            configurator.Configure(option);
        }

        return option;
    }
}
