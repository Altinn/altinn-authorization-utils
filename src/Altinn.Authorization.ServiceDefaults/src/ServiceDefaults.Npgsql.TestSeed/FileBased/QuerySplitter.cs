﻿using CommunityToolkit.Diagnostics;
using System.Buffers;
using System.Text;

namespace Altinn.Authorization.ServiceDefaults.Npgsql.TestSeed.FileBased;

/// <summary>
/// Splits a sql scripts into individual queries.
/// </summary>
/// <remarks>
/// This is an incredibly naive implementation that only works for simple queries.
/// We only care about semicolons that are at the end of a line (after removing
/// leading and trailing whitespace and comments).
/// </remarks>
internal readonly ref struct QuerySplitter
{
    private readonly ReadOnlySpan<char> _query;

    /// <summary>
    /// Constructs a new instance of <see cref="QuerySplitter"/>.
    /// </summary>
    /// <param name="query">The query to split.</param>
    public QuerySplitter(ReadOnlySpan<char> query)
    {
        _query = query;
    }

    /// <inheritdoc cref="IEnumerable{T}.GetEnumerator()"/>
    public Enumerator GetEnumerator() => new(_query);

    /// <summary>
    /// Enumerator for <see cref="QuerySplitter"/>.
    /// </summary>
    public ref struct Enumerator
    {
        private static ThreadLocal<StringBuilder> _cachedBuilder = new(() => new(256));

        private static readonly SearchValues<char> LineTerminators = SearchValues.Create(['\n', '\r']);
        private readonly StringBuilder _current;

        private ReadOnlySpan<char> _query;

        /// <summary>
        /// Constructs a new instance of <see cref="Enumerator"/>.
        /// </summary>
        /// <param name="query">The query to split.</param>
        internal Enumerator(ReadOnlySpan<char> query)
        {
            _query = query.Trim();
            _current = _cachedBuilder.Value!;
        }

        /// <inheritdoc cref="IEnumerator{T}.Current"/>
        public readonly string Current => _current.ToString();

        /// <inheritdoc cref="System.Collections.IEnumerator.MoveNext()"/>
        public bool MoveNext()
        {
            // First, clear the string builder that contains the previous query.
            _current.Clear();

            // Loop through the query, line by line
            foreach (var (lineRaw, rest) in _query.Split(LineTerminators, StringSplitOptions.RemoveEmptyEntries))
            {
                var line = RemoveLineComments(lineRaw.TrimEnd(), out var hasTrailingComment);

                // Skip empty lines.
                if (line.IsEmpty)
                {
                    continue;
                }

                // Append the line to the current query.
                _current.Append(line);

                // If the line ends with a semicolon, we have a complete query.
                if (line[^1] == ';' && !hasTrailingComment)
                {
                    // Remove the semicolon from the current query.
                    _current.Length = _current.Length - 1;
                    _query = rest;
                    return true;
                }

                _current.Append('\n');
            }

            if (_current.Length > 0)
            {
                ThrowHelper.ThrowInvalidOperationException("Query does not end with ';'.");
            }

            return false;
        }

        private static ReadOnlySpan<char> RemoveLineComments(ReadOnlySpan<char> line, out bool hasTrailingComment)
        {
            var commentStart = line.IndexOf(['-', '-']);
            if (commentStart == -1)
            {
                hasTrailingComment = false;
                return line;
            }

            hasTrailingComment = true;
            return line[..commentStart].TrimEnd();
        }
    }
}
