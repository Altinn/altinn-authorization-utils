
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Text;

namespace Altinn.Authorization.ServiceDefaults.Npgsql.Creation;

[DebuggerDisplay("GRANT {_options.RoleName} ON SCHEMA {_options.SchemaName}")]
internal sealed partial class NpgsqlGrantSchemaPrivileges
    : INpgsqlDatabaseCreator
{
    private readonly Settings _options;
    private readonly ILogger<NpgsqlGrantSchemaPrivileges> _logger;

    public NpgsqlGrantSchemaPrivileges(
        Settings options,
        ILogger<NpgsqlGrantSchemaPrivileges> logger)
    {
        _options = options;
        _logger = logger;
    }

    public DatabaseCreationOrder Order => DatabaseCreationOrder.CreateGrants;

    /// <inheritdoc />
    public async Task InitializeDatabaseAsync(INpgsqlConnectionProvider connectionProvider, IServiceProvider scopedServices, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(_options.RoleName))
        {
            Log.MissingRoleName(_logger);
            return;
        }

        if (string.IsNullOrEmpty(_options.SchemaName))
        {
            Log.MissingSchemaName(_logger);
            return;
        }

        if (!_options.GrantCreate && !_options.GrantUsage)
        {
            Log.MissingGrantOptions(_logger);
            return;
        }

        var conn = await connectionProvider.GetConnection(cancellationToken);
        await using var cmd = conn.CreateCommand();

        cmd.CommandText = CreateGrantSql();
        Log.CreatingGrant(_logger, _options.RoleName, _options.SchemaName);
        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }

    private string CreateGrantSql()
    {
        var builder = new StringBuilder();
        builder.Append($"GRANT ");

        AppendGrants(builder);

        builder.Append(" ON SCHEMA \"");
        builder.Append(_options.SchemaName);
        builder.Append("\" TO \"");
        builder.Append(_options.RoleName);
        builder.Append('\"');

        if (_options.WithGrantOption)
        {
            builder.Append(" WITH GRANT OPTION");
        }

        return builder.Append(';').ToString();
    }

    private void AppendGrants(StringBuilder builder)
    {
        if (_options.GrantCreate)
        {
            builder.Append("CREATE, ");
        }

        if (_options.GrantUsage)
        {
            builder.Append("USAGE, ");
        }

        builder.Length -= 2;
    }

    private static partial class Log
    {
        [LoggerMessage(1, LogLevel.Information, "Creating schema grant for {RoleName} on {SchemaName}.")]
        public static partial void CreatingGrant(ILogger logger, string roleName, string schemaName);

        [LoggerMessage(2, LogLevel.Error, "Role name is missing. Skipping grant.")]
        public static partial void MissingRoleName(ILogger logger);

        [LoggerMessage(3, LogLevel.Error, "Schema name is missing. Skipping grant.")]
        public static partial void MissingSchemaName(ILogger logger);

        [LoggerMessage(4, LogLevel.Error, "No grant options specified. Skipping grant.")]
        public static partial void MissingGrantOptions(ILogger logger);
    }

    internal sealed record Settings
    {
        public string? RoleName { get; set; }
        public string? SchemaName { get; set; }
        public bool GrantCreate { get; set; }
        public bool GrantUsage { get; set; }
        public bool WithGrantOption { get; set; }
    }
}
