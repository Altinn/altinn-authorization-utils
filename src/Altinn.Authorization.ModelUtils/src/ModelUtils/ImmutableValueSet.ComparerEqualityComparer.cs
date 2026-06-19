using CommunityToolkit.Diagnostics;

namespace Altinn.Authorization.ModelUtils;

public sealed partial class ImmutableValueSet<T>
{
    private sealed class ComparerEqualityComparer
        : IEqualityComparer<T>
    {
        public static IEqualityComparer<T> Create(IComparer<T> comparer)
        {
            if (comparer == Comparer<T>.Default)
            {
                return EqualityComparer<T>.Default;
            }

            return new ComparerEqualityComparer(comparer);
        }

        private readonly IComparer<T> _comparer;

        private ComparerEqualityComparer(IComparer<T> comparer)
        {
            _comparer = comparer;
        }

        /// <inheritdoc/>
        public bool Equals(T? x, T? y)
            => _comparer.Compare(x, y) == 0;

        /// <inheritdoc/>
        public int GetHashCode(T obj)
            => ThrowHelper.ThrowNotSupportedException<int>("GetHashCode is not supported by ComparerEqualityComparer because it is only used for equality comparison, and does not produce consistent hash codes.");
    }
}
