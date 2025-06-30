using Altinn.Authorization.ModelUtils.Tests.Utils.Shouldly;
using CommunityToolkit.Diagnostics;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.Json;

namespace Altinn.Authorization.ModelUtils.Tests.Utils;

public static class Json
{
#if NET9_0_OR_GREATER
    private readonly static JsonSerializerOptions _options = JsonSerializerOptions.Web;
#else
    private readonly static JsonSerializerOptions _options = new(JsonSerializerDefaults.Web);
#endif

    public static JsonSerializerOptions Options => _options;

    public static JsonDocument SerializeToDocument(object? value, Type type)
        => JsonSerializer.SerializeToDocument(value, type, _options);

    public static JsonDocument SerializeToDocument<T>(T value)
        => SerializeToDocument(value, typeof(T));

    public static string SerializeToString(object? value, Type type)
        => JsonSerializer.Serialize(value, type, _options);

    public static string SerializeToString<T>(T value)
        => SerializeToString(value, typeof(T));

    public static T? Deserialize<T>(JsonDocument document)
        => JsonSerializer.Deserialize<T>(document, _options);

    public static object? Deserialize(JsonDocument document, Type type)
        => JsonSerializer.Deserialize(document, type, _options);

    public static T? Deserialize<T>(string document)
        => JsonSerializer.Deserialize<T>(document, _options);

    public static object? Deserialize(string document, Type type)
        => JsonSerializer.Deserialize(document, type, _options);

    public static T? Deserialize<T>(ReadOnlySequence<byte> document)
    {
        var reader = new Utf8JsonReader(document);
        return JsonSerializer.Deserialize<T>(ref reader, _options);
    }

    public static object? Deserialize(ReadOnlySequence<byte> document, Type type)
    {
        var reader = new Utf8JsonReader(document);
        return JsonSerializer.Deserialize(ref reader, type, _options);
    }

    public static async ValueTask CheckRoundTripAsync<T>(T value, [StringSyntax(StringSyntaxAttribute.Json)] string json)
    {
        var serialized = SerializeToDocument(value);

        serialized.ShouldNotBeNull();
        serialized.ShouldBeStructurallyEquivalentTo(json);

        await CheckParsesAsync(json, value);
    }

    public static async ValueTask CheckParsesAsync<T>([StringSyntax(StringSyntaxAttribute.Json)] string json, T value)
    {
        var utf8 = Encoding.UTF8.GetBytes(json);
        var minSegments = new ReadOnlySequence<byte>(utf8);

        var deserialized = Deserialize<T>(minSegments);
        deserialized.ShouldBe(value);

        deserialized = await ParseUsingPartialsAsync<T>(utf8);
        deserialized.ShouldBe(value);
    }

    private static async ValueTask<T?> ParseUsingPartialsAsync<T>(ReadOnlyMemory<byte> utf8)
    {
        var options = new JsonSerializerOptions(_options)
        {
            DefaultBufferSize = 1, // Force partial reads
        };

        using var stream = new RepeatingJsonContentStream(utf8);
        IAsyncEnumerator<T?>? enumerator = null;
        try
        {
            enumerator = JsonSerializer.DeserializeAsyncEnumerable<T>(stream, options).GetAsyncEnumerator();

            if (!await enumerator.MoveNextAsync())
            {
                ThrowHelper.ThrowInvalidOperationException("Async enumeration did not yield any items. This is unexpected in this test context.");
            }

            return enumerator.Current;
        }
        finally
        {
            if (enumerator is { } obj)
            {
                await obj.DisposeAsync();
            }
        }
    }

    // this represents an infinite stream of the same JSON content, wrapped in an array.
    // reading this stream will yield <c>[{item}, {item}, ...</c> forever.
    private class RepeatingJsonContentStream(ReadOnlyMemory<byte> data)
        : Stream
    {
        // -2 = before array start
        // -1 = after single item read
        private int _index = -2;

        public override bool CanRead 
            => true;

        public override bool CanSeek 
            => false;

        public override bool CanWrite 
            => false;

        public override long Length 
            => ThrowHelper.ThrowNotSupportedException<long>();

        public override long Position 
        { 
            get => ThrowHelper.ThrowNotSupportedException<long>(); 
            set => ThrowHelper.ThrowNotSupportedException(); 
        }

        public override void Flush()
        {
            ThrowHelper.ThrowNotSupportedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return Read(buffer.AsSpan(offset, count));
        }

        public override int Read(Span<byte> buffer)
        {
            if (buffer.Length == 0)
            {
                return 0; // not room for any data
            }

            if (_index == -2)
            {
                buffer[0] = "["u8[0];
                _index = 0;
                return 1;
            }

            if (_index == -1)
            {
                buffer[0] = ","u8[0];
                _index = 0;
                return 1;
            }

            var remaining = data.Length - _index;
            var len = Math.Min(remaining, buffer.Length);
            data.Span.Slice(_index, len).CopyTo(buffer);
            _index += len;
            return len;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return ThrowHelper.ThrowNotSupportedException<long>();
        }

        public override void SetLength(long value)
        {
            ThrowHelper.ThrowNotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            ThrowHelper.ThrowNotSupportedException();
        }
    }
}
