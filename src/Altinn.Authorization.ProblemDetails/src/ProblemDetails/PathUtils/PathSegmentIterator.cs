using System.Buffers;

namespace Altinn.Authorization.ProblemDetails.PathUtils;

internal ref struct PathSegmentIterator
{
    private static readonly SearchValues<char> _modelStateKeySeparators
        = SearchValues.Create(['.', '[', ']']);

    private static readonly SearchValues<char> _propertySegmentSeparators
        = SearchValues.Create(['.', '[']);

    public static ReadOnlySpan<char> GetInitialSegment(ReadOnlySpan<char> key)
    {
        var index = key.IndexOfAny(_modelStateKeySeparators);
        if (index < 0)
        {
            return key;
        }

        return key[..index];
    }

    public static PathSegmentIterator Create(ReadOnlySpan<char> key)
        => new(key);

    private ReadOnlySpan<char> _remaining;
    private PathSegment _current;

    private PathSegmentIterator(ReadOnlySpan<char> path)
    {
        _remaining = path;
        _current = default;
    }

    public readonly PathSegmentIterator GetEnumerator()
        => this;

    public readonly PathSegment Current
        => _current;

    public readonly ReadOnlySpan<char> Remainder
        => _remaining;

    public bool MoveNext()
    {
        while (!_remaining.IsEmpty && _remaining[0] is '.' or ']')
        {
            _remaining = _remaining[1..];
        }

        if (_remaining.IsEmpty)
        {
            return false;
        }

        var remainder = _remaining;
        if (_remaining[0] is '[')
        {
            var closingBracketIndex = _remaining[1..].IndexOf(']');
            if (closingBracketIndex < 0)
            {
                _current = new PathSegment(_remaining[1..], PathSegmentType.Indexer, remainder);
                _remaining = [];
                return true;
            }

            closingBracketIndex += 1;
            _current = new PathSegment(_remaining[1..closingBracketIndex], PathSegmentType.Indexer, remainder);
            _remaining = _remaining[(closingBracketIndex + 1)..];
            return true;
        }

        var index = _remaining.IndexOfAny(_propertySegmentSeparators);
        if (index < 0)
        {
            _current = new PathSegment(_remaining, PathSegmentType.Property, remainder);
            _remaining = [];
            return true;
        }

        _current = new PathSegment(_remaining[..index], PathSegmentType.Property, remainder);
        _remaining = _remaining[index..];
        return true;
    }
}
