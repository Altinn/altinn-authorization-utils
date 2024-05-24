namespace Altinn.Authorization.ServiceDefaults.Npgsql.Migration;

/// <summary>
/// A service that migrates a PostgreSQL database.
/// </summary>
public interface INpgsqlDatabaseMigrator
{
    /// <summary>
    /// Migrates the database.
    /// </summary>
    /// <param name="connectionProvider">The <see cref="INpgsqlConnectionProvider"/>.</param>
    /// <param name="scopedServices">A scoped <see cref="IServiceProvider"/>.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
    Task MigrateDatabaseAsync(
        INpgsqlConnectionProvider connectionProvider, 
        IServiceProvider scopedServices, 
        CancellationToken cancellationToken);
}
