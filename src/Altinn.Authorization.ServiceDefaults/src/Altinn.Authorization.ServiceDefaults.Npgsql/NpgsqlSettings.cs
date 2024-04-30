namespace Altinn.Authorization.ServiceDefaults.Npgsql;

/// <summary>
/// Provides the client configuration settings for connecting to a PostgreSQL database using Npgsql.
/// </summary>
public sealed class NpgsqlSettings
{
    /// <summary>
    /// Gets or sets the connection string of the PostgreSQL database to connect to.
    /// </summary>
    public string? ConnectionString { get; set; }

    /// <summary>
    /// Gets or sets a boolean value that indicates whether the database health check is disabled or not.
    /// </summary>
    /// <value>
    /// The default value is <see langword="false"/>.
    /// </value>
    public bool DisableHealthChecks { get; set; }

    /// <summary>
    /// Gets or sets a boolean value that indicates whether the OpenTelemetry tracing is disabled or not.
    /// </summary>
    /// <value>
    /// The default value is <see langword="false"/>.
    /// </value>
    public bool DisableTracing { get; set; }

    /// <summary>
    /// Gets or sets a boolean value that indicates whether the OpenTelemetry metrics are disabled or not.
    /// </summary>
    /// <value>
    /// The default value is <see langword="false"/>.
    /// </value>
    public bool DisableMetrics { get; set; }

    /// <summary>
    /// Gets ot sets the settings for creating the database.
    /// </summary>
    public NpgsqlCreateDatabaseSettings Create { get; set; } = new();

    /// <summary>
    /// Gets or sets the settings for migrating the database.
    /// </summary>
    public NpgsqlMigrateDatabaseSettings Migrate { get; set; } = new();

    /// <summary>
    /// Gets or sets the settings for seeding the database.
    /// </summary>
    public NpgsqlSeedDatabaseSettings Seed { get; set; } = new();
}

/// <summary>
/// Provides the client configuration settings for seeding data to a PostgreSQL database using Npgsql.
/// </summary>
public sealed class NpgsqlSeedDatabaseSettings
{
    /// <summary>
    /// Gets or sets a boolean value that indicates whether the seeding of the database is enabled or not.
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// Gets or sets the connection string of the PostgreSQL database to connect to for seeding data.
    /// </summary>
    public string? ConnectionString { get; set; }
}

/// <summary>
/// Provides the client configuration settings for running database migrations on a PostgreSQL database using Npgsql.
/// </summary>
public sealed class NpgsqlMigrateDatabaseSettings
{
    /// <summary>
    /// Gets or sets a boolean value that indicates whether the migration of the database is enabled or not.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Gets or sets the connection string of the PostgreSQL database to connect to for running migrations.
    /// </summary>
    public string? ConnectionString { get; set; }
}

/// <summary>
/// Provides the client configuration settings for creating a PostgreSQL database using Npgsql.
/// </summary>
public sealed class NpgsqlCreateDatabaseSettings
{
    /// <summary>
    /// Gets or sets a boolean value that indicates whether the creation of the database is enabled or not.
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// Gets or sets the connection string of the PostgreSQL database-server to connect to for creating the database.
    /// </summary>
    public string? ConnectionString { get; set; }

    /// <summary>
    /// Gets or sets the name of the database to create.
    /// </summary>
    public string? DatabaseName { get; set; }

    /// <summary>
    /// Gets or sets the owner of the database to create.
    /// </summary>
    public string? DatabaseOwner { get; set; }

    /// <summary>
    /// Gets or sets the settings for creating roles in the database.
    /// </summary>
    public IDictionary<string, NpgsqlCreateRoleSettings> Roles { get; set; } = new Dictionary<string, NpgsqlCreateRoleSettings>();
}

/// <summary>
/// Provides the client configuration settings for creating a role in a PostgreSQL database using Npgsql.
/// </summary>
public sealed class NpgsqlCreateRoleSettings
{
    /// <summary>
    /// Gets or sets the name of the role to create.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the password of the role to create.
    /// </summary>
    public string? Password { get; set; }
}
