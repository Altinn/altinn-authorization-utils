using Altinn.Authorization.Cli.Utils;
using Spectre.Console.Cli;
using System.Diagnostics.CodeAnalysis;

namespace Altinn.Authorization.Cli.Database;

/// <summary>
/// Shared settings for all database commands.
/// </summary>
[ExcludeFromCodeCoverage]
public class DatabaseSettings
    : CommandSettings
{
    /// <summary>
    /// Gets the connection string to the database.
    /// </summary>
    [CommandOption("-c|--connection-string <CONNECTION_STRING>")]
    [EnvironmentVariable("DATABASE_CONNECTION_STRING")]
    public string? ConnectionString { get; init; }
}
