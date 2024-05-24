using CommunityToolkit.Diagnostics;
using System.Buffers;
using System.Diagnostics;
using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Altinn.Authorization.ProblemDetails;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
[JsonConverter(typeof(ErrorCodeJsonConverter))]
public readonly struct ErrorCode
    : IEquatable<ErrorCode>
    , IFormattable
    , ISpanFormattable
    , IUtf8SpanFormattable
    , IEqualityOperators<ErrorCode, ErrorCode, bool>
{
    internal const int NUM_LENGTH = 5;
    internal const int MIN_LENGTH = ErrorCodeDomain.MIN_LENGTH + 1 + NUM_LENGTH;
    internal const int MAX_LENGTH = ErrorCodeDomain.MAX_LENGTH + 1 + NUM_LENGTH;
    private static readonly SearchValues<char> VALID_CHARS
        = SearchValues.Create("-.0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ");

    private readonly string? _value;

    internal ErrorCode(ErrorCodeDomain domain, uint code)
    {
        Guard.IsNotNull(domain);
        Guard.IsLessThan(code, 100_000);

        var domainName = domain.Name.AsSpan();
        var strLength = domainName.Length + 1 + NUM_LENGTH;
        
        Span<char> span = stackalloc char[MAX_LENGTH];
        domainName.CopyTo(span);
        span[domainName.Length] = '-';
        var success = code.TryFormat(span[(domainName.Length + 1)..], out var written, "D5");
        Debug.Assert(success);
        Debug.Assert(written == NUM_LENGTH);

        var length = domainName.Length + 1 + NUM_LENGTH;
        _value = new string(span[..length]);
    }

    private ErrorCode(string value)
    {
        _value = value;
    }

    /// <summary>
    /// Gets a value indicating whether the <see cref="ErrorCode"/> has a value.
    /// </summary>
    public bool HasValue => _value is not null;

    /// <inheritdoc/>
    public bool Equals(ErrorCode other)
        => string.Equals(_value, other._value, StringComparison.Ordinal);

    /// <inheritdoc/>
    public override bool Equals(object? obj)
        => obj is ErrorCode other && Equals(other);

    /// <inheritdoc/>
    public override int GetHashCode()
        => string.GetHashCode(_value, StringComparison.Ordinal);

    /// <inheritdoc/>
    public override string? ToString()
        => _value;

    /// <inheritdoc/>
    public string ToString(string? format, IFormatProvider? formatProvider)
        => _value ?? string.Empty;

    private string DebuggerDisplay
    {
        get
        {
            if (_value is null)
            {
                return "null";
            }

            return $"\"{_value}\"";
        }
    }

    /// <inheritdoc/>
    public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
    {
        if (_value is null)
        {
            charsWritten = 0;
            return true;
        }

        if (destination.Length < _value.Length)
        {
            charsWritten = 0;
            return false;
        }

        _value.AsSpan().CopyTo(destination);
        charsWritten = _value.Length;
        return true;
    }

    /// <inheritdoc/>
    public bool TryFormat(Span<byte> utf8Destination, out int bytesWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
    {
        // We know that ErrorCode is ASCII only, so we can simplify converting to UTF-8
        if (_value is null)
        {
            bytesWritten = 0;
            return true;
        }

        if (utf8Destination.Length < _value.Length)
        {
            bytesWritten = 0;
            return false;
        }

        for (var i = 0; i < _value.Length; i++)
        {
            utf8Destination[i] = (byte)_value[i];
        }

        bytesWritten = _value.Length;
        return true;
    }

    /// <inheritdoc/>
    public static bool operator ==(ErrorCode left, ErrorCode right)
        => left.Equals(right);

    /// <inheritdoc/>
    public static bool operator !=(ErrorCode left, ErrorCode right)
        => !left.Equals(right);

    private sealed class ErrorCodeJsonConverter
        : JsonConverter<ErrorCode>
    {
        public override ErrorCode Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
            {
                return default;
            }

            if (reader.TokenType != JsonTokenType.String)
            {
                throw new JsonException($"Expected a string, found {reader.TokenType}");
            }

            var value = reader.GetString();
            if (value is null)
            {
                return default;
            }

            if (value.Length is > MAX_LENGTH or < MIN_LENGTH)
            {
                throw new JsonException("Invalid ErrorCode");
            }

            if (value.AsSpan().ContainsAnyExcept(VALID_CHARS))
            {
                throw new JsonException("Invalid ErrorCode");
            }

            return new ErrorCode(value);
        }

        public override void Write(Utf8JsonWriter writer, ErrorCode value, JsonSerializerOptions options)
        {
            Span<byte> buffer = stackalloc byte[MAX_LENGTH];
            var success = value.TryFormat(buffer, out var written, format: default, provider: null);
            Debug.Assert(success);

            writer.WriteStringValue(buffer[..written]);
        }
    }
}
