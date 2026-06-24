using System.Buffers;
using Microsoft.Extensions.DependencyInjection;

namespace Altinn.Authorization.ProblemDetails.PathUtils;

internal ref struct PathSegmentIterator
{
    private static readonly SearchValues<char> _modelStateKeySeparators
        = SearchValues.Create(['.', '[', ']']);

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
        if (_remaining.IsEmpty)
        {
            return false;
        }

        var index = _remaining.IndexOfAny(_modelStateKeySeparators);
        while (index == 0)
        {
            _remaining = _remaining[1..];
            index = _remaining.IndexOfAny(_modelStateKeySeparators);
        }

        if (index < 0)
        {
            // assuming well-formated path, if we have no more separators, this should be a property name
            // else, we should have a trailing ']'
            _current = new PathSegment(_remaining, PathSegmentType.Property, _remaining);
            _remaining = [];
            return true;
        }

        // handle `].`
        if (_remaining[index] == ']' && _remaining.Length > index + 1 && _remaining[index + 1] == '.')
        {
            _current = new PathSegment(_remaining[..index], PathSegmentType.Indexer, _remaining);
            _remaining = _remaining[(index + 2)..];
            return true;
        }

        _current = new PathSegment(_remaining[..index], PathSegmentType.Property, _remaining);
        _remaining = _remaining[(index + 1)..];
        return true;
    }
}
