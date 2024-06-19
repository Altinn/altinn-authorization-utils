using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Text;

namespace Altinn.Authorization.ServiceDefaults.Npgsql.Creation;

[DebuggerDisplay("CREATE SCHEMA {_options.Name}")]
internal sealed partial class NpgsqlCreateSchema
    : INpgsqlDatabaseCreator
{
    private readonly Settings _options;
    private readonly ILogger<NpgsqlCreateSchema> _logger;

    public NpgsqlCreateSchema(
        Settings options,
        ILogger<NpgsqlCreateSchema> logger)
    {
        _options = options;
        _logger = logger;
    }

    public DatabaseCreationOrder Order => DatabaseCreationOrder.CreateSchemas;

    /// <inheritdoc />
    public async Task InitializeDatabaseAsync(INpgsqlConnectionProvider connectionProvider, IServiceProvider scopedServices, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(_options.Name))
        {
            Log.MissingSchemaName(_logger);
            return;
        }

        var conn = await connectionProvider.GetConnection(cancellationToken);
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = CreateSchemaSql();

        Log.CreatingSchema(_logger, _options.Name);

        try
        {
            await cmd.ExecuteNonQueryAsync(cancellationToken);
        }
        catch (PostgresException ex) when (ex.SqlState == "42P06")
        {
            Log.SchemaAlreadyExists(_logger, _options.Name);
            await using var updateCmd = conn.CreateCommand();
            updateCmd.CommandText = UpdateSchemaSql();

            await updateCmd.ExecuteNonQueryAsync(cancellationToken);
        }
    }

    private string CreateSchemaSql()
    {
        var builder = new StringBuilder();
        builder.Append($"CREATE SCHEMA \"{_options.Name}\"");

        if (!string.IsNullOrEmpty(_options.Owner))
        {
            builder.Append($" AUTHORIZATION \"{_options.Owner}\"");
        }
        else
        {
            builder.Append(" AUTHORIZATION CURRENT_USER");
        }

        return builder.ToString();
    }

    private string UpdateSchemaSql()
    {
        var builder = new StringBuilder();
        builder.Append($"ALTER SCHEMA \"{_options.Name}\"");

        if (!string.IsNullOrEmpty(_options.Owner))
        {
            builder.Append($" OWNER TO \"{_options.Owner}\"");
        }
        else
        {
            builder.Append(" OWNER TO CURRENT_USER");
        }

        return builder.ToString();
    }

    private static partial class Log
    {
        [LoggerMessage(1, LogLevel.Information, "Creating schema {SchemaName}.")]
        public static partial void CreatingSchema(ILogger logger, string schemaName);

        [LoggerMessage(2, LogLevel.Error, "Schema name is missing. Skipping schema creation.")]
        public static partial void MissingSchemaName(ILogger logger);

        [LoggerMessage(3, LogLevel.Information, "Schema {SchemaName} already exists. Updating schema instead.")]
        public static partial void SchemaAlreadyExists(ILogger logger, string schemaName);
    }

    internal sealed record Settings(string Name, string? Owner);
}
