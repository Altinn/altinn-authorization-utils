using CommunityToolkit.Diagnostics;
using System.Collections;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace Altinn.Authorization.ProblemDetails;

/// <summary>
/// Builder for <see cref="ProblemExtensionData"/>.
/// </summary>
public struct ProblemExtensionDataBuilder
    : IDictionary<string, string>
    , IReadOnlyDictionary<string, string>
{
    private CollectionBuilder<(string Key, string Value)> _extensions;

    /// <inheritdoc/>
    public readonly int Count => _extensions.Count;

    /// <inheritdoc/>
    readonly bool ICollection<KeyValuePair<string, string>>.IsReadOnly => false;

    /// <inheritdoc/>
    public string this[string key]
    {
        get
        {
            if (!TryGetValue(key, out var value))
            {
                throw new KeyNotFoundException($"The given key '{key}' was not present in the collection.");
            }
                
            return value;
        }

        set
        {
            foreach (ref var kvp in _extensions.AsSpanUnsafe())
            {
                if (string.Equals(kvp.Key, key, StringComparison.Ordinal))
                {
                    kvp = new(key, value);
                    return;
                }
            }

            _extensions.Add(new(key, value));
        }
    }

    /// <summary>
    /// Creates a <see cref="ProblemExtensionData"/> from this builder.
    /// </summary>
    /// <returns>A new <see cref="ProblemExtensionData"/>.</returns>
    public readonly ProblemExtensionData ToImmutable()
    {
        if (_extensions.Count == 0)
        {
            return ProblemExtensionData.Empty;
        }

        var span = _extensions.Items;
        var builder = ImmutableArray.CreateBuilder<KeyValuePair<string, string>>(span.Length);
        
        foreach (ref readonly var kvp in span)
        {
            builder.Add(new(kvp.Key, kvp.Value));
        }

        return builder.MoveToImmutable();
    }

    /// <inheritdoc/>
    public readonly bool TryGetValue(string key, [MaybeNullWhen(false)] out string value)
    {
        foreach (ref readonly var kvp in _extensions.Items)
        {
            if (string.Equals(kvp.Key, key, StringComparison.Ordinal))
            {
                value = kvp.Value;
                return true;
            }
        }

        value = default;
        return false;
    }

    /// <inheritdoc/>
    public readonly bool ContainsKey(string key)
        => TryGetValue(key, out _);

    /// <inheritdoc/>
    public void Add(string key, string value)
    {
        if (TryGetValue(key, out _))
        {
            ThrowHelper.ThrowArgumentException($"An element with the key '{key}' already exists in the collection.");
        }

        _extensions.Add(new(key, value));
    }

    /// <inheritdoc/>
    public bool Remove(string key)
    {
        Guard.IsNotNull(key);

        var index = 0;
        foreach (ref readonly var kvp in _extensions.Items)
        {
            if (string.Equals(kvp.Key, key, StringComparison.Ordinal))
            {
                _extensions.SwapRemoveAt(index);
                return true;
            }

            index++;
        }

        return false;
    }

    /// <inheritdoc/>
    void ICollection<KeyValuePair<string, string>>.Add(KeyValuePair<string, string> item)
        => Add(item.Key, item.Value);

    /// <inheritdoc/>
    public void Clear()
    {
        _extensions.Clear();
    }

    /// <inheritdoc/>
    bool ICollection<KeyValuePair<string, string>>.Contains(KeyValuePair<string, string> item)
        => TryGetValue(item.Key, out var value)
        && string.Equals(value, item.Value, StringComparison.Ordinal);

    /// <inheritdoc/>
    void ICollection<KeyValuePair<string, string>>.CopyTo(KeyValuePair<string, string>[] array, int arrayIndex)
    {
        Guard.IsNotNull(array);
        Guard.IsGreaterThanOrEqualTo(arrayIndex, 0);
        Guard.HasSizeGreaterThanOrEqualTo(array, arrayIndex + _extensions.Count);

        var items = _extensions.Items;
        for (var i = 0; i < items.Length; i++)
        {
            ref readonly var kvp = ref items[i];
            array[arrayIndex + i] = new(kvp.Key, kvp.Value);
        }
    }

    /// <inheritdoc/>
    bool ICollection<KeyValuePair<string, string>>.Remove(KeyValuePair<string, string> item)
    {
        var index = 0;
        foreach (ref readonly var kvp in _extensions.Items)
        {
            if (string.Equals(kvp.Key, item.Key, StringComparison.Ordinal))
            {
                if (!string.Equals(kvp.Value, item.Value, StringComparison.Ordinal))
                {
                    return false;
                }

                _extensions.SwapRemoveAt(index);
                return true;
            }

            index++;
        }

        return false;
    }

    /// <inheritdoc/>
    public readonly Enumerator GetEnumerator()
        => new(in _extensions);

    /// <inheritdoc/>
    IEnumerator<KeyValuePair<string, string>> IEnumerable<KeyValuePair<string, string>>.GetEnumerator()
        => GetEnumerator();

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator()
        => GetEnumerator();

    /// <inheritdoc/>
    readonly IEnumerable<string> IReadOnlyDictionary<string, string>.Keys
        => _extensions.Select(static kvp => kvp.Key);

    /// <inheritdoc/>
    readonly IEnumerable<string> IReadOnlyDictionary<string, string>.Values
        => _extensions.Select(static kvp => kvp.Value);

    /// <inheritdoc/>
    ICollection<string> IDictionary<string, string>.Keys
        => [.. _extensions.Select(static kvp => kvp.Key)];

    /// <inheritdoc/>
    ICollection<string> IDictionary<string, string>.Values
        => [.. _extensions.Select(static kvp => kvp.Value)];

    /// <summary>
    /// Enumerator for <see cref="ProblemExtensionDataBuilder"/>.
    /// </summary>
    public struct Enumerator
        : IEnumerator<KeyValuePair<string, string>>
    {
        private CollectionBuilderEnumerator<(string Key, string Value)> _inner;

        internal Enumerator(in CollectionBuilder<(string Key, string Value)> inner)
        {
            _inner = inner.GetEnumerator();
        }

        /// <inheritdoc/>
        public readonly KeyValuePair<string, string> Current
        {
            get
            {
                var current = _inner.Current;
                return new(current.Key, current.Value);
            }
        }

        /// <inheritdoc/>
        readonly object IEnumerator.Current => Current;

        /// <inheritdoc/>
        public void Dispose() => _inner.Dispose();

        /// <inheritdoc/>
        public bool MoveNext() => _inner.MoveNext();

        /// <inheritdoc/>
        public void Reset() => _inner.Reset();
    }
}
