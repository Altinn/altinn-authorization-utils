using CommunityToolkit.Diagnostics;
using System.Buffers;
using System.Diagnostics;

namespace Altinn.Authorization.ModelUtils.FieldValueRecords.Json;

internal sealed class ArrayPoolWriter<T>
    : IBufferWriter<T>
    , IDisposable
{
    private readonly ArrayPool<T> _pool;
    private T[] _buffer;
    private int _index;

    public ArrayPoolWriter(ArrayPool<T> pool, int initialSize)
    {
        Guard.IsNotNull(pool);
        Guard.IsGreaterThan(initialSize, 0);

        _pool = pool;
        _buffer = _pool.Rent(initialSize);
    }

    public ArrayPoolWriter(int initialSize)
        : this(ArrayPool<T>.Shared, initialSize)
    {
    }

    /// <summary>
    /// Returns the data written to the underlying buffer so far, as a <see cref="ReadOnlySpan{T}"/>.
    /// </summary>
    public ReadOnlySpan<T> WrittenSpan => _buffer.AsSpan(0, _index);

    /// <inheritdoc/>
    public void Dispose()
    {
        if (_buffer.Length > 0)
        {
            _pool.Return(_buffer, clearArray: true);
        }
        
        _buffer = [];
        _index = 0;
    }

    /// <inheritdoc/>
    void IBufferWriter<T>.Advance(int count)
    {
        Guard.IsGreaterThanOrEqualTo(count, 0);

        if (_index > _buffer.Length - count)
        {
            ThrowHelper.ThrowInvalidOperationException("Advanced too far");
        }

        _index += count;
    }

    /// <inheritdoc/>
    Memory<T> IBufferWriter<T>.GetMemory(int sizeHint)
    {
        CheckAndResizeBuffer(sizeHint);
        Debug.Assert(_buffer.Length > _index);
        return _buffer.AsMemory(_index);
    }

    /// <inheritdoc/>
    Span<T> IBufferWriter<T>.GetSpan(int sizeHint)
    {
        CheckAndResizeBuffer(sizeHint);
        Debug.Assert(_buffer.Length > _index);
        return _buffer.AsSpan(_index);
    }

    /// <summary>
    /// Returns the amount of space available that can still be written into without forcing the underlying buffer to grow.
    /// </summary>
    private int FreeCapacity => _buffer.Length - _index;

    private void CheckAndResizeBuffer(int sizeHint)
    {
        Guard.IsGreaterThanOrEqualTo(sizeHint, 0);

        if (sizeHint == 0)
        {
            sizeHint = 1;
        }

        if (sizeHint < FreeCapacity)
        {
            int currentLength = _buffer.Length;

            // Attempt to grow by the larger of the sizeHint and double the current size.
            int growBy = Math.Max(sizeHint, currentLength);

            int newSize = currentLength + growBy;

            if ((uint)newSize > int.MaxValue)
            {
                ThrowHelper.ThrowInvalidOperationException("Cannot grow buffer beyond int.MaxValue");
            }

            ResizeBuffer(_pool, ref _buffer, newSize, _index);
        }

        static void ResizeBuffer(ArrayPool<T> pool, ref T[] buffer, int newSize, int written)
        {
            T[] toReturn = pool.Rent(newSize);
            try
            {
                if (written > 0)
                {
                    Array.Copy(buffer, toReturn, written);
                }

                (buffer, toReturn) = (toReturn, buffer);
            }
            finally
            {
                pool.Return(toReturn, clearArray: true);
            }
        }
    }
}
