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
    /// <param name="roleName"></param>
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

        if (privileges == NpgsqlDatabasePrivileges.None)
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

    public sealed class CreateSchemaSettings
    {
        public string? Name { get; set; }

        public string? Owner { get; set; }

        internal NpgsqlCreateSchema.Settings IntoSettings()
        {
            Guard.IsNotNullOrEmpty(Name);

            return new(Name, Owner);
        }
    }

    public sealed class GrantRoleToRoleSettings
    {
        public string? RoleName { get; set; }

        public string? GrantedRole { get; set; }

        internal NpgsqlGrantRoleToRole.Settings IntoSettings()
        {
            Guard.IsNotNullOrEmpty(RoleName);
            Guard.IsNotNullOrEmpty(GrantedRole);

            return new(RoleName, GrantedRole);
        }
    }

    public sealed class GrantDatabasePrivilegesSettings
    {
        public string? RoleName { get; set; }

        public string? DatabaseName { get; set; }

        public bool GrantCreate { get; set; }

        public bool GrantConnect { get; set; }

        public bool GrantTemporary { get; set; }

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
}
