namespace Altinn.Authorization.ServiceDefaults.Npgsql.Migration;

/// <summary>
/// Common options for PostgreSQL database migrations.
/// </summary>
public class NpgsqlDatabaseMigrationOptions
{
    /// <summary>
    /// The role that is running the migration.
    /// </summary>
    public string? MigratorRole { get; set; }

    /// <summary>
    /// The role that the application should have.
    /// </summary>
    public string? AppRole { get; set; }
}
