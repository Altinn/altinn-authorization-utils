using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Altinn.Authorization.ServiceDefaults.Npgsql.Creation;

[DebuggerDisplay("GRANT {_options.GrantedRole} TO {_options.RoleName}")]
internal sealed partial class NpgsqlGrantRoleToRole 
    : INpgsqlDatabaseCreator
{
    private readonly Settings _options;
    private readonly ILogger<NpgsqlGrantRoleToRole> _logger;

    public NpgsqlGrantRoleToRole(
        Settings options,
        ILogger<NpgsqlGrantRoleToRole> logger)
    {
        _options = options;
        _logger = logger;
    }

    public DatabaseCreationOrder Order => DatabaseCreationOrder.ConfigureRoles;

    public async Task InitializeDatabaseAsync(INpgsqlConnectionProvider connectionProvider, IServiceProvider services, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(_options.RoleName))
        {
            Log.MissingRoleName(_logger);
            return;
        }

        if (string.IsNullOrEmpty(_options.GrantedRole))
        {
            Log.MissingInheritFromRoleName(_logger);
            return;
        }

        var conn = await connectionProvider.GetConnection(cancellationToken);
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = CreateGrantSql();

        Log.GrantRoleToRole(_logger, _options.GrantedRole, _options.RoleName);

        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }

    private string CreateGrantSql()
    {
        return $"GRANT \"{_options.GrantedRole}\" TO \"{_options.RoleName}\"";
    }

    private static partial class Log
    {
        [LoggerMessage(1, LogLevel.Information, "Granting {InheritFromRoleName} to {RoleName}.")]
        public static partial void GrantRoleToRole(ILogger logger, string inheritFromRoleName, string roleName);

        [LoggerMessage(2, LogLevel.Error, "Role name is missing. Skipping grant.")]
        public static partial void MissingRoleName(ILogger logger);

        [LoggerMessage(3, LogLevel.Error, "Role name to inherit from is missing. Skipping grant.")]
        public static partial void MissingInheritFromRoleName(ILogger logger);
    }

    internal sealed record Settings(string RoleName, string GrantedRole);
}
