using System.Buffers;
using System.Buffers.Text;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Altinn.Cli.Jwks;

[ExcludeFromCodeCoverage]
internal static class Base64Helper
{
    public static void Decode(IBufferWriter<byte> writer, ReadOnlySpan<char> encoded)
    {
        var maxBinLength = Base64.GetMaxDecodedFromUtf8Length(encoded.Length);
        var span = writer.GetSpan(maxBinLength);
        if (!Convert.TryFromBase64Chars(encoded, span, out var written))
        {
            throw new ArgumentException("Invalid base64 data", nameof(encoded));
        }

        writer.Advance(written);
    }

    public static string Encode(ReadOnlySequence<byte> data)
    {
        if (data.IsSingleSegment)
        {
            return Convert.ToBase64String(data.First.Span);
        }

        var length = checked((int)data.Length);
        var scratch = ArrayPool<byte>.Shared.Rent(length);
        try
        {
            data.CopyTo(scratch);
            return Convert.ToBase64String(scratch.AsSpan(0, length));
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(scratch);
        }
    }

    public static void EncodeUtf8(IBufferWriter<byte> utf8Writer, ReadOnlySequence<byte> data)
    {
        int bytesWritten;
        var dataLength = checked((int)data.Length);
        var maxBase64Length = Base64.GetMaxEncodedToUtf8Length(dataLength);
        var dest = utf8Writer.GetSpan(maxBase64Length);
        OperationStatus result;

        if (data.IsSingleSegment)
        {
            result = Base64.EncodeToUtf8(data.FirstSpan, dest, out var bytesConsumed, out bytesWritten, isFinalBlock: true);
            Debug.Assert(bytesConsumed == dataLength);
        }
        else
        {
            // first copy all of the binary data into the write buffer - this is safe, since it will be overwritten in
            // converting to base64 and dest is guaranteed to be large enough to hold the base64 data which is larger
            // than the binary data
            data.CopyTo(dest);
            result = Base64.EncodeToUtf8InPlace(dest, dataLength, out bytesWritten);
        }

        Debug.Assert(result == OperationStatus.Done);
        utf8Writer.Advance(bytesWritten);
    }
}
