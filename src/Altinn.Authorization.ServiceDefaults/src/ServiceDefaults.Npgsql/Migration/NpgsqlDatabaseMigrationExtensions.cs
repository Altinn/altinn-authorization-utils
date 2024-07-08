using Altinn.Authorization.ServiceDefaults.Npgsql.Migration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Altinn.Authorization.ServiceDefaults.Npgsql;

/// <summary>
/// Extension methods for <see cref="INpgsqlDatabaseBuilder"/>.
/// </summary>
public static class NpgsqlDatabaseMigrationExtensions
{
    private readonly static ObjectFactory<InlineDatabaseMigration> _inlineDatabaseMigrationFactory
        = ActivatorUtilities.CreateFactory<InlineDatabaseMigration>([typeof(Func<NpgsqlConnection, IServiceProvider, CancellationToken, Task>)]);

    /// <summary>
    /// Adds an inline migration to the database initialization.
    /// </summary>
    /// <param name="builder">The <see cref="INpgsqlDatabaseBuilder"/>.</param>
    /// <param name="migration">The migration.</param>
    /// <returns><paramref name="builder"/>.</returns>
    public static INpgsqlDatabaseBuilder AddInlineMigration(
        this INpgsqlDatabaseBuilder builder, 
        Func<NpgsqlConnection, IServiceProvider, CancellationToken, Task> migration)
    {
        builder.Services.AddSingleton<INpgsqlDatabaseMigrator>((services) =>
        {
            return _inlineDatabaseMigrationFactory(services, [migration]);
        });

        return builder;
    }

    /// <summary>
    /// Adds a migrator to the database initialization.
    /// </summary>
    /// <typeparam name="TMigrator">The migrator type.</typeparam>
    /// <param name="builder">The <see cref="INpgsqlDatabaseBuilder"/>.</param>
    /// <param name="lifetime">The <see cref="ServiceLifetime"/>.</param>
    /// <returns><paramref name="builder"/>.</returns>
    public static INpgsqlDatabaseBuilder TryAddMigrator<TMigrator>(this INpgsqlDatabaseBuilder builder, ServiceLifetime lifetime = ServiceLifetime.Transient)
        where TMigrator : INpgsqlDatabaseMigrator
        => builder.TryAddMigrator(typeof(TMigrator), lifetime);

    /// <summary>
    /// Adds a migrator to the database initialization.
    /// </summary>
    /// <param name="builder">The <see cref="INpgsqlDatabaseBuilder"/>.</param>
    /// <param name="migrator">The migrator type.</param>
    /// <param name="lifetime">The <see cref="ServiceLifetime"/>.</param>
    /// <returns><paramref name="builder"/>.</returns>
    public static INpgsqlDatabaseBuilder TryAddMigrator(this INpgsqlDatabaseBuilder builder, Type migrator, ServiceLifetime lifetime = ServiceLifetime.Transient)
    {
        builder.Services.TryAdd(new ServiceDescriptor(typeof(INpgsqlDatabaseMigrator), migrator, lifetime));
        return builder;
    }
}
