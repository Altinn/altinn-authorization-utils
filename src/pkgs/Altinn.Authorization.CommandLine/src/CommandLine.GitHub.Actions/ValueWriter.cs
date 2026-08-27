using System.Buffers;
using System.Text;

namespace Altinn.Authorization.CommandLine.GitHub.Actions;

internal static class ValueWriter
{
    private static readonly Encoding _utf8Encoding
        = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);

    public static async Task WriteValue(string file, string key, string value, CancellationToken cancellationToken)
    {
        var buffer = new ArrayBufferWriter<byte>();
        WriteValue(buffer, key, value);

        await using var stream = File.Open(file, FileMode.Append, FileAccess.Write, FileShare.None);
        await stream.WriteAsync(buffer.WrittenMemory, cancellationToken);
    }

    private static void WriteValue(IBufferWriter<byte> buffer, string key, string value)
    {
        var delimiter = $"ghadelimiter_{Guid.NewGuid():N}";

        Write(buffer, key);
        buffer.Write("<<"u8);
        Write(buffer, delimiter);
        buffer.Write("\n"u8);
        Write(buffer, value);
        buffer.Write("\n"u8);
        Write(buffer, delimiter);
        buffer.Write("\n"u8);
    }

    private static void Write(IBufferWriter<byte> buffer, string value)
    {
        var maxLength = _utf8Encoding.GetMaxByteCount(value.Length);
        var span = buffer.GetSpan(maxLength);
        var bytesWritten = _utf8Encoding.GetBytes(value, span);
        buffer.Advance(bytesWritten);
    }
}
