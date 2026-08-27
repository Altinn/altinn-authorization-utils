using System.Collections;

namespace Altinn.Authorization.CommandLine.Utils;

internal sealed class ReadOnlyCollectionWrapper<T>
    : IReadOnlyCollection<T>
{
    private readonly ICollection<T> _collection;

    public ReadOnlyCollectionWrapper(ICollection<T> collection)
    {
        _collection = collection;
    }

    /// <inheritdoc/>
    public int Count
        => _collection.Count;

    /// <inheritdoc/>
    public IEnumerator<T> GetEnumerator()
        => _collection.GetEnumerator();

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator()
        => _collection.GetEnumerator();
}
