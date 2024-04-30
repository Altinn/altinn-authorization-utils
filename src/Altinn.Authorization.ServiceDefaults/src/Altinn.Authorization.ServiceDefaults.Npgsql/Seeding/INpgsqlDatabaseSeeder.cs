namespace Altinn.Authorization.ServiceDefaults.Npgsql.Seeding;

/// <summary>
/// A service that seeds a PostgreSQL database.
/// </summary>
public interface INpgsqlDatabaseSeeder
{
    /// <summary>
    /// Seeds the database.
    /// </summary>
    /// <param name="connectionProvider">The <see cref="INpgsqlConnectionProvider"/>.</param>
    /// <param name="scopedServices">A scoped <see cref="IServiceProvider"/>.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
    Task SeedDatabaseAsync(
        INpgsqlConnectionProvider connectionProvider,
        IServiceProvider scopedServices,
        CancellationToken cancellationToken);
}
