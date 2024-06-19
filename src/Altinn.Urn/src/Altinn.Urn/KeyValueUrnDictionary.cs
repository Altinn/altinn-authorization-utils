using Altinn.Urn.Json;
using System.Collections;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Altinn.Urn;

/// <summary>
/// A dictionary of <typeparamref name="TUrn"/> keyed by <typeparamref name="TVariants"/>.
/// </summary>
/// <typeparam name="TUrn">The URN type.</typeparam>
/// <typeparam name="TVariants">The URN variants enum type.</typeparam>
[DebuggerDisplay("Count = {Count}")]
[DebuggerTypeProxy(typeof(UrnDictionaryDebugView<,>))]
[JsonConverter(typeof(DictionaryUrnJsonConverter))]
public class KeyValueUrnDictionary<TUrn, TVariants>
    : IDictionary<TVariants, TUrn>
    , IReadOnlyDictionary<TVariants, TUrn>
    , ICollection<TUrn>
    , IReadOnlyCollection<TUrn>
    where TUrn : IKeyValueUrn<TUrn, TVariants>
    where TVariants : struct, Enum
{
    private readonly Dictionary<TVariants, TUrn> _dictionary = new();

    /// <inheritdoc/>
    public int Count => _dictionary.Count;

    /// <inheritdoc/>
    public bool IsReadOnly => false;

    /// <inheritdoc/>
    public TUrn this[TVariants key]
        => _dictionary[key];

    /// <inheritdoc/>
    TUrn IDictionary<TVariants, TUrn>.this[TVariants key]
    {
        get => _dictionary[key];
        set
        {
            if (!key.Equals(value.UrnType))
            {
                throw new ArgumentException("Key does not match value UrnType", nameof(key));
            }

            _dictionary[key] = value;
        }
    }

    /// <inheritdoc/>
    public bool ContainsKey(TVariants key)
        => _dictionary.ContainsKey(key);

    /// <inheritdoc/>
    public bool TryGetValue(TVariants key, [MaybeNullWhen(false)] out TUrn value)
        => _dictionary.TryGetValue(key, out value);

    /// <summary>
    /// Tries to add a new <typeparamref name="TUrn"/> to the dictionary.
    /// </summary>
    /// <param name="urn">The <typeparamref name="TUrn"/> to add.</param>
    /// <returns><see langword="true"/> if the urn was successfully added to the dictionary, otherwise <see langword="false"/>.</returns>
    public bool TryAdd(TUrn urn)
    {
        if (_dictionary.ContainsKey(urn.UrnType))
        {
            return false;
        }

        _dictionary.Add(urn.UrnType, urn);
        return true;
    }

    /// <summary>
    /// Adds a new <typeparamref name="TUrn"/> to the dictionary.
    /// </summary>
    /// <param name="value">The <typeparamref name="TUrn"/> to add.</param>
    /// <exception cref="ArgumentException">
    /// An <typeparamref name="TUrn"/> with the same <typeparamref name="TVariants"/> already exists in the <see cref="KeyValueUrnDictionary{TUrn, TVariants}"/>.
    /// </exception>
    public void Add(TUrn value)
        => _dictionary.Add(value.UrnType, value);

    /// <inheritdoc cref="ICollection{T}.Add(T)"/>
    /// <param name="item">The URN to add to the dictionary.</param>
    /// <param name="overwrite">If <see langword="true"/>, allows overwriting an existing value.</param>
    public void Add(TUrn item, bool overwrite)
    {
        if (overwrite)
        {
            _dictionary[item.UrnType] = item;
        }
        else
        {
            _dictionary.Add(item.UrnType, item);
        }
    }

    /// <inheritdoc/>
    void IDictionary<TVariants, TUrn>.Add(TVariants key, TUrn value)
        => _dictionary.Add(key, value);

    /// <inheritdoc/>
    void ICollection<KeyValuePair<TVariants, TUrn>>.Add(KeyValuePair<TVariants, TUrn> item)
        => _dictionary.Add(item.Key, item.Value);

    /// <inheritdoc/>
    public void Clear()
        => _dictionary.Clear();

    /// <inheritdoc/>
    bool ICollection<KeyValuePair<TVariants, TUrn>>.Contains(KeyValuePair<TVariants, TUrn> item)
        => ((ICollection<KeyValuePair<TVariants, TUrn>>)_dictionary).Contains(item);

    /// <inheritdoc/>
    bool ICollection<TUrn>.Contains(TUrn item)
        => ((ICollection<KeyValuePair<TVariants, TUrn>>)_dictionary).Contains(KeyValuePair.Create(item.UrnType, item));

    /// <inheritdoc/>
    bool ICollection<KeyValuePair<TVariants, TUrn>>.Remove(KeyValuePair<TVariants, TUrn> item)
        => ((ICollection<KeyValuePair<TVariants, TUrn>>)_dictionary).Remove(item);

    /// <inheritdoc/>
    bool ICollection<TUrn>.Remove(TUrn item)
        => ((ICollection<KeyValuePair<TVariants, TUrn>>)_dictionary).Remove(KeyValuePair.Create(item.UrnType, item));

    /// <inheritdoc/>
    public bool Remove(TVariants key)
        => _dictionary.Remove(key);

    /// <inheritdoc/>
    void ICollection<KeyValuePair<TVariants, TUrn>>.CopyTo(KeyValuePair<TVariants, TUrn>[] array, int arrayIndex)
        => ((ICollection<KeyValuePair<TVariants, TUrn>>)_dictionary).CopyTo(array, arrayIndex);

    /// <inheritdoc/>
    void ICollection<TUrn>.CopyTo(TUrn[] array, int arrayIndex)
        => _dictionary.Values.CopyTo(array, arrayIndex);

    /// <inheritdoc/>
    IEnumerator<TUrn> IEnumerable<TUrn>.GetEnumerator()
        => _dictionary.Values.GetEnumerator();

    /// <inheritdoc/>
    IEnumerator<KeyValuePair<TVariants, TUrn>> IEnumerable<KeyValuePair<TVariants, TUrn>>.GetEnumerator()
        => _dictionary.GetEnumerator();

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator()
        => GetEnumerator();

    /// <inheritdoc/>
    public Dictionary<TVariants, TUrn>.ValueCollection.Enumerator GetEnumerator()
        => _dictionary.Values.GetEnumerator();

    /// <inheritdoc/>
    public Dictionary<TVariants, TUrn>.ValueCollection Values => _dictionary.Values;

    /// <inheritdoc/>
    ICollection<TVariants> IDictionary<TVariants, TUrn>.Keys => _dictionary.Keys;

    /// <inheritdoc/>
    IEnumerable<TVariants> IReadOnlyDictionary<TVariants, TUrn>.Keys => _dictionary.Keys;

    /// <inheritdoc/>
    public Dictionary<TVariants, TUrn>.KeyCollection Keys => _dictionary.Keys;

    /// <inheritdoc/>
    ICollection<TUrn> IDictionary<TVariants, TUrn>.Values => _dictionary.Values;

    /// <inheritdoc/>
    IEnumerable<TUrn> IReadOnlyDictionary<TVariants, TUrn>.Values => _dictionary.Values;
}

/// <summary>
/// Defines a debug view for a <see cref="KeyValueUrnDictionary{TUrn, TVariant}"/> by a debugger.
/// </summary>
/// <typeparam name="TUrn">The urn type.</typeparam>
/// <typeparam name="TVariants">The urn variant enum type.</typeparam>
internal sealed class UrnDictionaryDebugView<TUrn, TVariants>
    where TUrn : IKeyValueUrn<TUrn, TVariants>
    where TVariants : struct, Enum
{
    private readonly KeyValueUrnDictionary<TUrn, TVariants> _dict;

    public UrnDictionaryDebugView(KeyValueUrnDictionary<TUrn, TVariants> dictionary)
    {
        ArgumentNullException.ThrowIfNull(dictionary);

        _dict = dictionary;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
    public DebugViewUrnDictionaryItem<TUrn, TVariants>[] Items
    {
        get
        {
            var keyValuePairs = new KeyValuePair<TVariants, TUrn>[_dict.Count];
            ((IDictionary<TVariants, TUrn>)_dict).CopyTo(keyValuePairs, 0);
            var items = new DebugViewUrnDictionaryItem<TUrn, TVariants>[keyValuePairs.Length];
            for (int i = 0; i < items.Length; i++)
            {
                items[i] = new DebugViewUrnDictionaryItem<TUrn, TVariants>(keyValuePairs[i]);
            }
            return items;
        }
    }
}

/// <summary>
/// Defines a key/value pair for displaying an item of a <see cref="KeyValueUrnDictionary{TUrn, TVariant}"/> by a debugger.
/// </summary>
/// <typeparam name="TUrn">The urn type.</typeparam>
/// <typeparam name="TVariants">The urn variant enum type.</typeparam>
[DebuggerDisplay("{Value}", Name = "[{Key}]")]
internal readonly struct DebugViewUrnDictionaryItem<TUrn, TVariants>
    where TUrn : IKeyValueUrn<TUrn, TVariants>
    where TVariants : struct, Enum
{
    public DebugViewUrnDictionaryItem(KeyValuePair<TVariants, TUrn> keyValue)
    {
        Key = keyValue.Key;
        Value = keyValue.Value;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
    public TVariants Key { get; }

    [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
    public TUrn Value { get; }
}
