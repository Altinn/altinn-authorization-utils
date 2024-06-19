namespace Altinn.Urn.SourceGenerator.Utils;

internal static class EquitableArray
{
    public static EquitableArray<T> Empty<T>() => EquitableArray<T>.Empty;

    public static EquitableArray<T>.Builder CreateBuilder<T>() => new();
}

internal readonly struct EquitableArray<T>
    : IEquatable<EquitableArray<T>>
{
    public static readonly EquitableArray<T> Empty = new(Array.Empty<T>());

    private readonly T[] _array;

    private EquitableArray(T[] array)
    {
        _array = array;
    }

    public bool IsDefault
        => _array is null;

    public T this[int index]
        => _array[index];

    public ReadOnlySpan<T> AsSpan()
        => _array.AsSpan();

    public override bool Equals(object? obj)
    {
        return obj is EquitableArray<T> other
            && Equals(other);
    }

    public bool Equals(EquitableArray<T> other)
    {
        if (ReferenceEquals(_array, other._array))
        {
            return true;
        }

        if (_array is null)
        {
            return false;
        }

        if (other._array is null)
        {
            return false;
        }

        return _array.Length == other._array.Length
            && _array.SequenceEqual(other._array, EqualityComparer<T>.Default);
    }

    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(_array.Length);
        foreach (var item in _array)
        {
            hash.Add(item, EqualityComparer<T>.Default);
        }

        return hash.ToHashCode();
    }

    public ReadOnlySpan<T>.Enumerator GetEnumerator()
        => ((ReadOnlySpan<T>)_array.AsSpan()).GetEnumerator();

    public ref struct Builder
    {
        private ValueListBuilder<T> _builder;

        public void Dispose()
        {
            _builder.Dispose();
        }

        public void Add(T item)
        {
            _builder.Append(item);
        }

        public readonly int Length => _builder.Length;

        public EquitableArray<T> ToArray()
        {
            return new(_builder.ToArray());
        }
    }
}
