using Altinn.Urn.Json;
using System.Collections;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Altinn.Urn;

[DebuggerDisplay("Count = {Count}")]
[DebuggerTypeProxy(typeof(UrnDictionaryDebugView<,>))]
[JsonConverter(typeof(DictionaryUrnJsonConverter))]
public class KeyValueUrnDictionary<TUrn, TVariant>
    : IDictionary<TVariant, TUrn>
    , IReadOnlyDictionary<TVariant, TUrn>
    , ICollection<TUrn>
    , IReadOnlyCollection<TUrn>
    where TUrn : IKeyValueUrn<TUrn, TVariant>
    where TVariant : struct, Enum
{
    private readonly Dictionary<TVariant, TUrn> _dictionary = new();

    /// <inheritdoc/>
    public int Count => _dictionary.Count;

    /// <inheritdoc/>
    public bool IsReadOnly => false;

    /// <inheritdoc/>
    public TUrn this[TVariant key]
        => _dictionary[key];

    /// <inheritdoc/>
    TUrn IDictionary<TVariant, TUrn>.this[TVariant key]
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
    public bool ContainsKey(TVariant key)
        => _dictionary.ContainsKey(key);

    /// <inheritdoc/>
    public bool TryGetValue(TVariant key, [MaybeNullWhen(false)] out TUrn value)
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

    /// <inheritdoc/>
    public void Add(TUrn value)
        => _dictionary.Add(value.UrnType, value);

    /// <inheritdoc cref="ICollection{T}.Add(T)"/>
    /// <param name="overwrite">If <see langword="true"/>, allows overwriting an existing value.</param>
    public void Add(TUrn value, bool overwrite)
    {
        if (overwrite)
        {
            _dictionary[value.UrnType] = value;
        }
        else
        {
            _dictionary.Add(value.UrnType, value);
        }
    }

    /// <inheritdoc/>
    void IDictionary<TVariant, TUrn>.Add(TVariant key, TUrn value)
        => _dictionary.Add(key, value);

    /// <inheritdoc/>
    void ICollection<KeyValuePair<TVariant, TUrn>>.Add(KeyValuePair<TVariant, TUrn> item)
        => _dictionary.Add(item.Key, item.Value);

    /// <inheritdoc/>
    public void Clear()
        => _dictionary.Clear();

    /// <inheritdoc/>
    bool ICollection<KeyValuePair<TVariant, TUrn>>.Contains(KeyValuePair<TVariant, TUrn> item)
        => ((ICollection<KeyValuePair<TVariant, TUrn>>)_dictionary).Contains(item);

    /// <inheritdoc/>
    bool ICollection<TUrn>.Contains(TUrn item)
        => ((ICollection<KeyValuePair<TVariant, TUrn>>)_dictionary).Contains(KeyValuePair.Create(item.UrnType, item));

    /// <inheritdoc/>
    bool ICollection<KeyValuePair<TVariant, TUrn>>.Remove(KeyValuePair<TVariant, TUrn> item)
        => ((ICollection<KeyValuePair<TVariant, TUrn>>)_dictionary).Remove(item);

    /// <inheritdoc/>
    bool ICollection<TUrn>.Remove(TUrn item)
        => ((ICollection<KeyValuePair<TVariant, TUrn>>)_dictionary).Remove(KeyValuePair.Create(item.UrnType, item));

    /// <inheritdoc/>
    public bool Remove(TVariant key)
        => _dictionary.Remove(key);

    /// <inheritdoc/>
    void ICollection<KeyValuePair<TVariant, TUrn>>.CopyTo(KeyValuePair<TVariant, TUrn>[] array, int arrayIndex)
        => ((ICollection<KeyValuePair<TVariant, TUrn>>)_dictionary).CopyTo(array, arrayIndex);

    /// <inheritdoc/>
    void ICollection<TUrn>.CopyTo(TUrn[] array, int arrayIndex)
        => _dictionary.Values.CopyTo(array, arrayIndex);

    /// <inheritdoc/>
    IEnumerator<TUrn> IEnumerable<TUrn>.GetEnumerator()
        => _dictionary.Values.GetEnumerator();

    /// <inheritdoc/>
    IEnumerator<KeyValuePair<TVariant, TUrn>> IEnumerable<KeyValuePair<TVariant, TUrn>>.GetEnumerator()
        => _dictionary.GetEnumerator();

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator()
        => GetEnumerator();

    /// <inheritdoc/>
    public Dictionary<TVariant, TUrn>.ValueCollection.Enumerator GetEnumerator()
        => _dictionary.Values.GetEnumerator();

    /// <inheritdoc/>
    public Dictionary<TVariant, TUrn>.ValueCollection Values => _dictionary.Values;

    /// <inheritdoc/>
    ICollection<TVariant> IDictionary<TVariant, TUrn>.Keys => _dictionary.Keys;

    /// <inheritdoc/>
    IEnumerable<TVariant> IReadOnlyDictionary<TVariant, TUrn>.Keys => _dictionary.Keys;

    /// <inheritdoc/>
    public Dictionary<TVariant, TUrn>.KeyCollection Keys => _dictionary.Keys;

    /// <inheritdoc/>
    ICollection<TUrn> IDictionary<TVariant, TUrn>.Values => _dictionary.Values;

    /// <inheritdoc/>
    IEnumerable<TUrn> IReadOnlyDictionary<TVariant, TUrn>.Values => _dictionary.Values;
}

/// <summary>
/// Defines a debug view for a <see cref="KeyValueUrnDictionary{TUrn, TVariant}"/> by a debugger.
/// </summary>
/// <typeparam name="TUrn">The urn type.</typeparam>
/// <typeparam name="TVariant">The urn variant enum type.</typeparam>
internal sealed class UrnDictionaryDebugView<TUrn, TVariant>
    where TUrn : IKeyValueUrn<TUrn, TVariant>
    where TVariant : struct, Enum
{
    private readonly KeyValueUrnDictionary<TUrn, TVariant> _dict;

    public UrnDictionaryDebugView(KeyValueUrnDictionary<TUrn, TVariant> dictionary)
    {
        ArgumentNullException.ThrowIfNull(dictionary);

        _dict = dictionary;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
    public DebugViewUrnDictionaryItem<TUrn, TVariant>[] Items
    {
        get
        {
            var keyValuePairs = new KeyValuePair<TVariant, TUrn>[_dict.Count];
            ((IDictionary<TVariant, TUrn>)_dict).CopyTo(keyValuePairs, 0);
            var items = new DebugViewUrnDictionaryItem<TUrn, TVariant>[keyValuePairs.Length];
            for (int i = 0; i < items.Length; i++)
            {
                items[i] = new DebugViewUrnDictionaryItem<TUrn, TVariant>(keyValuePairs[i]);
            }
            return items;
        }
    }
}

/// <summary>
/// Defines a key/value pair for displaying an item of a <see cref="KeyValueUrnDictionary{TUrn, TVariant}"/> by a debugger.
/// </summary>
/// <typeparam name="TUrn">The urn type.</typeparam>
/// <typeparam name="TVariant">The urn variant enum type.</typeparam>
[DebuggerDisplay("{Value}", Name = "[{Key}]")]
internal readonly struct DebugViewUrnDictionaryItem<TUrn, TVariant>
    where TUrn : IKeyValueUrn<TUrn, TVariant>
    where TVariant : struct, Enum
{
    public DebugViewUrnDictionaryItem(KeyValuePair<TVariant, TUrn> keyValue)
    {
        Key = keyValue.Key;
        Value = keyValue.Value;
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
    public TVariant Key { get; }

    [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
    public TUrn Value { get; }
}
