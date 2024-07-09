using CommunityToolkit.Diagnostics;
using System.Buffers;
using System.Collections;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Altinn.Authorization.ProblemDetails;

/// <summary>
/// Extensions for <see cref="ProblemInstance"/>s and <see cref="ValidationErrorInstance"/>s.
/// </summary>
[DebuggerDisplay("Count = {DebuggerLength}")]
[DebuggerTypeProxy(typeof(ProblemExtensionDataDebuggerProxy))]
[CollectionBuilder(typeof(ProblemExtensionData), nameof(Create))]
public readonly struct ProblemExtensionData
    : IEnumerable<KeyValuePair<string, string>>
    , IReadOnlyCollection<KeyValuePair<string, string>>
    , IReadOnlyDictionary<string, string>
    , IDictionary<string, object?> // Allows using ProblemExtensionData directly in ProblemDetails.
    , IEquatable<ProblemExtensionData>
    , IEqualityOperators<ProblemExtensionData, ProblemExtensionData, bool>
{
    /// <summary>
    /// Gets an empty <see cref="ProblemExtensionData"/>.
    /// </summary>
    public static ProblemExtensionData Empty
        => new(ImmutableArray<KeyValuePair<string, string>>.Empty);

    /// <summary>
    /// Creates a new <see cref="ProblemExtensionData"/> with the specified <paramref name="values"/>.
    /// </summary>
    /// <param name="values">The extension data values.</param>
    /// <returns>A new <see cref="ProblemExtensionData"/>.</returns>
    public static ProblemExtensionData Create(ReadOnlySpan<KeyValuePair<string, string>> values)
        => new(ImmutableArray.Create(values));

    private readonly ImmutableArray<KeyValuePair<string, string>> _values;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProblemExtensionData"/> struct.
    /// </summary>
    /// <param name="values">The extension values.</param>
    public ProblemExtensionData(ImmutableArray<KeyValuePair<string, string>> values)
    {
        _values = values;
    }

    /// <summary>
    /// Gets a value indicating whether this struct was initialized without an actual array instance.
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public bool IsDefault
        => _values.IsDefault;

    /// <summary>
    /// Gets a value indicating whether this struct is empty or uninitialized.
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public bool IsDefaultOrEmpty
        => _values.IsDefaultOrEmpty;

    /// <summary>
    /// Gets the number of elements in the array.
    /// </summary>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public int Length
        => _values.Length;

    [ExcludeFromCodeCoverage]
    private int DebuggerLength
        => _values.IsDefault ? 0 : _values.Length;

    /// <summary>
    /// Creates a new read-only span over this immutable array.
    /// </summary>
    /// <returns>The read-only span representation of this immutable array.</returns>
    public ReadOnlySpan<KeyValuePair<string, string>> AsSpan() => _values.AsSpan();

    /// <summary>
    /// Creates a new read-only memory region over this immutable array.
    /// </summary>
    /// <returns>The read-only memory representation of this immutable array.</returns>
    public ReadOnlyMemory<KeyValuePair<string, string>> AsMemory() => _values.AsMemory();

    /// <summary>
    /// Implicitly converts a <see cref="ImmutableArray{T}"/> of <see cref="KeyValuePair{TKey, TValue}"/> to a <see cref="ProblemExtensionData"/>.
    /// </summary>
    /// <param name="values">The extension data.</param>
    public static implicit operator ProblemExtensionData(ImmutableArray<KeyValuePair<string, string>> values)
        => new(values);

    /// <inheritdoc/>
    public static bool operator ==(ProblemExtensionData left, ProblemExtensionData right)
        => left.Equals(right);

    /// <inheritdoc/>
    public static bool operator !=(ProblemExtensionData left, ProblemExtensionData right)
        => !left.Equals(right);

    /// <inheritdoc/>
    public ValuesEnumerable this[string key]
        => new ValuesEnumerable(this, key);

    /// <inheritdoc/>
    public bool TryGetValue(string key, [NotNullWhen(true)] out string? value)
    {
        foreach (var kvp in _values)
        {
            if (string.Equals(key, kvp.Key, StringComparison.Ordinal))
            {
                value = kvp.Value;
                return true;
            }
        }

        value = null;
        return false;
    }

    /// <inheritdoc/>
    public bool ContainsKey(string key)
        => TryGetValue(key, out _);

    /// <inheritdoc/>
    public override bool Equals([NotNullWhen(true)] object? obj)
        => obj is ProblemExtensionData other && Equals(other);

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        if (_values.IsDefaultOrEmpty)
        {
            return 0;
        }

        var hash = new HashCode();
        hash.Add(_values.Length);

        // simple case, no sorting needed
        if (_values.Length == 1)
        {
            var ext = _values[0];
            hash.Add(string.GetHashCode(ext.Key, StringComparison.Ordinal));
            hash.Add(string.GetHashCode(ext.Value, StringComparison.Ordinal));
        }
        else
        {
            var scratch = ArrayPool<(string Key, string Value, int Index)>.Shared.Rent(_values.Length);
            try
            {
                for (var i = 0; i < _values.Length; i++)
                {
                    var (key, value) = _values[i];
                    scratch[i] = (key, value, i);
                }

                var span = scratch.AsSpan(0, _values.Length);
                span.Sort((a, b) => string.Compare(a.Key, b.Key, StringComparison.Ordinal) switch
                    {
                        0 => string.Compare(a.Value, b.Value, StringComparison.Ordinal),
                        var result => result,
                    });

                foreach (var (key, value, index) in span)
                {
                    hash.Add(string.GetHashCode(key, StringComparison.Ordinal));
                    hash.Add(string.GetHashCode(value, StringComparison.Ordinal));
                }
            }
            finally
            {
                ArrayPool<(string Key, string Value, int Index)>.Shared.Return(scratch);
            }
        }

        return hash.ToHashCode();
    }

    /// <inheritdoc/>
    public bool Equals(ProblemExtensionData other)
    {
        if (!_values.IsDefaultOrEmpty)
        {
            if (other._values.IsDefaultOrEmpty)
            {
                return false;
            }

            if (_values.Length != other._values.Length)
            {
                return false;
            }

            // simple case
            if (_values.Length == 1)
            {
                if (!string.Equals(other._values[0].Key, _values[0].Key, StringComparison.Ordinal))
                {
                    return false;
                }

                if (!string.Equals(other._values[0].Value, _values[0].Value, StringComparison.Ordinal))
                {
                    return false;
                }
            }
            else if (_values.Length <= 16)
            {
                Span<int> seen = stackalloc int[16];
                return CheckWithSeen(seen[.._values.Length], _values.AsSpan(), other._values.AsSpan());
            }
            else
            {
                var rented = ArrayPool<int>.Shared.Rent(_values.Length);
                try
                {
                    Span<int> seen = rented.AsSpan(0, _values.Length);
                    return CheckWithSeen(seen, _values.AsSpan(), other._values.AsSpan());
                }
                finally
                {
                    ArrayPool<int>.Shared.Return(rented);
                }
            }
        }
        else
        {
            return other._values.IsDefaultOrEmpty;
        }

        return true;

        static bool CheckWithSeen(Span<int> seen, ReadOnlySpan<KeyValuePair<string, string>> left, ReadOnlySpan<KeyValuePair<string, string>> right)
        {
            Debug.Assert(left.Length == right.Length);
            Debug.Assert(left.Length == seen.Length);

            var index = 0;
            foreach (var (key, value) in left)
            {
                if (!TryFind(right, key, value, seen[..index], out var foundIndex))
                {
                    return false;
                }

                seen[index++] = foundIndex;
            }

            return true;
        }

        static bool TryFind(ReadOnlySpan<KeyValuePair<string, string>> right, string key, string value, ReadOnlySpan<int> ignore, out int index)
        {
            for (var i = 0; i < right.Length; i++)
            {
                if (ignore.Contains(i))
                {
                    continue;
                }

                var (rightKey, rightValue) = right[i];
                if (string.Equals(key, rightKey, StringComparison.Ordinal)
                    && string.Equals(value, rightValue, StringComparison.Ordinal))
                {
                    index = i;
                    return true;
                }
            }

            index = default;
            return false;
        }
    }

    /// <summary>
    /// Returns an enumerator for the contents of the array.
    /// </summary>
    /// <returns>An enumerator.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ImmutableArray<KeyValuePair<string, string>>.Enumerator GetEnumerator() => _values.GetEnumerator();

    bool ICollection<KeyValuePair<string, object?>>.IsReadOnly => true;

    int ICollection<KeyValuePair<string, object?>>.Count => _values.Length;

    int IReadOnlyCollection<KeyValuePair<string, string>>.Count => _values.Length;

    IEnumerator IEnumerable.GetEnumerator() => (_values as IEnumerable).GetEnumerator();

    IEnumerator<KeyValuePair<string, string>> IEnumerable<KeyValuePair<string, string>>.GetEnumerator()
        => (_values as IEnumerable<KeyValuePair<string, string>>).GetEnumerator();

    IEnumerator<KeyValuePair<string, object?>> IEnumerable<KeyValuePair<string, object?>>.GetEnumerator()
        => _values.Select(static kvp => new KeyValuePair<string, object?>(kvp.Key, kvp.Value)).GetEnumerator();

    IEnumerable<string> IReadOnlyDictionary<string, string>.Keys => new KeysCollection(this);

    ICollection<string> IDictionary<string, object?>.Keys => new KeysCollection(this);

    IEnumerable<string> IReadOnlyDictionary<string, string>.Values => new ValuesCollection(this);

    ICollection<object?> IDictionary<string, object?>.Values => new ValuesCollection(this);

    object? IDictionary<string, object?>.this[string key]
    {
        get => this[key];
        set => ThrowHelper.ThrowNotSupportedException();
    }

    string IReadOnlyDictionary<string, string>.this[string key]
        => TryGetValue(key, out var value)
        ? value
        : ThrowHelper.ThrowArgumentOutOfRangeException<string>(nameof(key));

    void IDictionary<string, object?>.Add(string key, object? value) => ThrowHelper.ThrowNotSupportedException();

    bool IDictionary<string, object?>.Remove(string key) => ThrowHelper.ThrowNotSupportedException<bool>();

    bool ICollection<KeyValuePair<string, object?>>.Remove(KeyValuePair<string, object?> item) => ThrowHelper.ThrowNotSupportedException<bool>();

    void ICollection<KeyValuePair<string, object?>>.Add(KeyValuePair<string, object?> item) => ThrowHelper.ThrowNotSupportedException();

    void ICollection<KeyValuePair<string, object?>>.Clear() => ThrowHelper.ThrowNotSupportedException();

    bool ICollection<KeyValuePair<string, object?>>.Contains(KeyValuePair<string, object?> item)
        => item.Value is string itemValue
        && TryGetValue(item.Key, out var value)
        && string.Equals(itemValue, value, StringComparison.Ordinal);

    bool IDictionary<string, object?>.ContainsKey(string key) => ContainsKey(key);

    bool IDictionary<string, object?>.TryGetValue(string key, out object? value)
    {
        if (TryGetValue(key, out var stringValue))
        {
            value = stringValue;
            return true;
        }

        value = null;
        return false;
    }

    void ICollection<KeyValuePair<string, object?>>.CopyTo(KeyValuePair<string, object?>[] array, int arrayIndex)
    {
        Guard.HasSizeGreaterThanOrEqualTo(array, arrayIndex + _values.Length);

        foreach (var kvp in _values)
        {
            array[arrayIndex++] = new KeyValuePair<string, object?>(kvp.Key, kvp.Value);
        }
    }

    private class ValuesCollection
        : ICollection<object?> // IDictionary<string, object?>.Values
        , IEnumerable<string> // IReadOnlyDictionary<string, string>.Values
    {
        private readonly ProblemExtensionData _values;

        public ValuesCollection(ProblemExtensionData values)
        {
            _values = values;
        }

        int ICollection<object?>.Count => _values.Length;

        bool ICollection<object?>.IsReadOnly => true;

        void ICollection<object?>.Add(object? item) => ThrowHelper.ThrowNotSupportedException();

        void ICollection<object?>.Clear() => ThrowHelper.ThrowNotSupportedException();

        bool ICollection<object?>.Contains(object? item)
            => item is string stringItem
            && _values._values.Any(kvp => string.Equals(kvp.Value, stringItem, StringComparison.Ordinal));

        void ICollection<object?>.CopyTo(object?[] array, int arrayIndex)
        {
            Guard.HasSizeGreaterThanOrEqualTo(array, arrayIndex + _values.Length);

            foreach (var kvp in _values._values)
            {
                array[arrayIndex++] = kvp.Value;
            }
        }

        IEnumerator<object?> IEnumerable<object?>.GetEnumerator()
            => _values._values.Select(static kvp => (object?)kvp.Value).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => _values._values.Select(static kvp => kvp.Value).GetEnumerator();

        IEnumerator<string> IEnumerable<string>.GetEnumerator()
            => _values._values.Select(static kvp => kvp.Value).GetEnumerator();

        bool ICollection<object?>.Remove(object? item) => ThrowHelper.ThrowNotSupportedException<bool>();
    }

    private class KeysCollection
        : ICollection<string> // IDictionary<string, object?>.Keys
        , IEnumerable<string> // IReadOnlyDictionary<string, string>.Keys
    {
        private readonly ProblemExtensionData _values;

        public KeysCollection(ProblemExtensionData values)
        {
            _values = values;
        }

        int ICollection<string>.Count => _values.Length;

        bool ICollection<string>.IsReadOnly => true;

        void ICollection<string>.Add(string item) => ThrowHelper.ThrowNotSupportedException();

        void ICollection<string>.Clear() => ThrowHelper.ThrowNotSupportedException();

        bool ICollection<string>.Contains(string item)
            => _values._values.Any(kvp => string.Equals(kvp.Key, item, StringComparison.Ordinal));

        void ICollection<string>.CopyTo(string[] array, int arrayIndex)
        {
            Guard.HasSizeGreaterThanOrEqualTo(array, arrayIndex + _values.Length);

            foreach (var kvp in _values._values)
            {
                array[arrayIndex++] = kvp.Key;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
            => _values._values.Select(static kvp => kvp.Key).GetEnumerator();

        IEnumerator<string> IEnumerable<string>.GetEnumerator()
            => _values._values.Select(static kvp => kvp.Key).GetEnumerator();

        bool ICollection<string>.Remove(string item) => ThrowHelper.ThrowNotSupportedException<bool>();
    }

    /// <summary>
    /// An enumerable of values for a specific key in a <see cref="ProblemExtensionData"/>.
    /// </summary>
    public readonly struct ValuesEnumerable
        : IEnumerable<string>
    {
        private readonly ProblemExtensionData _data;
        private readonly string _key;

        internal ValuesEnumerable(ProblemExtensionData data, string key)
        {
            _data = data;
            _key = key;
        }

        /// <inheritdoc/>
        public ValuesEnumerator GetEnumerator() 
            => new(_key, _data._values.GetEnumerator());

        IEnumerator<string> IEnumerable<string>.GetEnumerator()
            => GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();
    }

    /// <summary>
    /// An enumerator for the values of a specific key in a <see cref="ProblemExtensionData"/>.
    /// </summary>
    public struct ValuesEnumerator
        : IEnumerator<string>
    {
        private readonly string _key;
        private ImmutableArray<KeyValuePair<string, string>>.Enumerator _enumerator;

        internal ValuesEnumerator(string key, ImmutableArray<KeyValuePair<string, string>>.Enumerator enumerator)
        {
            _key = key;
            _enumerator = enumerator;
        }

        /// <inheritdoc/>
        public string Current 
            => _enumerator.Current.Value;

        object IEnumerator.Current 
            => Current;

        /// <inheritdoc/>
        public bool MoveNext()
        {
            while (_enumerator.MoveNext())
            {
                if (string.Equals(_enumerator.Current.Key, _key, StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }

        void IEnumerator.Reset()
        {
        }

        void IDisposable.Dispose()
        {
        }
    }

    /// <summary>
    /// Displays <see cref="ProblemExtensionData"/> in the debugger.
    /// </summary>
    [ExcludeFromCodeCoverage]
    private sealed class ProblemExtensionDataDebuggerProxy
    {
        /// <summary>
        /// The <see cref="ProblemExtensionData"/> being debugged.
        /// </summary>
        private readonly ProblemExtensionData _data;

        /// <summary>
        /// The contents of the <see cref="ProblemExtensionData"/>, cached into an array.
        /// </summary>
        private ProblemExtensionItemDebuggerView[]? _cachedContents;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProblemExtensionDataDebuggerProxy"/> class.
        /// </summary>
        /// <param name="data">The <see cref="ProblemExtensionData"/> to show in the debugger.</param>
        public ProblemExtensionDataDebuggerProxy(ProblemExtensionData data)
        {
            _data = data;

            if (_data.IsDefaultOrEmpty)
            {
                _cachedContents = Array.Empty<ProblemExtensionItemDebuggerView>();
            }
        }

        /// <summary>
        /// Gets the contents of the dictionary for the display in the debugger.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public ProblemExtensionItemDebuggerView[] Contents
            => _cachedContents
            ??= _data._values.Select(ProblemExtensionItemDebuggerView.From).ToArray();
    }

    /// <summary>
    /// Defines a key/value pair for displaying an item of a dictionary by a debugger.
    /// </summary>
    [ExcludeFromCodeCoverage]
    [DebuggerDisplay("{Value}", Name = "[{Key}]")]
    private readonly struct ProblemExtensionItemDebuggerView
    {
        public static ProblemExtensionItemDebuggerView From(KeyValuePair<string, string> value)
        {
            return new(value);
        }

        public ProblemExtensionItemDebuggerView(KeyValuePair<string, string> value)
        {
            Key = value.Key;
            Value = value.Value;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
        public string Key { get; }

        [DebuggerBrowsable(DebuggerBrowsableState.Collapsed)]
        public string Value { get; }
    }
}
