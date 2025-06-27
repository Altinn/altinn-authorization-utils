using System.Buffers;

namespace Altinn.Authorization.ModelUtils.Tests.Utils;

internal static class Sequence
{
    public static ReadOnlySequence<byte> CreateFullSegmented(Memory<byte> data)
    {
        SimpleSegment start = new(Array.Empty<byte>(), 0);
        SimpleSegment current = start;

        for (var i = 0; i < data.Length; i++)
        {
            SimpleSegment next = new(data.Slice(i, 1), i);
            current.NextSegment = next;
            current = next;
        }

        return new ReadOnlySequence<byte>(start, 0, current, current.Memory.Length);
    }

    private sealed class SimpleSegment
        : ReadOnlySequenceSegment<byte>
    {
        public SimpleSegment(Memory<byte> memory, int offset)
        {
            Memory = memory;
            RunningIndex = offset;
        }

        public SimpleSegment NextSegment
        {
            set => Next = value;
        }
    }
}
