﻿namespace Altinn.Authorization.ServiceDefaults.Npgsql.Creation;

/// <summary>
/// A service that creates/initializes a PostgreSQL database.
/// </summary>
public interface INpgsqlDatabaseCreator
{
    /// <summary>
    /// The order in which the database creator should be executed.
    /// </summary>
    DatabaseCreationOrder Order => DatabaseCreationOrder.CreateDatabases;

    /// <summary>
    /// Creates/initializes the database.
    /// </summary>
    /// <param name="connectionProvider">The <see cref="INpgsqlConnectionProvider"/>.</param>
    /// <param name="scopedServices">A scoped <see cref="IServiceProvider"/>.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
    Task InitializeDatabaseAsync(
        INpgsqlConnectionProvider connectionProvider,
        IServiceProvider scopedServices,
        CancellationToken cancellationToken);
}
