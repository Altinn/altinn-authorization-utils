namespace Altinn.Authorization.ServiceDefaults.Npgsql.Migration;

internal class InlineDatabaseMigration
    : INpgsqlDatabaseMigrator
{
    private readonly Func<NpgsqlConnection, IServiceProvider, CancellationToken, Task> _migration;

    public InlineDatabaseMigration(
        Func<NpgsqlConnection, IServiceProvider, CancellationToken, Task> migration)
    {
        _migration = migration;
    }

    public async Task MigrateDatabaseAsync(INpgsqlConnectionProvider connectionProvider, IServiceProvider scopedServices, CancellationToken cancellationToken)
    {
        var connection = await connectionProvider.GetConnection(cancellationToken);
        await _migration(connection, scopedServices, cancellationToken);
    }
}
