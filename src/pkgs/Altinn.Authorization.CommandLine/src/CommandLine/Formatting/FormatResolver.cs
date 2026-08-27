using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace Altinn.Authorization.CommandLine.Formatting;

/// <summary>
/// Defines a resolver that can resolve a formatter for a specific type.
/// </summary>
/// <typeparam name="TFormat">The type of the format to resolve formatters for.</typeparam>
internal sealed class FormatResolver<TFormat>
    where TFormat : IFormat
{
    private readonly ImmutableArray<IFormatterResolver<TFormat>> _formatterResolvers;
    private readonly ImmutableArray<IFormatter<TFormat>> _formatters;

    public FormatResolver(
        IEnumerable<IFormatterResolver<TFormat>> formatterResolvers,
        IEnumerable<IFormatter<TFormat>> formatters)
    {
        _formatterResolvers = formatterResolvers.ToImmutableArray();
        _formatters = formatters.ToImmutableArray();
    }

    /// <inheritdoc/>
    public bool TryResolve(Type type, [NotNullWhen(true)] out IFormatter<TFormat>? formatter)
    {
        foreach (var resolver in _formatterResolvers)
        {
            if (resolver.TryResolve(type, out formatter))
            {
                return true;
            }
        }

        foreach (var f in _formatters)
        {
            if (f.CanFormat(type))
            {
                formatter = f;
                return true;
            }
        }

        if (type.IsAssignableTo(typeof(IFormattable<TFormat>)))
        {
            formatter = SelfFormatter<TFormat>.Instance;
            return true;
        }

        formatter = null;
        return false;
    }
}
