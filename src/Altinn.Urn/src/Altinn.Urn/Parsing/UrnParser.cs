using System.Buffers;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Altinn.Urn.Parsing;

internal ref struct UrnParser
{
    public static UrnParseResult Parse(ReadOnlySpan<byte> data)
    {
        UrnParser parser = new(data, source: null);
        return parser.Parse();
    }

    public static UrnParseResult Parse(ReadOnlySpan<char> data)
        => Parse(data, source: null);

    public static UrnParseResult Parse(string data)
        => Parse(data.AsSpan(), source: data);

    private static UrnParseResult Parse(ReadOnlySpan<char> data, string? source)
    {
        if (!AllAscii(data))
        {
            return UrnParseError.NotAscii;
        }

        var buffer = ArrayPool<byte>.Shared.Rent(data.Length);
        try
        {
            var span = buffer.AsSpan(0, data.Length);
            ConvertToBytes(data, span);
            UrnParser parser = new(span, source);
            return parser.Parse();
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }

        static bool AllAscii(ReadOnlySpan<char> data)
            => data.ContainsAnyExceptInRange(lowInclusive: (char)32, highInclusive: (char)126);

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        static void ConvertToBytes(ReadOnlySpan<char> source, Span<byte> dest)
        {
            Debug.Assert(source.Length == dest.Length);

            int length = source.Length;
            int remaining = length % (Vector<byte>.Count * 2);
            int lastVectorIndex = length - remaining;
            var sourceConverted = MemoryMarshal.Cast<char, ushort>(source);

            int i;
            for (i = 0; i < lastVectorIndex; i += Vector<byte>.Count * 2)
            {
                var lowVec = new Vector<ushort>(sourceConverted.Slice(i));
                var highVec = new Vector<ushort>(sourceConverted.Slice(i + Vector<ushort>.Count));
                var destVec = Vector.Narrow(lowVec, highVec);
                destVec.CopyTo(dest.Slice(i));
            }

            for (; i < length; i++)
            {
                dest[i] = (byte)sourceConverted[i];
            }

            Debug.Assert(i == length);
        }
    }

    private readonly ReadOnlySpan<byte> _data;
    private readonly string? _source;
    private ReadOnlySpan<byte> _rest;
    private ParseState _state;
    private Range? _namespaceIdentifier;
    private Range? _namespaceSpecificString;
    private Range? _rComponent;
    private Range? _qComponent;
    private Range? _fComponent;

    private UrnParser(ReadOnlySpan<byte> data, string? source)
    {
        _data = data;
        _source = source;
        _rest = data;
        _state = ParseState.AssignedName;
    }

    private enum ParseState
        : byte
    {
        AssignedName,
        RComponent,
        QComponent,
        FComponent,
    }

    private UrnParseResult Parse()
    {
        throw new NotImplementedException();
    }
}

internal enum UrnParseError
    : byte
{
    Ok = 0,
    NotAscii,
    TooShort,
    TooLong,
    MissingUrnPrefix,
    InvalidNamespaceIdentifier,
    InvalidNamespaceSpecificString,
    InvalidRComponent,
    InvalidQComponent,
    InvalidFComponent,
}

internal readonly struct UrnParseResult(UrnParseError error, Urn? value)
{
    public UrnParseError Error { get; } = error;
    public Urn? Value { get; } = value;

    public bool TryGetValue([MaybeNullWhen(false)] out Urn value)
    {
        value = Value;
        return Error == UrnParseError.Ok;
    }

    [MemberNotNullWhen(false, nameof(Value))]
    public bool IsError
        => Error != UrnParseError.Ok;

    public static implicit operator UrnParseResult(Urn value)
        => new(UrnParseError.Ok, value);

    public static implicit operator UrnParseResult(UrnParseError error)
        => new(error, null);
}
