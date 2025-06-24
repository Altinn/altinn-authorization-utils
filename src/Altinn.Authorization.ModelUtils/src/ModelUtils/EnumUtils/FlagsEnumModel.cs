using CommunityToolkit.Diagnostics;
using System.Collections.Concurrent;
using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Text.Json;

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
    /// <param name="options">The JSON serializer options to use. Defaults to <see cref="JsonSerializerOptions.Web"/>.</param>
    /// <returns>A new model.</returns>
    public static FlagsEnumModel<T> Create<T>(JsonSerializerOptions? options = null)
        where T : struct, Enum
        => FlagsEnumModel<T>.Create(options ?? JsonSerializerOptions.Web);
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
    /// <param name="options">The JSON serializer options to use.</param>
    /// <returns>A new model.</returns>
    public static FlagsEnumModel<TEnum> Create(JsonSerializerOptions options)
    {
        var values = Enum.GetValues<TEnum>();
        var names = Enum.GetNames<TEnum>();
        
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

            var name = GetString(value, fieldName, options);
            builder.Add(new Item(value, name));
        }

        if (!noneFound)
        {
            ThrowHelper.ThrowInvalidOperationException("Flags enum must have a default value called 'None'");
        }

        // Sort by number of bits set, greatest number of bits set first, then by absolute value of the enum value.
        builder.Sort(static (a, b) =>
        {
            var aBits = a.Value.NumBitsSet();
            var bBits = b.Value.NumBitsSet();

            return bBits.CompareTo(aBits);
        });

        var items = builder.DrainToImmutable();
        var byStringBuilder = new Dictionary<string, TEnum>(items.Length, StringComparer.Ordinal);
        foreach (ref readonly var item in items.AsSpan())
        {
            byStringBuilder.TryAdd(item.Name, item.Value);
        }

        var byString = byStringBuilder.ToFrozenDictionary();
        var byValue = items.Select(static i => KeyValuePair.Create(i.Value, i.Name)).ToFrozenDictionary();
        return new(items, byString, byValue);
    }

    private static string GetString(TEnum value, string fieldName, JsonSerializerOptions options)
    {
        using var doc = JsonSerializer.SerializeToDocument(value, options);
        if (doc.RootElement.ValueKind == JsonValueKind.Number)
        {
            // if the serializer serializes to numbers, we default to using kebab-case for the name
            return JsonNamingPolicy.KebabCaseLower.ConvertName(fieldName);
        }

        if (doc.RootElement.ValueKind != JsonValueKind.String)
        {
            ThrowHelper.ThrowInvalidOperationException("Flags enum must serialize to a string or number");
        }

        return doc.RootElement.GetString()!;
    }

    private readonly FrozenDictionary<string, TEnum> _byString;
    private readonly FrozenDictionary<TEnum, string> _byValue;
    private readonly ImmutableArray<Item> _items;
    private readonly ConcurrentDictionary<TEnum, string> _formatted = new();

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
    /// <param name="name">The name to match.</param>
    /// <param name="value">Output value. Set to <see langword="default"/> if name is not found.</param>
    /// <returns><see langword="true"/> if the parsing was successful, otherwise <see langword="false"/>.</returns>
    public bool TryParse(ReadOnlySpan<char> name, out TEnum value)
    {
        if (name.Length == 0)
        {
            value = default;
            return true;
        }

        var lookup = _byString.GetAlternateLookup<ReadOnlySpan<char>>();
        if (lookup.TryGetValue(name, out value))
        {
            return true;
        }

        value = default; // none
        foreach (var range in name.Split(','))
        {
            var item = name[range].Trim();
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

    private string FormatCore(TEnum value)
    {
        if (value.IsDefault())
        {
            return string.Empty;
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

        return string.Join(',', items.Select(static i => i.Name));
    }

    /// <summary>
    /// An item in the model.
    /// </summary>
    /// <param name="Value">The value of the item.</param>
    /// <param name="Name">The name of the item.</param>
    public readonly record struct Item(TEnum Value, string Name);
}
