using System.Collections.Immutable;

namespace Altinn.Authorization.RepoCtl.Model.Utils;

internal static class ArrayHelpers
{
    extension<T>(ImmutableArray<T>.Builder builder)
    {
        public void SortBy<U>(Func<T, U> keySelector, IComparer<U>? comparer = null)
        {
            comparer ??= Comparer<U>.Default;
            builder.Sort((a, b) => comparer.Compare(keySelector(a), keySelector(b)));
        }
    }

    extension<T>(ImmutableArray<T> array)
    {
        public int BinarySearchBy<TProj>(Func<T, TProj> keySelector, TProj key)
            where TProj : IComparable<TProj>, allows ref struct
            => array.AsSpan().BinarySearchBy(keySelector, key);
    }

    extension<T>(ReadOnlySpan<T> span)
    {
        public int BinarySearchBy<TProj>(Func<T, TProj> keySelector, TProj key)
            where TProj : IComparable<TProj>, allows ref struct
            => span.BinarySearch(new ProjectedComparer<T, TProj>(keySelector, key, default(SelfComparer<TProj>)));
    }

    private readonly ref struct ProjectedComparer<T, TProj>
        : IComparable<T>
        where TProj : allows ref struct
    {
        private readonly Func<T, TProj> _keySelector;
        private readonly TProj _key;
        private readonly IComparer<TProj> _comparer;

        public ProjectedComparer(Func<T, TProj> keySelector, TProj key, IComparer<TProj> comparer)
        {
            _keySelector = keySelector;
            _key = key;
            _comparer = comparer;
        }

        public int CompareTo(T? other)
        {
            if (other is null)
            {
                return 1;
            }

            return _comparer.Compare(_key, _keySelector(other));
        }
    }

    private readonly struct SelfComparer<T>
        : IComparer<T>
        where T : IComparable<T>, allows ref struct
    {
        public int Compare(T? x, T? y)
        {
            if (x is null)
            {
                return y is null ? 0 : -1;
            }

            if (y is null)
            {
                return 1;
            }

            return x.CompareTo(y);
        }
    }
}
