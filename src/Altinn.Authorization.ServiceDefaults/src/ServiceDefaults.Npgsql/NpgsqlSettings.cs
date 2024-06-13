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
    public string? ClusterConnectionString { get; set; }

    /// <summary>
    /// Gets or sets the connection string of the PostgreSQL database to connect to for initializing the database post creation.
    /// </summary>
    public string? DatabaseConnectionString { get; set; }

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

    /// <summary>
    /// Gets or sets the settings for creating schemas in the database.
    /// </summary>
    public IDictionary<string, NpgsqlCreateSchemaSettings> Schemas { get; set; } = new Dictionary<string, NpgsqlCreateSchemaSettings>();
}

/// <summary>
/// Provides the client configuration settings for creating a schema in a PostgreSQL database using Npgsql.
/// </summary>
public sealed class NpgsqlCreateSchemaSettings
{
    /// <summary>
    /// Gets or sets the name of the schema to create.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the owner of the schema to create.
    /// </summary>
    public string? Owner { get; set; }
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

    /// <summary>
    /// Gets or sets the settings for granting permissions to the role.
    /// </summary>
    public NpgsqlRoleGrantSettings Grants { get; set; } = new();
}

/// <summary>
/// Provides the client configuration settings for granting permissions in a PostgreSQL database using Npgsql.
/// </summary>
public sealed class NpgsqlRoleGrantSettings
{
    /// <summary>
    /// Gets or sets the kind of privileges to grant on the database.
    /// </summary>
    public NpgsqlDatabaseGrantSettings Database { get; set; } = new();

    /// <summary>
    /// Gets or sets the schemas to grant permissions to.
    /// </summary>
    public IDictionary<string, NpgsqlSchemaGrantSettings> Schemas { get; set; } = new Dictionary<string, NpgsqlSchemaGrantSettings>();

    /// <summary>
    /// Gets or sets roles that the role is granted.
    /// </summary>
    public IDictionary<string, NpgsqlRoleGrantRoleSettings> Roles { get; set; } = new Dictionary<string, NpgsqlRoleGrantRoleSettings>();
}

/// <summary>
/// Provides the client configuration settings for granting permissions from a role to another in a PostgreSQL database using Npgsql.
/// </summary>
public sealed class NpgsqlRoleGrantRoleSettings
{
    /// <summary>
    /// Gets or sets a boolean value that indicates whether the role is granted role usage.
    /// </summary>
    public bool Usage { get; set; }

    /// <summary>
    /// Gets or sets schemas that the role is granted default permissions on.
    /// </summary>
    public IDictionary<string, NpgsqlRoleGrantSchemaSettings> Schemas { get; set; } = new Dictionary<string, NpgsqlRoleGrantSchemaSettings>();
}

/// <summary>
/// Provides the client configuration settings for granting default privileges in a PostgreSQL schema using Npgsql.
/// </summary>
public sealed class NpgsqlRoleGrantSchemaSettings
{
    /// <summary>
    /// Gets or sets the kind of privileges to grant on the tables in the schema.
    /// </summary>
    public NpgsqlTableGrantSettings Tables { get; set; } = new();

    /// <summary>
    /// Gets or sets the kind of privileges to grant on the sequences in the schema.
    /// </summary>
    public NpgsqlSequenceGrantSettings Sequences { get; set; } = new();
}

/// <summary>
/// Provides the client configuration settings for granting permissions in a PostgreSQL database using Npgsql.
/// </summary>
public sealed class NpgsqlDatabaseGrantSettings
{
    /// <summary>
    /// Gets or sets the kind of privileges to grant on the database.
    /// </summary>
    public NpgsqlDatabasePrivileges Privileges { get; set; }

    /// <summary>
    /// Gets or sets a boolean value that indicates whether the privileges should be granted with the grant option.
    /// </summary>
    public bool WithGrantOption { get; set; }
}

/// <summary>
/// Provides the client configuration settings for granting permissions in a PostgreSQL schema using Npgsql.
/// </summary>
public sealed class NpgsqlSchemaGrantSettings
{
    /// <summary>
    /// Gets or sets the kind of privileges to grant on the schema.
    /// </summary>
    public NpgsqlSchemaPrivileges Privileges { get; set; }

    /// <summary>
    /// Gets or sets a boolean value that indicates whether the privileges should be granted with the grant option.
    /// </summary>
    public bool WithGrantOption { get; set; }
}

/// <summary>
/// Provides the client configuration settings for granting permissions in a PostgreSQL table using Npgsql.
/// </summary>
public sealed class NpgsqlTableGrantSettings
{
    /// <summary>
    /// Gets or sets the kind of privileges to grant on the table.
    /// </summary>
    public NpgsqlTablePrivileges Privileges { get; set; }

    /// <summary>
    /// Gets or sets a boolean value that indicates whether the privileges should be granted with the grant option.
    /// </summary>
    public bool WithGrantOption { get; set; }
}

/// <summary>
/// Provides the client configuration settings for granting permissions in a PostgreSQL sequence using Npgsql.
/// </summary>
public sealed class NpgsqlSequenceGrantSettings
{
    /// <summary>
    /// Gets or sets the kind of privileges to grant on the sequence.
    /// </summary>
    public NpgsqlSequencePrivileges Privileges { get; set; }

    /// <summary>
    /// Gets or sets a boolean value that indicates whether the privileges should be granted with the grant option.
    /// </summary>
    public bool WithGrantOption { get; set; }
}

/// <summary>
/// PgSQL database privileges.
/// </summary>
[Flags]
public enum NpgsqlDatabasePrivileges : byte
{
    /// <summary>
    /// No privileges.
    /// </summary>
    None = 0,

    /// <summary>
    /// <c>CREATE</c> privileges.
    /// </summary>
    Create = 1 << 0,
    /// <summary>
    /// <c>CONNECT</c> privileges.
    /// </summary>
    Connect = 1 << 1,
    /// <summary>
    /// <c>TEMPORARY</c> privileges.
    /// </summary>
    Temporary = 1 << 2,

    /// <summary>
    /// All database privileges.
    /// </summary>
    All = Create | Connect | Temporary,
}

/// <summary>
/// PgSQL schema privileges.
/// </summary>
[Flags]
public enum NpgsqlSchemaPrivileges : byte
{
    /// <summary>
    /// No privileges.
    /// </summary>
    None = 0,

    /// <summary>
    /// <c>CREATE</c> privileges.
    /// </summary>
    Create = 1 << 0,
    /// <summary>
    /// <c>USAGE</c> privileges.
    /// </summary>
    Usage = 1 << 1,

    /// <summary>
    /// All schema privileges.
    /// </summary>
    All = Create | Usage,
}

/// <summary>
/// PgSQL table privileges.
/// </summary>
[Flags]
public enum NpgsqlTablePrivileges : byte
{
    /// <summary>
    /// No privileges.
    /// </summary>
    None = 0,

    /// <summary>
    /// <c>SELECT</c> privileges.
    /// </summary>
    Select = 1 << 0,
    /// <summary>
    /// <c>INSERT</c> privileges.
    /// </summary>
    Insert = 1 << 1,
    /// <summary>
    /// <c>UPDATE</c> privileges.
    /// </summary>
    Update = 1 << 2,
    /// <summary>
    /// <c>DELETE</c> privileges.
    /// </summary>
    Delete = 1 << 3,
    /// <summary>
    /// <c>TRUNCATE</c> privileges.
    /// </summary>
    Truncate = 1 << 4,
    /// <summary>
    /// <c>REFERENCES</c> privileges.
    /// </summary>
    References = 1 << 5,
    /// <summary>
    /// <c>TRIGGER</c> privileges.
    /// </summary>
    Trigger = 1 << 6,

    /// <summary>
    /// <c>SELECT</c>, <c>INSERT</c>, <c>UPDATE</c>, and <c>DELETE</c> privileges.
    /// </summary>
    Usage = Select | Insert | Update | Delete,

    /// <summary>
    /// All table privileges.
    /// </summary>
    All = Select | Insert | Update | Delete | Truncate | References | Trigger,
}

/// <summary>
/// PgSQL sequence privileges.
/// </summary>
[Flags]
public enum NpgsqlSequencePrivileges : byte
{
    /// <summary>
    /// No privileges.
    /// </summary>
    None = 0,

    /// <summary>
    /// <c>USAGE</c> privileges.
    /// </summary>
    Usage = 1 << 0,
    /// <summary>
    /// <c>SELECT</c> privileges.
    /// </summary>
    Select = 1 << 1,
    /// <summary>
    /// <c>UPDATE</c> privileges.
    /// </summary>
    Update = 1 << 2,

    /// <summary>
    /// All sequence privileges.
    /// </summary>
    All = Usage | Select | Update,
}
