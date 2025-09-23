using System.Buffers;
using CommunityToolkit.Diagnostics;

namespace Altinn.Authorization.ServiceDefaults.HttpClient;

/// <summary>
/// Url helpers.
/// </summary>
public static class Url
{
    private const int MAX_LENGTH = 1024 * 16;

    /// <summary>
    /// Creates a URL with the specified base URL and query parameters.
    /// </summary>
    /// <param name="baseUrl">The base url.</param>
    /// <param name="parameters">The query parameters.</param>
    /// <returns>The url.</returns>
    public static string Create(string baseUrl, ReadOnlySpan<QueryParam> parameters)
    {
        var buffer = ArrayPool<char>.Shared.Rent(MAX_LENGTH);

        try
        {
            if (!TryCreate(baseUrl, parameters, buffer, out var written))
            {
                ThrowHelper.ThrowInvalidOperationException("Failed to create URL.");
            }

            if (written == baseUrl.Length && buffer.AsSpan(0, written).SequenceEqual(baseUrl))
            {
                return baseUrl;
            }

            return new(buffer, 0, written);
        }
        finally
        {
            ArrayPool<char>.Shared.Return(buffer, clearArray: true);
        }
    }

    /// <summary>
    /// Tries to create a URL with the specified base URL and query parameters, writing the result to the provided destination span.
    /// </summary>
    /// <param name="baseUrl">The base url.</param>
    /// <param name="parameters">Query parameters.</param>
    /// <param name="destination">Destination span.</param>
    /// <param name="written">When the method completes, if it returns <see langword="true"/>, contains characters written to <paramref name="destination"/>.</param>
    /// <returns>Whether or not the creation succeeded.</returns>
    public static bool TryCreate(ReadOnlySpan<char> baseUrl, ReadOnlySpan<QueryParam> parameters, Span<char> destination, out int written)
    {
        written = 0;

        if (!baseUrl.TryCopyTo(destination))
        {
            return false;
        }

        written += baseUrl.Length;
        var separator = '?';

        for (var i = 0; i < parameters.Length; i++)
        {
            var param = parameters[i];
            if (destination.Length - written < 1)
            {
                return false;
            }

            destination[written] = separator;
            separator = '&';
            written++;

            if (!Uri.TryEscapeDataString(param.Key, destination[written..], out var keyWritten))
            {
                return false;
            }

            written += keyWritten;

            if (param.Value is { } value)
            {
                if (destination.Length - written < 1)
                {
                    return false;
                }

                destination[written] = '=';
                written++;

                if (!Uri.TryEscapeDataString(value, destination[written..], out var valueWritten))
                {
                    return false;
                }

                written += valueWritten;
            }
        }

        return true;
    }

    /// <summary>
    /// A query parameter.
    /// </summary>
    public readonly struct QueryParam
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="QueryParam"/> struct with a key and no value.
        /// </summary>
        /// <param name="key">The query param key.</param>
        public QueryParam(string key)
        {
            Key = key;
            Value = null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryParam"/> struct with a key and a value.
        /// </summary>
        /// <param name="key">The query param key.</param>
        /// <param name="value">The query param value.</param>
        public QueryParam(string key, string? value)
        {
            Key = key;
            Value = value;
        }

        /// <summary>
        /// Gets the key.
        /// </summary>
        public readonly string Key { get; }

        /// <summary>
        /// Gets the value.
        /// </summary>
        public readonly string? Value { get; }
    }
}
