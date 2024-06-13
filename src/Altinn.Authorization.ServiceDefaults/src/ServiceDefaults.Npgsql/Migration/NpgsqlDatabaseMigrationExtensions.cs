using Altinn.Authorization.ServiceDefaults.Npgsql.Migration;
using Microsoft.Extensions.DependencyInjection;

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
}
