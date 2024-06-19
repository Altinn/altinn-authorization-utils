
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Text;

namespace Altinn.Authorization.ServiceDefaults.Npgsql.Creation;

[DebuggerDisplay("GRANT {_options.RoleName} ON DATABASE {_options.DatabaseName}")]
internal sealed partial class NpgsqlGrantDatabasePrivileges
    : INpgsqlDatabaseCreator
{
    private readonly Settings _options;
    private readonly ILogger<NpgsqlGrantDatabasePrivileges> _logger;

    public NpgsqlGrantDatabasePrivileges(
        Settings options,
        ILogger<NpgsqlGrantDatabasePrivileges> logger)
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

        if (string.IsNullOrEmpty(_options.DatabaseName))
        {
            Log.MissingDatabaseName(_logger);
            return;
        }

        if (!_options.GrantCreate && !_options.GrantConnect && !_options.GrantTemporary)
        {
            Log.MissingGrantOptions(_logger);
            return;
        }

        var conn = await connectionProvider.GetConnection(cancellationToken);
        await using var cmd = conn.CreateCommand();

        cmd.CommandText = CreateGrantSql();
        Log.CreatingGrant(_logger, _options.RoleName, _options.DatabaseName);
        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }

    private string CreateGrantSql()
    {
        var builder = new StringBuilder();
        builder.Append($"GRANT ");

        AppendGrants(builder);

        builder.Append(" ON DATABASE \"");
        builder.Append(_options.DatabaseName);
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

        if (_options.GrantConnect)
        {
            builder.Append("CONNECT, ");
        }

        if (_options.GrantTemporary)
        {
            builder.Append("TEMPORARY, ");
        }

        builder.Length -= 2;
    }

    private static partial class Log
    {
        [LoggerMessage(1, LogLevel.Information, "Creating database grant for {RoleName} on {DatabaseName}.")]
        public static partial void CreatingGrant(ILogger logger, string roleName, string databaseName);

        [LoggerMessage(2, LogLevel.Error, "Role name is missing. Skipping grant.")]
        public static partial void MissingRoleName(ILogger logger);

        [LoggerMessage(3, LogLevel.Error, "Database name is missing. Skipping grant.")]
        public static partial void MissingDatabaseName(ILogger logger);

        [LoggerMessage(4, LogLevel.Error, "No grant options specified. Skipping grant.")]
        public static partial void MissingGrantOptions(ILogger logger);
    }

    internal sealed record Settings
    {
        public required string RoleName { get; init; }
        public required string DatabaseName { get; init; }
        public required bool WithGrantOption { get; init; }
        public required bool GrantCreate { get; init; }
        public required bool GrantConnect { get; init; }
        public required bool GrantTemporary { get; init; }
    }
}
