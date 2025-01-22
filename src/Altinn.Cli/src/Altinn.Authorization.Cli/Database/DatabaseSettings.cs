using Altinn.Authorization.Cli.Utils;
using Spectre.Console.Cli;
using System.Diagnostics.CodeAnalysis;

namespace Altinn.Authorization.Cli.Database;

[ExcludeFromCodeCoverage]
public class DatabaseSettings
    : CommandSettings
{
    [CommandOption("-c|--connection-string <CONNECTION_STRING>")]
    [EnvironmentVariable("DATABASE_CONNECTION_STRING")]
    public string? ConnectionString { get; init; }
}
