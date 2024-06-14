using Altinn.Authorization.ServiceDefaults.Npgsql.Creation;
using CommunityToolkit.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

namespace Altinn.Authorization.ServiceDefaults.Npgsql;

/// <summary>
/// Extension methods for <see cref="INpgsqlDatabaseBuilder"/>.
/// </summary>
public static class NpgsqlDatabaseCreatorServiceCollectionExtensions
{
    private static readonly ObjectFactory<NpgsqlCreateDatabase> _dbFactory = ActivatorUtilities.CreateFactory<NpgsqlCreateDatabase>([typeof(NpgsqlCreateDatabase.Settings)]);
    private static readonly ObjectFactory<NpgsqlCreateRole> _roleFactory = ActivatorUtilities.CreateFactory<NpgsqlCreateRole>([typeof(NpgsqlCreateRole.Settings)]);
    private static readonly ObjectFactory<NpgsqlCreateSchema> _schemaFactory = ActivatorUtilities.CreateFactory<NpgsqlCreateSchema>([typeof(NpgsqlCreateSchema.Settings)]);
    private static readonly ObjectFactory<NpgsqlGrantRoleToRole> _grantRoleToRoleFactory = ActivatorUtilities.CreateFactory<NpgsqlGrantRoleToRole>([typeof(NpgsqlGrantRoleToRole.Settings)]);
    private static readonly ObjectFactory<NpgsqlGrantDatabasePrivileges> _grantDatabasePrivilegesFactory = ActivatorUtilities.CreateFactory<NpgsqlGrantDatabasePrivileges>([typeof(NpgsqlGrantDatabasePrivileges.Settings)]);
    private static readonly ObjectFactory<NpgsqlGrantSchemaPrivileges> _grantSchemaPrivilegesFactory = ActivatorUtilities.CreateFactory<NpgsqlGrantSchemaPrivileges>([typeof(NpgsqlGrantSchemaPrivileges.Settings)]);
    private static readonly ObjectFactory<NpgsqlGrantDefaultTablePrivileges> _grantDefaultTablePrivilegesFactory = ActivatorUtilities.CreateFactory<NpgsqlGrantDefaultTablePrivileges>([typeof(NpgsqlGrantDefaultTablePrivileges.Settings)]);
    private static readonly ObjectFactory<NpgsqlGrantDefaultSequencePrivileges> _grantDefaultSequencePrivilegesFactory = ActivatorUtilities.CreateFactory<NpgsqlGrantDefaultSequencePrivileges>([typeof(NpgsqlGrantDefaultSequencePrivileges.Settings)]);

    /// <summary>
    /// Configures the initialization service to create a database if database creation is enabled.
    /// </summary>
    /// <param name="builder">The <see cref="INpgsqlDatabaseBuilder"/>.</param>
    /// <param name="configure">A configure delegate.</param>
    /// <returns><paramref name="builder"/>.</returns>
    public static INpgsqlDatabaseBuilder CreateDatabase(this INpgsqlDatabaseBuilder builder, Action<IServiceProvider, CreateDatabaseSettings> configure)
    {
        builder.Services.AddSingleton<INpgsqlDatabaseCreator>((services) =>
        {
            CreateDatabaseSettings settings = new();
            configure(services, settings);

            return _dbFactory(services, [settings.IntoSettings()]);
        });

        return builder;
    }

    /// <summary>
    /// Configures the initialization service to create a database if database creation is enabled.
    /// </summary>
    /// <param name="builder">The <see cref="INpgsqlDatabaseBuilder"/>.</param>
    /// <param name="configure">A configure delegate.</param>
    /// <returns><paramref name="builder"/>.</returns>
    public static INpgsqlDatabaseBuilder CreateDatabase(this INpgsqlDatabaseBuilder builder, Action<CreateDatabaseSettings> configure)
    {
        return builder.CreateDatabase((_, options) => configure(options));
    }

    /// <summary>
    /// Configures the initialization service to create a database if database creation is enabled.
    /// </summary>
    /// <param name="builder">The <see cref="INpgsqlDatabaseBuilder"/>.</param>
    /// <param name="databaseName">The database name.</param>
    /// <param name="databaseOwner">The database owner (optional).</param>
    /// <returns><paramref name="builder"/>.</returns>
    public static INpgsqlDatabaseBuilder CreateDatabase(this INpgsqlDatabaseBuilder builder, string databaseName, string? databaseOwner)
    {
        Guard.IsNotNullOrEmpty(databaseName);

        return builder.CreateDatabase(options =>
        {
            options.DatabaseName = databaseName;
            options.DatabaseOwner = databaseOwner;
        });
    }

    /// <summary>
    /// Configures the initialization service to create a role if database creation is enabled.
    /// </summary>
    /// <param name="builder">The <see cref="INpgsqlDatabaseBuilder"/>.</param>
    /// <param name="configure">A configure delegate.</param>
    /// <returns><paramref name="builder"/>.</returns>
    public static INpgsqlDatabaseBuilder CreateRole(this INpgsqlDatabaseBuilder builder, Action<IServiceProvider, CreateRoleSettings> configure)
    {
        builder.Services.AddSingleton<INpgsqlDatabaseCreator>((services) =>
        {
            CreateRoleSettings settings = new();
            configure(services, settings);

            return _roleFactory(services, [settings.IntoSettings()]);
        });

        return builder;
    }

    /// <summary>
    /// Configures the initialization service to create a role if database creation is enabled.
    /// </summary>
    /// <param name="builder">The <see cref="INpgsqlDatabaseBuilder"/>.</param>
    /// <param name="configure">A configure delegate.</param>
    /// <returns><paramref name="builder"/>.</returns>
    public static INpgsqlDatabaseBuilder CreateRole(this INpgsqlDatabaseBuilder builder, Action<CreateRoleSettings> configure)
    {
        return builder.CreateRole((_, options) => configure(options));
    }

    /// <summary>
    /// Configures the initialization service to create a role if database creation is enabled.
    /// </summary>
    /// <param name="builder">The <see cref="INpgsqlDatabaseBuilder"/>.</param>
    /// <param name="name">The role name.</param>
    /// <param name="password">The role password (optional).</param>
    /// <returns><paramref name="builder"/>.</returns>
    public static INpgsqlDatabaseBuilder CreateRole(this INpgsqlDatabaseBuilder builder, string name, string? password)
    {
        Guard.IsNotNull(builder);
        Guard.IsNotNullOrEmpty(name);

        return builder.CreateRole(options =>
        {
            options.Name = name;
            options.Password = password;
        });
    }

    /// <summary>
    /// Configures the initialization service to create a schema if database creation is enabled.
    /// </summary>
    /// <param name="builder">The <see cref="INpgsqlDatabaseBuilder"/>.</param>
    /// <param name="configure">A configure delegate.</param>
    /// <returns><paramref name="builder"/>.</returns>
    public static INpgsqlDatabaseBuilder CreateSchema(this INpgsqlDatabaseBuilder builder, Action<IServiceProvider, CreateSchemaSettings> configure)
    {
        builder.Services.AddSingleton<INpgsqlDatabaseCreator>((services) =>
        {
            CreateSchemaSettings settings = new();
            configure(services, settings);

            return _schemaFactory(services, [settings.IntoSettings()]);
        });

        return builder;
    }

    /// <summary>
    /// Configures the initialization service to create a schema if database creation is enabled.
    /// </summary>
    /// <param name="builder">The <see cref="INpgsqlDatabaseBuilder"/>.</param>
    /// <param name="configure">A configure delegate.</param>
    /// <returns><paramref name="builder"/>.</returns>
    public static INpgsqlDatabaseBuilder CreateSchema(this INpgsqlDatabaseBuilder builder, Action<CreateSchemaSettings> configure)
    {
        return builder.CreateSchema((_, options) => configure(options));
    }

    /// <summary>
    /// Configures the initialization service to create a schema if database creation is enabled.
    /// </summary>
    /// <param name="builder">The <see cref="INpgsqlDatabaseBuilder"/>.</param>
    /// <param name="name">The schema name.</param>
    /// <param name="owner">The schema owner (optional).</param>
    /// <returns><paramref name="builder"/>.</returns>
    public static INpgsqlDatabaseBuilder CreateSchema(this INpgsqlDatabaseBuilder builder, string name, string? owner)
    {
        Guard.IsNotNull(builder);
        Guard.IsNotNullOrEmpty(name);

        return builder.CreateSchema(options =>
        {
            options.Name = name;
            options.Owner = owner;
        });
    }

    /// <summary>
    /// Configures the initialization service to grant a role to another role.
    /// </summary>
    /// <param name="builder">The <see cref="INpgsqlDatabaseBuilder"/>.</param>
    /// <param name="configure">A configure delegate.</param>
    /// <returns><paramref name="builder"/>.</returns>
    public static INpgsqlDatabaseBuilder GrantRoleToRole(this INpgsqlDatabaseBuilder builder, Action<IServiceProvider, GrantRoleToRoleSettings> configure)
    {
        Guard.IsNotNull(builder);
        Guard.IsNotNull(configure);

        builder.Services.AddSingleton<INpgsqlDatabaseCreator>((services) =>
        {
            GrantRoleToRoleSettings settings = new();
            configure(services, settings);

            return _grantRoleToRoleFactory(services, [settings.IntoSettings()]);
        });

        return builder;
    }

    /// <summary>
    /// Configures the initialization service to grant a role to another role.
    /// </summary>
    /// <param name="builder">The <see cref="INpgsqlDatabaseBuilder"/>.</param>
    /// <param name="configure">A configure delegate.</param>
    /// <returns><paramref name="builder"/>.</returns>
    public static INpgsqlDatabaseBuilder GrantRoleToRole(this INpgsqlDatabaseBuilder builder, Action<GrantRoleToRoleSettings> configure)
    {
        Guard.IsNotNull(builder);
        Guard.IsNotNull(configure);

        return builder.GrantRoleToRole((_, options) => configure(options));
    }

    /// <summary>
    /// Configures the initialization service to grant a role to another role.
    /// </summary>
    /// <param name="builder">The <see cref="INpgsqlDatabaseBuilder"/>.</param>
    /// <param name="roleName">The role to grant on.</param>
    /// <param name="grantedRole">The role to grant.</param>
    /// <returns><paramref name="builder"/>.</returns>
    public static INpgsqlDatabaseBuilder GrantRoleToRole(this INpgsqlDatabaseBuilder builder, string roleName, string grantedRole)
    {
        Guard.IsNotNull(builder);
        Guard.IsNotNullOrEmpty(roleName);
        Guard.IsNotNullOrEmpty(grantedRole);

        return builder.GrantRoleToRole(options =>
        {
            options.RoleName = roleName;
            options.GrantedRole = grantedRole;
        });
    }

    /// <summary>
    /// Configures the initialization service to grant privileges to a role on a database.
    /// </summary>
    /// <param name="builder">The <see cref="INpgsqlDatabaseBuilder"/>.</param>
    /// <param name="configure">A configure delegate.</param>
    /// <returns><paramref name="builder"/>.</returns>
    public static INpgsqlDatabaseBuilder GrantDatabasePrivileges(this INpgsqlDatabaseBuilder builder, Action<IServiceProvider, GrantDatabasePrivilegesSettings> configure)
    {
        Guard.IsNotNull(builder);
        Guard.IsNotNull(configure);

        builder.Services.AddSingleton<INpgsqlDatabaseCreator>((services) =>
        {
            GrantDatabasePrivilegesSettings settings = new();
            configure(services, settings);

            return _grantDatabasePrivilegesFactory(services, [settings.IntoSettings()]);
        });

        return builder;
    }

    /// <summary>
    /// Configures the initialization service to grant privileges to a role on a database.
    /// </summary>
    /// <param name="builder">The <see cref="INpgsqlDatabaseBuilder"/>.</param>
    /// <param name="configure">A configure delegate.</param>
    /// <returns><paramref name="builder"/>.</returns>
    public static INpgsqlDatabaseBuilder GrantDatabasePrivileges(this INpgsqlDatabaseBuilder builder, Action<GrantDatabasePrivilegesSettings> configure)
    {
        Guard.IsNotNull(builder);
        Guard.IsNotNull(configure);

        return builder.GrantDatabasePrivileges((_, options) => configure(options));
    }

    /// <summary>
    /// Configures the initialization service to grant privileges to a role on a database.
    /// </summary>
    /// <param name="builder">The <see cref="INpgsqlDatabaseBuilder"/>.</param>
    /// <param name="databaseName">The database name.</param>
    /// <param name="roleName">The role name.</param>
    /// <param name="privileges">The privileges to grant.</param>
    /// <param name="withGrantOption"><see langword="true"/> if the role <paramref name="roleName"/> should be allowed to grant these permissions to other roles.</param>
    /// <returns><paramref name="builder"/>.</returns>
    public static INpgsqlDatabaseBuilder GrantDatabasePrivileges(
        this INpgsqlDatabaseBuilder builder,
        string databaseName,
        string roleName,
        NpgsqlDatabasePrivileges privileges,
        bool withGrantOption)
    {
        Guard.IsNotNull(builder);
        Guard.IsNotNullOrEmpty(databaseName);
        Guard.IsNotNullOrEmpty(roleName);

        if ((privileges & NpgsqlDatabasePrivileges.All) == NpgsqlDatabasePrivileges.None)
        {
            return builder;
        }

        return builder.GrantDatabasePrivileges(options =>
        {
            options.DatabaseName = databaseName;
            options.RoleName = roleName;
            options.GrantCreate = privileges.HasFlag(NpgsqlDatabasePrivileges.Create);
            options.GrantConnect = privileges.HasFlag(NpgsqlDatabasePrivileges.Connect);
            options.GrantTemporary = privileges.HasFlag(NpgsqlDatabasePrivileges.Temporary);
            options.WithGrantOption = withGrantOption;
        });
    }

    /// <summary>
    /// Configures the initialization service to grant privileges to a role on a schema.
    /// </summary>
    /// <param name="builder">The <see cref="INpgsqlDatabaseBuilder"/>.</param>
    /// <param name="configure">A configure delegate.</param>
    /// <returns><paramref name="builder"/>.</returns>
    public static INpgsqlDatabaseBuilder GrantSchemaPrivileges(this INpgsqlDatabaseBuilder builder, Action<IServiceProvider, GrantSchemaPrivilegesSettings> configure)
    {
        Guard.IsNotNull(builder);
        Guard.IsNotNull(configure);

        builder.Services.AddSingleton<INpgsqlDatabaseCreator>((services) =>
        {
            GrantSchemaPrivilegesSettings settings = new();
            configure(services, settings);

            return _grantSchemaPrivilegesFactory(services, [settings.IntoSettings()]);
        });

        return builder;
    }

    /// <summary>
    /// Configures the initialization service to grant privileges to a role on a schema.
    /// </summary>
    /// <param name="builder">The <see cref="INpgsqlDatabaseBuilder"/>.</param>
    /// <param name="configure">A configure delegate.</param>
    /// <returns><paramref name="builder"/>.</returns>
    public static INpgsqlDatabaseBuilder GrantSchemaPrivileges(this INpgsqlDatabaseBuilder builder, Action<GrantSchemaPrivilegesSettings> configure)
    {
        Guard.IsNotNull(builder);
        Guard.IsNotNull(configure);

        return builder.GrantSchemaPrivileges((_, options) => configure(options));
    }

    /// <summary>
    /// Configures the initialization service to grant privileges to a role on a schema.
    /// </summary>
    /// <param name="builder">The <see cref="INpgsqlDatabaseBuilder"/>.</param>
    /// <param name="schemaName">The schema name.</param>
    /// <param name="roleName">The role name.</param>
    /// <param name="privileges">The privileges to grant.</param>
    /// <param name="withGrantOption"><see langword="true"/> if the role <paramref name="roleName"/> should be allowed to grant these permissions to other roles.</param>
    /// <returns><paramref name="builder"/>.</returns>
    public static INpgsqlDatabaseBuilder GrantSchemaPrivileges(
        this INpgsqlDatabaseBuilder builder,
        string schemaName,
        string roleName,
        NpgsqlSchemaPrivileges privileges,
        bool withGrantOption)
    {
        Guard.IsNotNull(builder);
        Guard.IsNotNullOrEmpty(schemaName);
        Guard.IsNotNullOrEmpty(roleName);

        if ((privileges & NpgsqlSchemaPrivileges.All) == NpgsqlSchemaPrivileges.None)
        {
            return builder;
        }

        return builder.GrantSchemaPrivileges(options =>
        {
            options.SchemaName = schemaName;
            options.RoleName = roleName;
            options.GrantCreate = privileges.HasFlag(NpgsqlSchemaPrivileges.Create);
            options.GrantUsage = privileges.HasFlag(NpgsqlSchemaPrivileges.Usage);
            options.WithGrantOption = withGrantOption;
        });
    }

    /// <summary>
    /// Configures the initialization service to grant default table privileges to a role on a schema.
    /// </summary>
    /// <param name="builder">The <see cref="INpgsqlDatabaseBuilder"/>.</param>
    /// <param name="configure">A configure delegate.</param>
    /// <returns><paramref name="builder"/>.</returns>
    public static INpgsqlDatabaseBuilder GrantDefaultTablePrivilegesInSchema(this INpgsqlDatabaseBuilder builder, Action<IServiceProvider, GrantDefaultTablePrivilegesSettings> configure)
    {
        Guard.IsNotNull(builder);
        Guard.IsNotNull(configure);

        builder.Services.AddSingleton<INpgsqlDatabaseCreator>((services) =>
        {
            GrantDefaultTablePrivilegesSettings settings = new();
            configure(services, settings);

            return _grantDefaultTablePrivilegesFactory(services, [settings.IntoSettings()]);
        });

        return builder;
    }

    /// <summary>
    /// Configures the initialization service to grant default table privileges to a role on a schema.
    /// </summary>
    /// <param name="builder">The <see cref="INpgsqlDatabaseBuilder"/>.</param>
    /// <param name="configure">A configure delegate.</param>
    /// <returns><paramref name="builder"/>.</returns>
    public static INpgsqlDatabaseBuilder GrantDefaultTablePrivilegesInSchema(this INpgsqlDatabaseBuilder builder, Action<GrantDefaultTablePrivilegesSettings> configure)
    {
        Guard.IsNotNull(builder);
        Guard.IsNotNull(configure);

        return builder.GrantDefaultTablePrivilegesInSchema((_, options) => configure(options));
    }

    /// <summary>
    /// Configures the initialization service to grant default table privileges to a role on a schema for all tables created by <paramref name="creatorRoleName"/>.
    /// </summary>
    /// <param name="builder">The <see cref="INpgsqlDatabaseBuilder"/>.</param>
    /// <param name="creatorRoleName">The role who's created tables will have privileges granted to <paramref name="roleName"/>.</param>
    /// <param name="roleName">The role that will be granted privileges on tables created by <paramref name="creatorRoleName"/>.</param>
    /// <param name="schemaName">The schema name.</param>
    /// <param name="privileges">The privileges to grant.</param>
    /// <param name="withGrantOption"><see langword="true"/> if the role <paramref name="roleName"/> should be allowed to grant these permissions to other roles.</param>
    /// <returns><paramref name="builder"/>.</returns>
    public static INpgsqlDatabaseBuilder GrantDefaultTablePrivilegesInSchema(
        this INpgsqlDatabaseBuilder builder,
        string creatorRoleName,
        string roleName,
        string schemaName,
        NpgsqlTablePrivileges privileges,
        bool withGrantOption)
    {
        Guard.IsNotNull(builder);
        Guard.IsNotNullOrEmpty(creatorRoleName);
        Guard.IsNotNullOrEmpty(roleName);
        Guard.IsNotNullOrEmpty(schemaName);

        if ((privileges & NpgsqlTablePrivileges.All) == NpgsqlTablePrivileges.None)
        {
            return builder;
        }

        return builder.GrantDefaultTablePrivilegesInSchema(options =>
        {
            options.CreatorRoleName = creatorRoleName;
            options.RoleName = roleName;
            options.SchemaName = schemaName;
            options.GrantInsert = privileges.HasFlag(NpgsqlTablePrivileges.Insert);
            options.GrantSelect = privileges.HasFlag(NpgsqlTablePrivileges.Select);
            options.GrantUpdate = privileges.HasFlag(NpgsqlTablePrivileges.Update);
            options.GrantDelete = privileges.HasFlag(NpgsqlTablePrivileges.Delete);
            options.GrantTruncate = privileges.HasFlag(NpgsqlTablePrivileges.Truncate);
            options.GrantReferences = privileges.HasFlag(NpgsqlTablePrivileges.References);
            options.GrantTrigger = privileges.HasFlag(NpgsqlTablePrivileges.Trigger);
            options.WithGrantOption = withGrantOption;
        });
    }

    /// <summary>
    /// Configures the initialization service to grant default sequence privileges to a role on a schema.
    /// </summary>
    /// <param name="builder">The <see cref="INpgsqlDatabaseBuilder"/>.</param>
    /// <param name="configure">A configure delegate.</param>
    /// <returns><paramref name="builder"/>.</returns>
    public static INpgsqlDatabaseBuilder GrantDefaultSequencePrivilegesInSchema(this INpgsqlDatabaseBuilder builder, Action<IServiceProvider, GrantDefaultSequencePrivilegesSettings> configure)
    {
        Guard.IsNotNull(builder);
        Guard.IsNotNull(configure);

        builder.Services.AddSingleton<INpgsqlDatabaseCreator>((services) =>
        {
            GrantDefaultSequencePrivilegesSettings settings = new();
            configure(services, settings);

            return _grantDefaultSequencePrivilegesFactory(services, [settings.IntoSettings()]);
        });

        return builder;
    }

    /// <summary>
    /// Configures the initialization service to grant default sequence privileges to a role on a schema.
    /// </summary>
    /// <param name="builder">The <see cref="INpgsqlDatabaseBuilder"/>.</param>
    /// <param name="configure">A configure delegate.</param>
    /// <returns><paramref name="builder"/>.</returns>
    public static INpgsqlDatabaseBuilder GrantDefaultSequencePrivilegesInSchema(this INpgsqlDatabaseBuilder builder, Action<GrantDefaultSequencePrivilegesSettings> configure)
    {
        Guard.IsNotNull(builder);
        Guard.IsNotNull(configure);

        return builder.GrantDefaultSequencePrivilegesInSchema((_, options) => configure(options));
    }

    /// <summary>
    /// Configures the initialization service to grant default sequence privileges to a role on a schema for all sequence created by <paramref name="creatorRoleName"/>.
    /// </summary>
    /// <param name="builder">The <see cref="INpgsqlDatabaseBuilder"/>.</param>
    /// <param name="creatorRoleName">The role who's created sequences will have privileges granted to <paramref name="roleName"/>.</param>
    /// <param name="roleName">The role that will be granted privileges on sequences created by <paramref name="creatorRoleName"/>.</param>
    /// <param name="schemaName">The schema name.</param>
    /// <param name="privileges">The privileges to grant.</param>
    /// <param name="withGrantOption"><see langword="true"/> if the role <paramref name="roleName"/> should be allowed to grant these permissions to other roles.</param>
    /// <returns><paramref name="builder"/>.</returns>
    public static INpgsqlDatabaseBuilder GrantDefaultSequencePrivilegesInSchema(
        this INpgsqlDatabaseBuilder builder,
        string creatorRoleName,
        string roleName,
        string schemaName,
        NpgsqlSequencePrivileges privileges,
        bool withGrantOption)
    {
        Guard.IsNotNull(builder);
        Guard.IsNotNullOrEmpty(creatorRoleName);
        Guard.IsNotNullOrEmpty(roleName);
        Guard.IsNotNullOrEmpty(schemaName);

        if ((privileges & NpgsqlSequencePrivileges.All) == NpgsqlSequencePrivileges.None)
        {
            return builder;
        }

        return builder.GrantDefaultSequencePrivilegesInSchema(options =>
        {
            options.CreatorRoleName = creatorRoleName;
            options.RoleName = roleName;
            options.SchemaName = schemaName;
            options.GrantUsage = privileges.HasFlag(NpgsqlSequencePrivileges.Usage);
            options.GrantSelect = privileges.HasFlag(NpgsqlSequencePrivileges.Select);
            options.GrantUpdate = privileges.HasFlag(NpgsqlSequencePrivileges.Update);
            options.WithGrantOption = withGrantOption;
        });
    }

    /// <summary>
    /// Settings for creating a database.
    /// </summary>
    public sealed class CreateDatabaseSettings
    {
        /// <summary>
        /// Gets or sets the database name.
        /// </summary>
        public string? DatabaseName { get; set; }

        /// <summary>
        /// Gets or sets the database owner (optional).
        /// </summary>
        public string? DatabaseOwner { get; set; }

        internal NpgsqlCreateDatabase.Settings IntoSettings()
        {
            Guard.IsNotNullOrEmpty(DatabaseName);

            return new(DatabaseName, DatabaseOwner);
        }
    }

    /// <summary>
    /// Settings for creating a role.
    /// </summary>
    public sealed class CreateRoleSettings
    {
        /// <summary>
        /// Gets or sets the role name.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the role password (optional).
        /// </summary>
        public string? Password { get; set; }

        internal NpgsqlCreateRole.Settings IntoSettings()
        {
            Guard.IsNotNullOrEmpty(Name);

            return new(Name, Password);
        }
    }

    /// <summary>
    /// Settings for creating a schema.
    /// </summary>
    public sealed class CreateSchemaSettings
    {
        /// <summary>
        /// Gets or sets the schema name.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the schema owner (optional).
        /// </summary>
        public string? Owner { get; set; }

        internal NpgsqlCreateSchema.Settings IntoSettings()
        {
            Guard.IsNotNullOrEmpty(Name);

            return new(Name, Owner);
        }
    }

    /// <summary>
    /// Settings for granting a role to another role.
    /// </summary>
    public sealed class GrantRoleToRoleSettings
    {
        /// <summary>
        /// Gets or sets the role to grant on.
        /// </summary>
        public string? RoleName { get; set; }

        /// <summary>
        /// Gets or sets the role to grant.
        /// </summary>
        public string? GrantedRole { get; set; }

        internal NpgsqlGrantRoleToRole.Settings IntoSettings()
        {
            Guard.IsNotNullOrEmpty(RoleName);
            Guard.IsNotNullOrEmpty(GrantedRole);

            return new(RoleName, GrantedRole);
        }
    }

    /// <summary>
    /// Settings for granting privileges to a role on a database.
    /// </summary>
    public sealed class GrantDatabasePrivilegesSettings
    {
        /// <summary>
        /// Gets or sets the role name.
        /// </summary>
        public string? RoleName { get; set; }

        /// <summary>
        /// Gets or sets the database name.
        /// </summary>
        public string? DatabaseName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to grant the <c>CREATE</c> privilege.
        /// </summary>
        public bool GrantCreate { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to grant the <c>CONNECT</c> privilege.
        /// </summary>
        public bool GrantConnect { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to grant the <c>TEMPORARY</c> privilege.
        /// </summary>
        public bool GrantTemporary { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to grant the privileges with the <c>WITH GRANT OPTION</c>.
        /// </summary>
        public bool WithGrantOption { get; set; }

        internal NpgsqlGrantDatabasePrivileges.Settings IntoSettings()
        {

            Guard.IsNotNullOrEmpty(RoleName);
            Guard.IsNotNullOrEmpty(DatabaseName);

            return new()
            {
                RoleName = RoleName,
                DatabaseName = DatabaseName,
                GrantCreate = GrantCreate,
                GrantConnect = GrantConnect,
                GrantTemporary = GrantTemporary,
                WithGrantOption = WithGrantOption,
            };
        }
    }

    /// <summary>
    /// Settings for granting privileges to a role on a schema.
    /// </summary>
    public sealed class GrantSchemaPrivilegesSettings
    {
        /// <summary>
        /// Gets or sets the role name.
        /// </summary>
        public string? RoleName { get; set; }

        /// <summary>
        /// Gets or sets the schema name.
        /// </summary>
        public string? SchemaName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to grant the <c>CREATE</c> privilege.
        /// </summary>
        public bool GrantCreate { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to grant the <c>USAGE</c> privilege.
        /// </summary>
        public bool GrantUsage { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to grant the privileges with the <c>WITH GRANT OPTION</c>.
        /// </summary>
        public bool WithGrantOption { get; set; }

        internal NpgsqlGrantSchemaPrivileges.Settings IntoSettings()
        {
            Guard.IsNotNullOrEmpty(RoleName);
            Guard.IsNotNullOrEmpty(SchemaName);

            return new()
            {
                RoleName = RoleName,
                SchemaName = SchemaName,
                GrantCreate = GrantCreate,
                GrantUsage = GrantUsage,
                WithGrantOption = WithGrantOption,
            };
        }
    }

    /// <summary>
    /// Settings for granting default table privileges to a role.
    /// </summary>
    public sealed class GrantDefaultTablePrivilegesSettings
    {
        /// <summary>
        /// Gets or sets the role name of the role that will create the tables
        /// that <see cref="RoleName"/> will be granted privileges on.
        /// </summary>
        public string? CreatorRoleName { get; set; }

        /// <summary>
        /// Gets or sets the role name of the role that will be granted privileges
        /// on tables created by <see cref="CreatorRoleName"/>.
        /// </summary>
        public string? RoleName { get; set; }

        /// <summary>
        /// Gets or sets the schema name.
        /// </summary>
        public string? SchemaName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to grant the <c>SELECT</c> privilege.
        /// </summary>
        public bool GrantSelect { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to grant the <c>INSERT</c> privilege.
        /// </summary>
        public bool GrantInsert { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to grant the <c>UPDATE</c> privilege.
        /// </summary>
        public bool GrantUpdate { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to grant the <c>DELETE</c> privilege.
        /// </summary>
        public bool GrantDelete { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to grant the <c>TRUNCATE</c> privilege.
        /// </summary>
        public bool GrantTruncate { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to grant the <c>REFERENCES</c> privilege.
        /// </summary>
        public bool GrantReferences { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to grant the <c>TRIGGER</c> privilege.
        /// </summary>
        public bool GrantTrigger { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to grant the privileges with the <c>WITH GRANT OPTION</c>.
        /// </summary>
        public bool WithGrantOption { get; set; }

        internal NpgsqlGrantDefaultTablePrivileges.Settings IntoSettings()
        {
            Guard.IsNotNullOrEmpty(RoleName);
            Guard.IsNotNullOrEmpty(SchemaName);

            return new()
            {
                CreatorRoleName = CreatorRoleName,
                RoleName = RoleName,
                SchemaName = SchemaName,
                GrantSelect = GrantSelect,
                GrantInsert = GrantInsert,
                GrantUpdate = GrantUpdate,
                GrantDelete = GrantDelete,
                GrantTruncate = GrantTruncate,
                GrantReferences = GrantReferences,
                GrantTrigger = GrantTrigger,
                WithGrantOption = WithGrantOption,
            };
        }
    }

    /// <summary>
    /// Settings for granting default sequence privileges to a role.
    /// </summary>
    public sealed class GrantDefaultSequencePrivilegesSettings
    {
        /// <summary>
        /// Gets or sets the role name of the role that will create the sequences
        /// that <see cref="RoleName"/> will be granted privileges on.
        /// </summary>
        public string? CreatorRoleName { get; set; }

        /// <summary>
        /// Gets or sets the role name of the role that will be granted privileges
        /// on sequences created by <see cref="CreatorRoleName"/>.
        /// </summary>
        public string? RoleName { get; set; }

        /// <summary>
        /// Gets or sets the schema name.
        /// </summary>
        public string? SchemaName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to grant the <c>USAGE</c> privilege.
        /// </summary>
        public bool GrantUsage { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to grant the <c>SELECT</c> privilege.
        /// </summary>
        public bool GrantSelect { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to grant the <c>UPDATE</c> privilege.
        /// </summary>
        public bool GrantUpdate { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to grant the privileges with the <c>WITH GRANT OPTION</c>.
        /// </summary>
        public bool WithGrantOption { get; set; }

        internal NpgsqlGrantDefaultSequencePrivileges.Settings IntoSettings()
        {
            Guard.IsNotNullOrEmpty(RoleName);
            Guard.IsNotNullOrEmpty(SchemaName);

            return new()
            {
                CreatorRoleName = CreatorRoleName,
                RoleName = RoleName,
                SchemaName = SchemaName,
                GrantUsage = GrantUsage,
                GrantSelect = GrantSelect,
                GrantUpdate = GrantUpdate,
                WithGrantOption = WithGrantOption,
            };
        }
    }
}
