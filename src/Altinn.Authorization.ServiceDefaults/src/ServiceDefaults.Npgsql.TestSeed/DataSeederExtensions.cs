﻿using CommunityToolkit.Diagnostics;
using Microsoft.Extensions.Logging;
using System.Collections;

namespace Altinn.Authorization.ServiceDefaults.Npgsql.TestSeed;

/// <summary>
/// Extension methods for test-data seeder types.
/// </summary>
internal static class DataSeederExtensions
{
    /// <summary>
    /// Begins a logger scope for the specified data seeder source.
    /// </summary>
    /// <param name="source">The <see cref="ITestDataSource"/>.</param>
    /// <param name="logger">The <see cref="ILogger"/>.</param>
    /// <returns>A <see cref="IDisposable"/> for ending the scope.</returns>
    public static IDisposable? BeginLoggerScope(this ITestDataSource source, ILogger logger)
        => logger.BeginScope(new DataSeederScope(source));

    private readonly struct DataSeederScope(ITestDataSource source)
        : IReadOnlyList<KeyValuePair<string, object?>>
    {
        public KeyValuePair<string, object?> this[int index]
            => index switch
            {
                0 => new KeyValuePair<string, object?>("SeedDataSource", source.DisplayName),
                _ => ThrowHelper.ThrowArgumentOutOfRangeException<KeyValuePair<string, object?>>(nameof(index), index, "Index is out of range.")
            };

        int IReadOnlyCollection<KeyValuePair<string, object?>>.Count => 1;

        public IEnumerator<KeyValuePair<string, object?>> GetEnumerator()
        {
            yield return this[0];
        }

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();

        public override string ToString()
            => $"SeedDataSource: {source.DisplayName}";
    }
}
