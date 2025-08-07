using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Altinn.Authorization.ServiceDefaults.Telemetry;

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

        /// <inheritdoc cref="StateInner.Tags"/>
        public IReadOnlyCollection<KeyValuePair<string, object?>> Tags
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _inner.Tags;
        }

        /// <inheritdoc cref="StateInner.Links"/>
        public IReadOnlyCollection<ActivityLink> Links
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _inner.Links;
        }

        /// <inheritdoc cref="StateInner.AddTags(ReadOnlySpan{KeyValuePair{string, object?}})"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddTags(ReadOnlySpan<KeyValuePair<string, object?>> tags)
            => _inner.AddTags(tags);

        /// <inheritdoc cref="StateInner.AddTags(in System.Diagnostics.TagList)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddTags(in System.Diagnostics.TagList tags)
            => _inner.AddTags(in tags);

        /// <inheritdoc cref="StateInner.AddLinks(ReadOnlySpan{ActivityLink})"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddLinks(ReadOnlySpan<ActivityLink> links)
            => _inner.AddLinks(links);

        /// <inheritdoc cref="StateInner.Clear()"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Dispose()
            => _inner.Clear();
    }

    /// <summary>
    /// Additional state to use when creating an <see cref="Activity"/>.
    /// </summary>
    internal sealed class StateInner
    {
        private readonly TagList _tags = new();
        private readonly LinkList _links = new();

        /// <summary>
        /// Gets the set of tags.
        /// </summary>
        public IReadOnlyCollection<KeyValuePair<string, object?>> Tags => _tags;

        /// <summary>
        /// Gets the set of links.
        /// </summary>
        public IReadOnlyCollection<ActivityLink> Links => _links;

        /// <summary>
        /// Adds a tag to the list of tags.
        /// </summary>
        /// <param name="tags">The tags to add.</param>
        public void AddTags(ReadOnlySpan<KeyValuePair<string, object?>> tags)
        {
            _tags.AddRange(tags);
        }

        /// <summary>
        /// Adds a tag to the list of tags.
        /// </summary>
        /// <param name="tags">The tags to add.</param>
        public void AddTags(in System.Diagnostics.TagList tags)
        {
            _tags.AddRange(in tags);
        }

        /// <summary>
        /// Adds links to the list of links.
        /// </summary>
        /// <param name="links">The links to add.</param>
        public void AddLinks(ReadOnlySpan<ActivityLink> links)
        {
            _links.AddRange(links);
        }

        /// <summary>
        /// Resets the state of this object to its initial condition.
        /// </summary>
        public void Clear()
        {
            _tags.Clear();
            _links.Clear();
        }
    }

    private sealed class TagList
        : IReadOnlyCollection<KeyValuePair<string, object?>>
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
            var count = tags.Length;
            if (count == 0)
            {
                return;
            }

            if (HasUnsetTagValue(tags))
            {
                AddRangeSlow(tags);
                return;
            }
            
            Reserve(count);
            tags.CopyTo(_values.AsSpan(_count));
            _count += count;

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

        /// <summary>
        /// Adds tags to the list.
        /// </summary>
        /// <param name="tags">The tags to add.</param>
        public void AddRange(in System.Diagnostics.TagList tags)
        {
            var count = tags.Count;
            if (count == 0)
            {
                return;
            }

            Reserve(count);
            tags.CopyTo(_values.AsSpan(_count));
            _count += count;
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

    private sealed class LinkList
        : IReadOnlyCollection<ActivityLink>
        , IEnumerator<ActivityLink>
    {
        private ActivityLink[] _values = [];
        private int _count;
        private int _pos = -1;

        /// <summary>
        /// Gets the number of links currently in this instance.
        /// </summary>
        public int Count => _count;

        /// <summary>
        /// Allocates some room to put some links.
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
        /// Adds links to the list.
        /// </summary>
        /// <param name="links">The links to add.</param>
        public void AddRange(ReadOnlySpan<ActivityLink> links)
        {
            if (links.Length == 0)
            {
                return;
            }

            Reserve(links.Length);
            links.CopyTo(_values.AsSpan(_count));
            _count += links.Length;
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

        ActivityLink IEnumerator<ActivityLink>.Current => _values[_pos];

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

        IEnumerator<ActivityLink> IEnumerable<ActivityLink>.GetEnumerator()
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
