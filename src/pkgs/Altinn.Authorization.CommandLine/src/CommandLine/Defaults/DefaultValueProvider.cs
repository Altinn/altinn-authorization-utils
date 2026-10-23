using System.Diagnostics.CodeAnalysis;
using Altinn.Authorization.CommandLine.Help;

namespace Altinn.Authorization.CommandLine;

/// <summary>Describes a typed default-value source.</summary>
public abstract class DefaultValueProvider
{
    /// <summary>Creates a provider for a constant, including an explicit null value.</summary>
    public static DefaultValueProvider<T> FromValue<T>(T value)
        => new ConstantValueProvider<T>(value);

    /// <summary>Gets the type supplied by this provider.</summary>
    public abstract Type ValueType { get; }

    /// <summary>Binds this provider to a compatible target type.</summary>
    public abstract DefaultValueProvider<T> Bind<T>();

    /// <summary>Gets help text without resolving the default value.</summary>
    public abstract IEnumerable<TextSegment> GetHelpText(HelpDisplayArgumentContext context);
}

/// <summary>Supplies a typed default value.</summary>
/// <typeparam name="T">The value type.</typeparam>
public abstract class DefaultValueProvider<T>
    : DefaultValueProvider
{
    /// <inheritdoc />
    public sealed override Type ValueType => typeof(T);

    /// <summary>Tries to supply a value, returning a parse diagnostic on failure.</summary>
    public abstract bool TryProvideValue(DefaultValueProviderContext context, out T value, [NotNullWhen(false)] out string? error);

    /// <inheritdoc />
    public override DefaultValueProvider<TTarget> Bind<TTarget>()
    {
        if (this is DefaultValueProvider<TTarget> typed)
        {
            return typed;
        }

        if (!typeof(TTarget).IsAssignableFrom(typeof(T)))
        {
            throw new ArgumentException($"The default value type '{typeof(T)}' cannot be assigned to '{typeof(TTarget)}'.");
        }

        return new BoundValueProvider<TTarget>(this);
    }

    private sealed class BoundValueProvider<TTarget>(DefaultValueProvider<T> inner)
        : DefaultValueProvider<TTarget>
    {
        public override bool TryProvideValue(DefaultValueProviderContext context, out TTarget value, [NotNullWhen(false)] out string? error)
        {
            var success = inner.TryProvideValue(context, out var provided, out error);
            value = success ? (TTarget)(object?)provided! : default!;
            return success;
        }

        public override IEnumerable<TextSegment> GetHelpText(HelpDisplayArgumentContext context) => inner.GetHelpText(context);
    }
}
