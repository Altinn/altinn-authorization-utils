namespace Altinn.Authorization.ServiceDefaults.Npgsql;

/// <summary>
/// A connection provider for Npgsql connections, used during database initialization.
/// </summary>
public interface INpgsqlConnectionProvider
{
    /// <summary>
    /// Gets the connection string of the PostgreSQL database to connect to.
    /// </summary>
    string ConnectionString { get; }

    /// <summary>
    /// Gets a (shared) connection to the PostgreSQL database.
    /// </summary>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
    /// <returns>A <see cref="NpgsqlConnection"/>.</returns>
    Task<NpgsqlConnection> GetConnection(CancellationToken cancellationToken);
}
