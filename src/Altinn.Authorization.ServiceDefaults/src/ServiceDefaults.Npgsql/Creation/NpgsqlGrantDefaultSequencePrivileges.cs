
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Text;

namespace Altinn.Authorization.ServiceDefaults.Npgsql.Creation;

[DebuggerDisplay("ALTER DEFAULT PRIVILEGES FOR ROLE {_options.RoleName} IN SCHEMA {_options.SchemaName} ON SEQUENCES")]
internal sealed partial class NpgsqlGrantDefaultSequencePrivileges
    : INpgsqlDatabaseCreator
{
    private readonly Settings _options;
    private readonly ILogger<NpgsqlGrantDefaultSequencePrivileges> _logger;

    public NpgsqlGrantDefaultSequencePrivileges(
        Settings options,
        ILogger<NpgsqlGrantDefaultSequencePrivileges> logger)
    {
        _options = options;
        _logger = logger;
    }

    public DatabaseCreationOrder Order => DatabaseCreationOrder.AlterDefaultPrivileges;

    /// <inheritdoc />
    public async Task InitializeDatabaseAsync(INpgsqlConnectionProvider connectionProvider, IServiceProvider scopedServices, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(_options.RoleName))
        {
            Log.MissingRoleName(_logger);
            return;
        }

        if (string.IsNullOrEmpty(_options.CreatorRoleName))
        {
            Log.MissingCreatedByRoleName(_logger);
            return;
        }

        if (string.IsNullOrEmpty(_options.SchemaName))
        {
            Log.MissingSchemaName(_logger);
            return;
        }

        if (!_options.GrantUsage
            && !_options.GrantSelect
            && !_options.GrantUpdate)
        {
            Log.MissingGrantOptions(_logger);
            return;
        }

        var conn = await connectionProvider.GetConnection(cancellationToken);
        await using var cmd = conn.CreateCommand();

        cmd.CommandText = CreateGrantSql();
        Log.CreatingGrant(_logger, _options.RoleName, _options.CreatorRoleName, _options.SchemaName);
        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }

    private string CreateGrantSql()
    {
        var builder = new StringBuilder();
        builder.Append($"ALTER DEFAULT PRIVILEGES FOR ROLE \"");
        builder.Append(_options.CreatorRoleName);
        builder.Append("\" IN SCHEMA \"");
        builder.Append(_options.SchemaName);
        builder.Append("\" GRANT ");

        AppendGrants(builder);

        builder.Append(" ON SEQUENCES TO \"");
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
        if (_options.GrantUsage)
        {
            builder.Append("USAGE, ");
        }

        if (_options.GrantSelect)
        {
            builder.Append("SELECT, ");
        }

        if (_options.GrantUpdate)
        {
            builder.Append("UPDATE, ");
        }

        builder.Length -= 2;
    }

    private static partial class Log
    {
        [LoggerMessage(1, LogLevel.Information, "Creating default sequence grant for {RoleName} on {SchemaName} for sequences created by {CreatorRoleName}.")]
        public static partial void CreatingGrant(ILogger logger, string roleName, string creatorRoleName, string schemaName);

        [LoggerMessage(2, LogLevel.Error, "Role name is missing. Skipping grant.")]
        public static partial void MissingRoleName(ILogger logger);

        [LoggerMessage(3, LogLevel.Error, "Schema name is missing. Skipping grant.")]
        public static partial void MissingSchemaName(ILogger logger);

        [LoggerMessage(4, LogLevel.Error, "No grant options specified. Skipping grant.")]
        public static partial void MissingGrantOptions(ILogger logger);

        [LoggerMessage(5, LogLevel.Error, "Created by role name is missing. Skipping grant.")]
        public static partial void MissingCreatedByRoleName(ILogger logger);
    }

    internal sealed record Settings
    {
        public string? RoleName { get; set; }
        public string? CreatorRoleName { get; set; }
        public string? SchemaName { get; set; }
        public bool GrantUsage { get; set; }
        public bool GrantSelect { get; set; }
        public bool GrantUpdate { get; set; }
        public bool WithGrantOption { get; set; }
    }
}
