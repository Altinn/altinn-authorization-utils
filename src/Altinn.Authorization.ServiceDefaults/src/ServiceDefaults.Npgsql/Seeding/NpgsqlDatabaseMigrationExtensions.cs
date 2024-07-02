using Altinn.Authorization.ServiceDefaults.Npgsql.Seeding;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Altinn.Authorization.ServiceDefaults.Npgsql;

/// <summary>
/// Extension methods for <see cref="INpgsqlDatabaseBuilder"/>.
/// </summary>
public static class NpgsqlDatabaseSeedingExtensions
{
    /// <summary>
    /// Adds a seeder to the database initialization.
    /// </summary>
    /// <typeparam name="TSeeder">The seeder type.</typeparam>
    /// <param name="builder">The <see cref="INpgsqlDatabaseBuilder"/>.</param>
    /// <param name="lifetime">The <see cref="ServiceLifetime"/>.</param>
    /// <returns><paramref name="builder"/>.</returns>
    public static INpgsqlDatabaseBuilder TryAddSeeder<TSeeder>(this INpgsqlDatabaseBuilder builder, ServiceLifetime lifetime = ServiceLifetime.Transient)
        where TSeeder : INpgsqlDatabaseSeeder
        => builder.TryAddSeeder(typeof(TSeeder), lifetime);

    /// <summary>
    /// Adds a seeder to the database initialization.
    /// </summary>
    /// <param name="builder">The <see cref="INpgsqlDatabaseBuilder"/>.</param>
    /// <param name="migrator">The seeder type.</param>
    /// <param name="lifetime">The <see cref="ServiceLifetime"/>.</param>
    /// <returns><paramref name="builder"/>.</returns>
    public static INpgsqlDatabaseBuilder TryAddSeeder(this INpgsqlDatabaseBuilder builder, Type migrator, ServiceLifetime lifetime = ServiceLifetime.Transient)
    {
        builder.Services.TryAdd(new ServiceDescriptor(typeof(INpgsqlDatabaseSeeder), migrator, lifetime));
        return builder;
    }
}
