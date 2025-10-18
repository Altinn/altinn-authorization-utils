using CommunityToolkit.Diagnostics;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Altinn.Authorization.ModelUtils.EnumUtils;

/// <summary>
/// Static helpers for <see cref="FlagsEnumModel{TEnum}"/>.
/// </summary>
public static class FlagsEnumModel
{
    /// <summary>
    /// Creates a new <see cref="FlagsEnumModel{TEnum}"/>.
    /// </summary>
    /// <typeparam name="T">The enum type.</typeparam>
    /// <param name="namingPolicy">Optional naming policy.</param>
    /// <returns>A new model.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The provided <paramref name="namingPolicy"/> is not supported.</exception>
    public static FlagsEnumModel<T> Create<T>(JsonKnownNamingPolicy namingPolicy = JsonKnownNamingPolicy.KebabCaseLower)
        where T : struct, Enum
        => Create<T>(namingPolicy switch
        {
            JsonKnownNamingPolicy.Unspecified => null,
            JsonKnownNamingPolicy.CamelCase => JsonNamingPolicy.CamelCase,
            JsonKnownNamingPolicy.SnakeCaseLower => JsonNamingPolicy.SnakeCaseLower,
            JsonKnownNamingPolicy.SnakeCaseUpper => JsonNamingPolicy.SnakeCaseUpper,
            JsonKnownNamingPolicy.KebabCaseLower => JsonNamingPolicy.KebabCaseLower,
            JsonKnownNamingPolicy.KebabCaseUpper => JsonNamingPolicy.KebabCaseUpper,
            _ => ThrowHelper.ThrowArgumentOutOfRangeException<JsonNamingPolicy?>(nameof(namingPolicy), namingPolicy, "Unexpected known naming policy."),
        });

    /// <summary>
    /// Creates a new <see cref="FlagsEnumModel{TEnum}"/>.
    /// </summary>
    /// <typeparam name="T">The enum type.</typeparam>
    /// <param name="namingPolicy">Optional naming policy.</param>
    /// <returns>A new model.</returns>
    public static FlagsEnumModel<T> Create<T>(JsonNamingPolicy? namingPolicy)
        where T : struct, Enum
        => FlagsEnumModel<T>.Create(namingPolicy);
}

/// <summary>
/// A model for a flags enum.
/// </summary>
/// <typeparam name="TEnum">The enum type.</typeparam>
public sealed class FlagsEnumModel<TEnum>
    where TEnum : struct, Enum
{
    /// <summary>
    /// Creates a new <see cref="FlagsEnumModel{TEnum}"/>.
    /// </summary>
    /// <param name="namingPolicy">Optional naming policy.</param>
    /// <returns>A new model.</returns>
    internal static FlagsEnumModel<TEnum> Create(
        JsonNamingPolicy? namingPolicy)
    {
        var values = Enum.GetValues<TEnum>();
        var names = Enum.GetNames<TEnum>();
        Debug.Assert(values.Length == names.Length);

        Dictionary<string, string>? enumMemberAttributes = null;
        foreach (FieldInfo field in typeof(TEnum).GetFields(BindingFlags.Public | BindingFlags.Static))
        {
            if (field.GetCustomAttribute<JsonStringEnumMemberNameAttribute>() is { } attribute
                && !string.IsNullOrEmpty(attribute.Name))
            {
                enumMemberAttributes ??= new(StringComparer.Ordinal);
                enumMemberAttributes.Add(field.Name, attribute.Name);
            }
        }
        
        var noneFound = false;
        var builder = ImmutableArray.CreateBuilder<Item>(values.Length - 1);
        for (int i = 0; i < values.Length; i++)
        {
            var value = values[i];
            var fieldName = names[i];

            if (value.Equals(default(TEnum)))
            {
                if (fieldName != "None")
                {
                    ThrowHelper.ThrowInvalidOperationException("Flags enum's default value must be called 'None'");
                }

                noneFound = true;
                continue;
            }

            var name = GetString(value, fieldName, namingPolicy, enumMemberAttributes);
            builder.Add(new Item(value, name));
        }

        if (!noneFound)
        {
            ThrowHelper.ThrowInvalidOperationException("Flags enum must have a default value called 'None'");
        }

        // Sort by number of bits set, greatest number of bits set first, then by absolute value of the enum value, and last by length of the name.
        builder.Sort(static (a, b) =>
        {
            var aBits = a.Value.NumBitsSet();
            var bBits = b.Value.NumBitsSet();

            var result = bBits.CompareTo(aBits);
            if (result != 0)
            {
                return result;
            }

            result = a.Value.CompareTo(b.Value);
            if (result != 0)
            {
                return result;
            }

            return a.Name.Length.CompareTo(b.Name.Length);
        });

        var items = builder.DrainToImmutable();
        var byStringBuilder = new Dictionary<string, TEnum>(items.Length, StringComparer.Ordinal);
        var byValueBuilder = new Dictionary<TEnum, string>(items.Length);
        foreach (ref readonly var item in items.AsSpan())
        {
            byStringBuilder.TryAdd(item.Name, item.Value);
            byValueBuilder.TryAdd(item.Value, item.Name);
        }

        var byString = byStringBuilder.ToFrozenDictionary();
        var byValue = byValueBuilder.ToFrozenDictionary();
        return new(items, byString, byValue);
    }

    private static string GetString(TEnum value, string fieldName, JsonNamingPolicy? namingPolicy, Dictionary<string, string>? enumMemberAttributes)
    {
        if (enumMemberAttributes is not null && enumMemberAttributes.TryGetValue(fieldName, out var result))
        {
            return result;
        }

        if (namingPolicy is not null)
        {
            return namingPolicy.ConvertName(fieldName);
        }

        return fieldName;
    }

    private readonly FrozenDictionary<string, TEnum> _byString;
    private readonly FrozenDictionary<TEnum, string> _byValue;
    private readonly ImmutableArray<Item> _items;
    private readonly ConcurrentDictionary<TEnum, string> _formatted = new();
    private readonly ConcurrentDictionary<TEnum, ImmutableArray<TEnum>> _components = new();

    private FlagsEnumModel(
        ImmutableArray<Item> items,
        FrozenDictionary<string, TEnum> byString,
        FrozenDictionary<TEnum, string> byValue)
    {
        _items = items;
        _byString = byString;
        _byValue = byValue;
    }

    /// <summary>
    /// Gets the items in the model.
    /// </summary>
    /// <remarks>
    /// The items are ordered based on the number of bits set in the value, with the greatest number of bits set first.
    /// </remarks>
    public ImmutableArray<Item> Items => _items;

    /// <summary>
    /// Tries to parse a <see cref="ReadOnlySpan{T}"/> of characters to an enum value.
    /// </summary>
    /// <param name="input">The input to parse.</param>
    /// <param name="value">Output value. Set to <see langword="default"/> if name is not found.</param>
    /// <returns><see langword="true"/> if the parsing was successful, otherwise <see langword="false"/>.</returns>
    public bool TryParse(ReadOnlySpan<char> input, out TEnum value)
    {
        if (input.Length == 0)
        {
            value = default;
            return true;
        }

        var lookup = _byString.GetAlternateLookup<ReadOnlySpan<char>>();
        if (lookup.TryGetValue(input, out value))
        {
            return true;
        }

        value = default; // none
        foreach (var range in input.Split(','))
        {
            var item = input[range].Trim();
            if (item.Length == 0)
            {
                continue;
            }

            if (!lookup.TryGetValue(item, out var itemValue))
            {
                return false; // not found
            }

            value = value.BitwiseOr(itemValue); // accumulate flags
        }

        return true;
    }

    /// <summary>
    /// Formats a <typeparamref name="TEnum"/>.
    /// </summary>
    /// <param name="value">The value to format.</param>
    /// <returns>The formatted string.</returns>
    public string Format(TEnum value)
        => _byValue.TryGetValue(value, out var formatted)
        ? formatted
        : _formatted.GetOrAdd(value, FormatCore);

    /// <summary>
    /// Attempts to format a <typeparamref name="TEnum"/> into a <see cref="Span{T}"/> of characters.
    /// </summary>
    /// <param name="value">The enum-value to format.</param>
    /// <param name="destination">The destination span.</param>
    /// <param name="charsWritten">Number of characters written, if the method returns <see langword="true"/>.</param>
    /// <returns>Whether or not the formatting succeeded.</returns>
    public bool TryFormat(TEnum value, Span<char> destination, out int charsWritten)
    {
        var formatted = Format(value);
        
        if (formatted.Length <= destination.Length)
        {
            formatted.AsSpan().CopyTo(destination);
            charsWritten = formatted.Length;
            return true;
        }
        
        charsWritten = 0;
        return false;
    }

    private string FormatCore(TEnum value)
    {
        var components = GetComponentStrings(value);

        return string.Join(',', components);
    }

    /// <summary>
    /// Gets the component strings that make up a <typeparamref name="TEnum"/> value.
    /// </summary>
    /// <param name="value">The enum value.</param>
    /// <returns>An <see cref="ImmutableArray{T}"/> of the components of <paramref name="value"/>.</returns>
    public ComponentStringEnumerable GetComponentStrings(TEnum value)
        => new(this, GetComponents(value));

    private ImmutableArray<TEnum> GetComponents(TEnum value)
        => _components.GetOrAdd(value, GetComponentsCore);

    private ImmutableArray<TEnum> GetComponentsCore(TEnum value)
    {
        if (value.IsDefault())
        {
            return [];
        }

        if (_byValue.ContainsKey(value))
        {
            return [value];
        }

        var items = new List<Item>();

        foreach (ref readonly var item in _items.AsSpan())
        {
            if (value.HasFlag(item.Value))
            {
                items.Add(item);
                value = value.RemoveFlags(item.Value);

                if (value.IsDefault())
                {
                    break;
                }
            }
        }

        if (!value.IsDefault())
        {
            ThrowHelper.ThrowArgumentException(nameof(value), "Not all bits in the enum are named");
        }

        items.Sort(static (a, b) => Comparer<TEnum>.Default.Compare(a.Value, b.Value));

        return items.Select(static i => i.Value).ToImmutableArray();
    }

    /// <summary>
    /// An item in the model.
    /// </summary>
    /// <param name="Value">The value of the item.</param>
    /// <param name="Name">The name of the item.</param>
    public readonly record struct Item(TEnum Value, string Name);

    /// <summary>
    /// Enumerable for string components.
    /// </summary>
    public struct ComponentStringEnumerable
        : IEnumerable<string>
    {
        private readonly FlagsEnumModel<TEnum> _model;
        private ImmutableArray<TEnum> _inner;

        internal ComponentStringEnumerable(
            FlagsEnumModel<TEnum> model,
            ImmutableArray<TEnum> inner)
        {
            _model = model;
            _inner = inner;
        }

        /// <inheritdoc cref="IEnumerable{T}.GetEnumerator()"/>
        public ComponentStringEnumerator GetEnumerator()
            => new(_model, _inner.GetEnumerator());

        /// <inheritdoc/>
        IEnumerator<string> IEnumerable<string>.GetEnumerator()
            => GetEnumerator();

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    /// <summary>
    /// Enumerator for string components.
    /// </summary>
    public struct ComponentStringEnumerator
        : IEnumerator<string>
    {
        private readonly FlagsEnumModel<TEnum> _model;
        private ImmutableArray<TEnum>.Enumerator _inner;

        internal ComponentStringEnumerator(
            FlagsEnumModel<TEnum> model,
            ImmutableArray<TEnum>.Enumerator inner)
        {
            _model = model;
            _inner = inner;
        }

        /// <inheritdoc/>
        public readonly string Current
            => _model._byValue[_inner.Current];

        /// <inheritdoc/>
        readonly object IEnumerator.Current
            => Current;

        /// <inheritdoc/>
        readonly void IDisposable.Dispose()
        {
        }

        /// <inheritdoc/>
        public bool MoveNext()
            => _inner.MoveNext();

        /// <inheritdoc/>
        readonly void IEnumerator.Reset()
        {
            ThrowHelper.ThrowNotSupportedException();
        }
    }
}
