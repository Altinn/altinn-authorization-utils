using System.Buffers;
using System.Text;
using CommunityToolkit.Diagnostics;

namespace Altinn.Authorization.ProblemDetails.PathUtils;

internal static class JsonPointerExtensions
{
    private static readonly SearchValues<char> _jsonPointerEscapeChars
        = SearchValues.Create(['~', '/']);

    private static readonly SearchValues<char> _systemTextJsonPathSeparatorChars
        = SearchValues.Create(['.', '[']);

    /// <summary>
    /// Creates a JSON pointer from a System.Text.Json path.
    /// </summary>
    /// <param name="jsonPath">The JSON path to convert.</param>
    /// <returns>The corresponding JSON pointer.</returns>
    internal static string CreateFromSystemTextJsonPath(ReadOnlySpan<char> jsonPath)
    {
        if (jsonPath.Length == 0)
        {
            ThrowHelper.ThrowArgumentException(nameof(jsonPath), "JSON path cannot be empty.");
        }

        if (jsonPath[0] != '$')
        {
            ThrowHelper.ThrowArgumentException(nameof(jsonPath), "JSON path must start with '$'.");
        }

        jsonPath = jsonPath[1..]; // Skip the leading '$'
        if (jsonPath.IsEmpty)
        {
            return "/";
        }

        var builder = new StringBuilder(jsonPath.Length);
        ReadJsonPath(jsonPath, builder);

        return builder.ToString();

        static void ReadJsonPath(ReadOnlySpan<char> path, StringBuilder builder)
        {
            while (path.Length > 0)
            {
                if (path[0] == '.')
                {
                    path = path[1..];

                    var endIndex = path.IndexOfAny(_systemTextJsonPathSeparatorChars);
                    if (endIndex < 0)
                    {
                        AppendSegment(builder, path);
                        return;
                    }

                    var segment = path[..endIndex];
                    path = path[endIndex..];

                    AppendSegment(builder, segment);
                }
                else if (path[0] == '[')
                {
                    if (path.Length > 1 && path[1] == '\'')
                    {
                        // Handle property names in brackets, e.g., ['property']
                        var endIndex = path.IndexOf("']");
                        if (endIndex < 0)
                        {
                            // This path is really invalid, so we just treat it as a big literal
                            AppendSegment(builder, path);
                            return;
                        }

                        var propertyName = path[2..endIndex];
                        var remainder = path[(endIndex + 2)..]; // Skip the closing "']"

#if NET11_0_OR_GREATER
                        if (propertyName.Contains('\\'))
                        {
                            // Property name contains escaped characters, so we need to unescape
                            propertyName = UnescapeJsonPath(propertyName);
                        }
#endif

                        AppendSegment(builder, propertyName);
                        path = remainder;
                    }
                    else
                    {
                        // Handle array indices in brackets, e.g., [0]
                        var endIndex = path.IndexOf(']');
                        if (endIndex < 0)
                        {
                            // This path is really invalid, so we just treat it as a big literal
                            AppendSegment(builder, path);
                            return;
                        }

                        var arrayIndex = path[1..endIndex];
                        var remainder = path[(endIndex + 1)..]; // Skip the closing ']'

                        AppendSegment(builder, arrayIndex);
                        path = remainder;
                    }
                }
                else
                {
                    var endIndex = path.IndexOfAny(_systemTextJsonPathSeparatorChars);
                    if (endIndex < 0)
                    {
                        // No more separators, so this is the last segment
                        AppendSegment(builder, path);
                        return;
                    }

                    var segment = path[..endIndex];
                    path = path[endIndex..];
                    AppendSegment(builder, segment);
                }
            }
        }

        static void AppendSegment(StringBuilder builder, ReadOnlySpan<char> segment)
        {
            builder.Append('/');
            builder.AppendEscapeJsonPointer(segment);
        }

#if NET11_0_OR_GREATER
        static string UnescapeJsonPath(ReadOnlySpan<char> value)
        {
            var builder = new StringBuilder(value.Length);

            while (value.Length > 0)
            {
                var escapedIndex = value.IndexOf('\\');
                if (escapedIndex < 0)
                {
                    builder.Append(value);
                    return builder.ToString();
                }

                builder.Append(value[..escapedIndex]);
                if (escapedIndex + 1 < value.Length)
                {
                    builder.Append(value[escapedIndex + 1]);
                    value = value[(escapedIndex + 2)..];
                }
                else
                {
                    // Trailing backslash, treat it as a literal
                    builder.Append('\\');
                    return builder.ToString();
                }
            }

            return builder.ToString();
        }
#endif
    }

    extension(StringBuilder builder)
    {
        internal StringBuilder AppendEscapeJsonPointer(ReadOnlySpan<char> value)
        {
            while (value.Length > 0)
            {
                var index = value.IndexOfAny(_jsonPointerEscapeChars);
                if (index < 0)
                {
                    builder.Append(value);
                    break;
                }

                builder.Append(value[..index]);
                switch (value[index])
                {
                    case '~':
                        builder.Append("~0");
                        break;
                    case '/':
                        builder.Append("~1");
                        break;
                }

                value = value[(index + 1)..];
            }

            return builder;
        }
    }
}
