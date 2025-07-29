using System.Buffers;
using System.Diagnostics.CodeAnalysis;

namespace Altinn.Cli.Jwks;

[ExcludeFromCodeCoverage]
internal static class StreamExtensions
{
    public static ValueTask WriteAsync(this Stream stream, ReadOnlySequence<byte> data, CancellationToken cancellationToken = default)
    {
        if (data.IsSingleSegment)
        {
            return stream.WriteAsync(data.First, cancellationToken);
        }

        return WriteMany(stream, data, cancellationToken);

        static async ValueTask WriteMany(Stream stream, ReadOnlySequence<byte> data, CancellationToken cancellationToken)
        {
            foreach (var segment in data)
            {
                await stream.WriteAsync(segment, cancellationToken);
            }
        }
    }
}
