using System.Buffers;

namespace Altinn.Authorization.RepoCtl.Utils;

internal static class StreamHelpers
{
    private const byte LF = (byte)'\n';
    private const byte CR = (byte)'\r';
    private static readonly SearchValues<byte> _lineEndings = SearchValues.Create([CR, LF]);

    extension(Stream stream)
    {
        public ValueTask WriteAsync(ReadOnlySequence<byte> sequence, CancellationToken cancellationToken = default)
        {
            if (sequence.IsSingleSegment)
            {
                return stream.WriteAsync(sequence.First, cancellationToken);
            }
            else
            {
                return WriteMultiSegment(stream, sequence, cancellationToken);
            }

            static async ValueTask WriteMultiSegment(Stream stream, ReadOnlySequence<byte> sequence, CancellationToken cancellationToken)
            {
                foreach (var segment in sequence)
                {
                    await stream.WriteAsync(segment, cancellationToken);
                }
            }
        }

        public async Task NormalizeLineEndingsAndCopyToAsync(IBufferWriter<byte> destination, CancellationToken cancellationToken = default)
        {
            var prevReadLastByteCr = false;

            while (true)
            {
                var buffer = destination.GetMemory();
                var bytesRead = await stream.ReadAsync(buffer, cancellationToken);
                if (bytesRead == 0)
                {
                    break;
                }

                var lastByteCr = buffer.Span[bytesRead - 1] == CR;
                var firstLineBreak = buffer.Span[..bytesRead].IndexOfAny(_lineEndings);
                if (firstLineBreak >= 0)
                {
                    var span = buffer.Span[..bytesRead];
                    NormalizeLineEndings(ref span, firstLineBreak, prevReadLastByteCr);
                    bytesRead = span.Length;
                }

                prevReadLastByteCr = lastByteCr;
                destination.Advance(bytesRead);
            }
        }
    }

    internal static void NormalizeLineEndings(ref Span<byte> data, int firstLineBreakIndex, bool lastByteWasCr)
    {
        int src = firstLineBreakIndex;
        int dst = firstLineBreakIndex;

        if (lastByteWasCr && data[0] == LF)
        {
            // The previous read ended with a CR and this read starts with an LF, so we need to skip the LF
            src++;

            if (src == data.Length)
            {
                // The entire buffer was just the LF, so we can just return an empty span
                data = [];
                return;
            }

            int next = data[src..].IndexOfAny(_lineEndings);
            if (next < 0)
            {
                // No more line endings - just shift the data down by one and return
                data.Slice(src).CopyTo(data);
                data = data[..^1];
                return;
            }

            data.Slice(src, next).CopyTo(data[dst..]);
            src += next;
            dst += next;
        }

        while (true)
        {
            if (data[src] == CR
                && src + 1 < data.Length
                && data[src + 1] == LF)
            {
                src += 2;
            }
            else
            {
                src++;
            }

            data[dst++] = LF;
            if (src == data.Length)
                break;

            int next = data[src..].IndexOfAny(_lineEndings);
            if (next < 0)
            {
                // No more line endings - copy the rest of the data
                if (dst != src)
                {
                    data[src..].CopyTo(data[dst..]);
                }

                dst += data.Length - src;
                break;
            }

            if (dst != src)
            {
                data.Slice(src, next).CopyTo(data[dst..]);
            }

            src += next;
            dst += next;
        }

        data = data[..dst];
    }
}
