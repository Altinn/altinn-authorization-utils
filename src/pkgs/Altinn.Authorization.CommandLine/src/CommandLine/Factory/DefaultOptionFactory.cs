using System.Collections.Immutable;
using System.CommandLine;

namespace Altinn.Authorization.CommandLine.Factory;

internal sealed class DefaultOptionFactory
    : OptionFactory
{
    private readonly ImmutableArray<IConfigureOption> _configurators;

    public DefaultOptionFactory(IEnumerable<IConfigureOption> configurators)
    {
        _configurators = configurators.ToImmutableArray();
    }

    public override Option<T> Create<T>(string name, ReadOnlySpan<string> aliases, string? description, bool isRequired, DefaultValueProvider<T>? defaultValueProvider)
    {
        var option = new Option<T>(name, [.. aliases])
        {
            Description = description,
            Required = isRequired,
        };

        DefaultValueProviderBinding.Apply(option, defaultValueProvider);

        foreach (var configurator in _configurators)
        {
            configurator.Configure(option);
        }

        return option;
    }
}
