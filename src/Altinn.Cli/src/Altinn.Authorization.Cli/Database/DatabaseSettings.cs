using Altinn.Authorization.Cli.Utils;
using Spectre.Console.Cli;

namespace Altinn.Authorization.Cli.Database;

public class DatabaseSettings
    : CommandSettings
{
    [CommandOption("-c|--connection-string <CONNECTION_STRING>")]
    [EnvironmentVariable("DATABASE_CONNECTION_STRING")]
    public string? ConnectionString { get; init; }
}
