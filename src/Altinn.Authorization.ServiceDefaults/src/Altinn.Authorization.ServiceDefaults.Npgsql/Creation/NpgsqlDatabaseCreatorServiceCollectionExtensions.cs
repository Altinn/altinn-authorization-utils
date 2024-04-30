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
        Guard.IsNotNullOrEmpty(name);

        return builder.CreateRole(options =>
        {
            options.Name = name;
            options.Password = password;
        });
    }

    public sealed class CreateDatabaseSettings
    {
        public string? DatabaseName { get; set; }

        public string? DatabaseOwner { get; set; }

        internal NpgsqlCreateDatabase.Settings IntoSettings()
        {
            Guard.IsNotNullOrEmpty(DatabaseName);

            return new(DatabaseName, DatabaseOwner);
        }
    }

    public sealed class CreateRoleSettings
    {
        public string? Name { get; set; }

        public string? Password { get; set; }

        internal NpgsqlCreateRole.Settings IntoSettings()
        {
            Guard.IsNotNullOrEmpty(Name);

            return new(Name, Password);
        }
    }
}
