using CommunityToolkit.Diagnostics;

namespace Altinn.Authorization.RepoCtl.Model.Utils;

/// <summary>
/// A stream that delegates all operations to an underlying base stream.
/// </summary>
public abstract class DelegatingStream
    : Stream
{
    private readonly Stream _baseStream;

    /// <summary>
    /// Initializes a new instance of the <see cref="DelegatingStream"/> class that wraps the specified base stream.
    /// </summary>
    /// <param name="baseStream">The stream to wrap.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="baseStream"/> is <see langword="null"/>.</exception>
    protected DelegatingStream(Stream baseStream)
    {
        Guard.IsNotNull(baseStream);

        _baseStream = baseStream;
    }

    /// <inheritdoc/>
    public sealed override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback? callback, object? state)
        => _baseStream.BeginRead(buffer, offset, count, callback, state);

    /// <inheritdoc/>
    public sealed override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback? callback, object? state)
        => _baseStream.BeginWrite(buffer, offset, count, callback, state);

    /// <inheritdoc/>
    public sealed override bool CanRead
        => _baseStream.CanRead;

    /// <inheritdoc/>
    public sealed override bool CanWrite
        => _baseStream.CanWrite;

    /// <inheritdoc/>
    public sealed override bool CanSeek
        => _baseStream.CanSeek;

    /// <inheritdoc/>
    public sealed override bool CanTimeout
        => _baseStream.CanTimeout;

    /// <inheritdoc/>
    public sealed override void CopyTo(Stream destination, int bufferSize)
        => _baseStream.CopyTo(destination, bufferSize);

    /// <inheritdoc/>
    public sealed override Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken)
        => _baseStream.CopyToAsync(destination, bufferSize, cancellationToken);

    /// <inheritdoc/>
    protected sealed override void Dispose(bool disposing)
    {
        // Can't call _baseStream.Dispose(bool) as it's protected,
        // so instead if disposing is true we call _baseStream.Dispose,
        // which will in turn call _baseStream.Dispose(true).  If disposing is
        // false, then this is from a derived stream's finalizer, and it shouldn't
        // be calling _baseStream.Dispose(false) anyway; that should be left up
        // to that stream's finalizer, if it has one.
        if (disposing) _baseStream.Dispose();
    }

    /// <inheritdoc/>
    public sealed override ValueTask DisposeAsync()
        => _baseStream.DisposeAsync();

    /// <inheritdoc/>
    public sealed override int EndRead(IAsyncResult asyncResult)
        => _baseStream.EndRead(asyncResult);

    /// <inheritdoc/>
    public sealed override void EndWrite(IAsyncResult asyncResult)
        => _baseStream.EndWrite(asyncResult);

    /// <inheritdoc/>
    public sealed override void Flush()
        => _baseStream.Flush();

    /// <inheritdoc/>
    public sealed override Task FlushAsync(CancellationToken cancellationToken)
        => _baseStream.FlushAsync(cancellationToken);

    /// <inheritdoc/>
    public sealed override long Length
        => _baseStream.Length;

    /// <inheritdoc/>
    public sealed override long Position
    {
        get => _baseStream.Position;
        set => _baseStream.Position = value;
    }

    /// <inheritdoc/>
    public sealed override int Read(byte[] buffer, int offset, int count)
        => _baseStream.Read(buffer, offset, count);

    /// <inheritdoc/>
    public sealed override int Read(Span<byte> buffer)
        => _baseStream.Read(buffer);

    /// <inheritdoc/>
    public sealed override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        => _baseStream.ReadAsync(buffer, offset, count, cancellationToken);

    /// <inheritdoc/>
    public sealed override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        => _baseStream.ReadAsync(buffer, cancellationToken);

    /// <inheritdoc/>
    public sealed override int ReadByte()
        => _baseStream.ReadByte();

    /// <inheritdoc/>
    public sealed override int ReadTimeout
    {
        get => _baseStream.ReadTimeout;
        set => _baseStream.ReadTimeout = value;
    }

    /// <inheritdoc/>
    public sealed override long Seek(long offset, SeekOrigin origin)
        => _baseStream.Seek(offset, origin);

    /// <inheritdoc/>
    public sealed override void SetLength(long value)
        => _baseStream.SetLength(value);

    /// <inheritdoc/>
    public sealed override void Write(byte[] buffer, int offset, int count)
        => _baseStream.Write(buffer, offset, count);

    /// <inheritdoc/>
    public sealed override void Write(ReadOnlySpan<byte> buffer)
        => _baseStream.Write(buffer);

    /// <inheritdoc/>
    public sealed override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        => _baseStream.WriteAsync(buffer, offset, count, cancellationToken);

    /// <inheritdoc/>
    public sealed override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
        => _baseStream.WriteAsync(buffer, cancellationToken);

    /// <inheritdoc/>
    public sealed override void WriteByte(byte value)
        => _baseStream.WriteByte(value);

    /// <inheritdoc/>
    public sealed override int WriteTimeout
    {
        get => _baseStream.WriteTimeout;
        set => _baseStream.WriteTimeout = value;
    }
}
