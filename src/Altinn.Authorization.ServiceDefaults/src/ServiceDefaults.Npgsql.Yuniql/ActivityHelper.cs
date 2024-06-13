using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Altinn.Authorization.ServiceDefaults.Npgsql.Yuniql;

internal static class ActivityHelper
{
    [ThreadStatic]
    private static StateInner? _state;

    /// <summary>
    /// Gets a thread-local instance of this type.
    /// </summary>
    public static State ThreadLocalState
    {
        get
        {
            ref var result = ref _state;
            result ??= new();

            return new(ref result);
        }
    }

    /// <summary>
    /// Gets a value that can be used to indicate that a tag value should be unset.
    /// </summary>
    public static object UnsetTagValue => UnsetTagValueMarker.Instance;

    /// <summary>
    /// Additional state to use when creating an <see cref="Activity"/>.
    /// </summary>
    public readonly ref struct State
    {
        private readonly ref StateInner _inner;

        internal State(ref StateInner inner)
        {
            _inner = ref inner;
        }

        /// <summary>
        /// Gets the array of tags.
        /// </summary>
        public IEnumerable<KeyValuePair<string, object?>> Tags
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _inner.Tags;
        }

        /// <summary>
        /// Adds a tag to the list of tags.
        /// </summary>
        /// <param name="tags">The tags to add.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddTags(ReadOnlySpan<KeyValuePair<string, object?>> tags)
            => _inner.AddTags(tags);

        /// <summary>
        /// Resets the state of this object to its initial condition.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
            => _inner.Clear();

        /// <summary>
        /// Gets the number of tags currently in this instance.
        /// </summary>
        public int TagsCount
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _inner.TagsCount;
        }
    }

    /// <summary>
    /// Additional state to use when creating an <see cref="Activity"/>.
    /// </summary>
    internal sealed class StateInner
    {
        private TagList _tags = new();

        /// <summary>
        /// Gets the array of tags.
        /// </summary>
        public IEnumerable<KeyValuePair<string, object?>> Tags => _tags;

        /// <summary>
        /// Adds a tag to the list of tags.
        /// </summary>
        /// <param name="tags">The tags to add.</param>
        public void AddTags(ReadOnlySpan<KeyValuePair<string, object?>> tags)
        {
            _tags.AddRange(tags);
        }

        /// <summary>
        /// Resets the state of this object to its initial condition.
        /// </summary>
        public void Clear()
        {
            _tags.Clear();
        }

        /// <summary>
        /// Gets the number of tags currently in this instance.
        /// </summary>
        public int TagsCount => _tags.Count;
    }

    private sealed class TagList
        : IEnumerable<KeyValuePair<string, object?>>
        , IEnumerator<KeyValuePair<string, object?>>
    {
        private KeyValuePair<string, object?>[] _values = [];
        private int _count;
        private int _pos = -1;

        /// <summary>
        /// Gets the number of tags currently in this instance.
        /// </summary>
        public int Count => _count;

        /// <summary>
        /// Allocates some room to put some tags.
        /// </summary>
        /// <param name="count">The amount of space to allocate.</param>
        private void Reserve(int count)
        {
            int avail = _values.Length - _count;
            if (count > avail)
            {
                var need = _values.Length + (count - avail);
                Array.Resize(ref _values, need);
            }
        }

        /// <summary>
        /// Adds tags to the list.
        /// </summary>
        /// <param name="tags">The tags to add.</param>
        public void AddRange(ReadOnlySpan<KeyValuePair<string, object?>> tags)
        {
            if (tags.Length == 0)
            {
                return;
            }

            if (HasUnsetTagValue(tags))
            {
                AddRangeSlow(tags);
                return;
            }
            
            Reserve(tags.Length);
            tags.CopyTo(_values.AsSpan(_count));
            _count += tags.Length;

            static bool HasUnsetTagValue(ReadOnlySpan<KeyValuePair<string, object?>> tags)
            {
                for (var i = 0; i < tags.Length; i++)
                {
                    if (ReferenceEquals(tags[i].Value, UnsetTagValueMarker.Instance))
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        private void AddRangeSlow(ReadOnlySpan<KeyValuePair<string, object?>> tags)
        {
            Debug.Assert(!tags.IsEmpty);
            Reserve(tags.Length);

            for (var i = 0; i < tags.Length; i++)
            {
                if (ReferenceEquals(tags[i].Value, UnsetTagValueMarker.Instance))
                {
                    continue;
                }

                _values[_count++] = tags[i];
            }
        }

        /// <summary>
        /// Resets the state of this object to its initial condition.
        /// </summary>
        public void Clear()
        {
            Array.Clear(_values, 0, _count);
            _count = 0;
            _pos = -1;
        }

        KeyValuePair<string, object?> IEnumerator<KeyValuePair<string, object?>>.Current => _values[_pos];

        object IEnumerator.Current => _values[_pos];

        bool IEnumerator.MoveNext()
        {
            return ++_pos < _count;
        }

        void IDisposable.Dispose()
        {
            _pos = -1;
        }

        void IEnumerator.Reset()
        {
            _pos = -1;
        }

        IEnumerator<KeyValuePair<string, object?>> IEnumerable<KeyValuePair<string, object?>>.GetEnumerator()
        {
            return this;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this;
        }
    }

    [DebuggerDisplay("{ToString(),nq}")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    private sealed class UnsetTagValueMarker
    {
        public readonly static UnsetTagValueMarker Instance = new UnsetTagValueMarker();

        private UnsetTagValueMarker() 
        {
        }

        public override string ToString() 
            => "<unset tag value>";
    }
}
