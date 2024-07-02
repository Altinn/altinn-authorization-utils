using Altinn.Authorization.ServiceDefaults.Npgsql.Seeding;
using Altinn.Authorization.ServiceDefaults.Npgsql.TestSeed.FileBased;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.FileProviders;

namespace Altinn.Authorization.ServiceDefaults.Npgsql.TestSeed;

/// <summary>
/// Extension methods for configuring test-seed services.
/// </summary>
public static class TestSeedServiceCollectionExtensions
{
    private static readonly ObjectFactory<SeedDataDirectoryTestDataSeederProvider> _dirProviderFactory
        = ActivatorUtilities.CreateFactory<SeedDataDirectoryTestDataSeederProvider>([typeof(SeedDataDirectorySettings)]);

    /// <summary>
    /// Adds a test-data seeder provider to the initialization of the database.
    /// </summary>
    /// <typeparam name="T">The type of the test-data seeder provider.</typeparam>
    /// <param name="builder">The <see cref="INpgsqlDatabaseBuilder"/>.</param>
    /// <param name="lifetime">The <see cref="ServiceLifetime"/>.</param>
    /// <returns><paramref name="builder"/>.</returns>
    public static INpgsqlDatabaseBuilder TryAddTestSeederProvider<T>(
        this INpgsqlDatabaseBuilder builder,
        ServiceLifetime lifetime = ServiceLifetime.Transient)
        where T : ITestDataSeederProvider
        => builder.TryAddTestSeederProvider(typeof(T), lifetime);

    /// <summary>
    /// Adds a test-data seeder provider to the initialization of the database.
    /// </summary>
    /// <param name="builder">The <see cref="INpgsqlDatabaseBuilder"/>.</param>
    /// <param name="providerType">The type of the test-data seeder provider.</param>
    /// <param name="lifetime">The <see cref="ServiceLifetime"/>.</param>
    /// <returns><paramref name="builder"/>.</returns>
    public static INpgsqlDatabaseBuilder TryAddTestSeederProvider(
        this INpgsqlDatabaseBuilder builder,
        Type providerType,
        ServiceLifetime lifetime = ServiceLifetime.Transient)
    {
        builder.AddTestDataSeeding();
        builder.Services.TryAddEnumerable(new ServiceDescriptor(typeof(ITestDataSeederProvider), providerType, lifetime));

        return builder;
    }

    /// <summary>
    /// Adds a test-data seeder provider to the initialization of the database.
    /// </summary>
    /// <param name="builder">The <see cref="INpgsqlDatabaseSeeder"/>.</param>
    /// <param name="factory">The service factory.</param>
    /// <param name="lifetime">The <see cref="ServiceLifetime"/>.</param>
    /// <returns><paramref name="builder"/>.</returns>
    public static INpgsqlDatabaseBuilder AddTestSeederProvider(
        this INpgsqlDatabaseBuilder builder,
        Func<IServiceProvider, ITestDataSeederProvider> factory,
        ServiceLifetime lifetime = ServiceLifetime.Transient)
    {
        builder.AddTestDataSeeding();
        builder.Services.Add(ServiceDescriptor.Describe(typeof(ITestDataSeederProvider), factory, lifetime));
        
        return builder;
    }

    /// <summary>
    /// Seeds the database with test-data from a file provider.
    /// </summary>
    /// <param name="builder">The <see cref="INpgsqlDatabaseSeeder"/>.</param>
    /// <param name="configure">Configuration delegate.</param>
    /// <returns><paramref name="builder"/>.</returns>
    public static INpgsqlDatabaseBuilder SeedFromFileProvider(
        this INpgsqlDatabaseBuilder builder,
        Action<IServiceProvider, SeedDataDirectorySettings> configure)
    {
        return builder.AddTestSeederProvider(sp =>
        {
            var settings = new SeedDataDirectorySettings();
            configure(sp, settings);

            return _dirProviderFactory(sp, [settings]);
        });
    }

    /// <summary>
    /// Seeds the database with test-data from a file provider.
    /// </summary>
    /// <param name="builder">The <see cref="INpgsqlDatabaseSeeder"/>.</param>
    /// <param name="fileProvider">The file provider containing the test-data files.</param>
    /// <param name="subPath">Optional sub-path within <paramref name="fileProvider"/>.</param>
    /// <returns><paramref name="builder"/>.</returns>
    public static INpgsqlDatabaseBuilder SeedFromFileProvider(
        this INpgsqlDatabaseBuilder builder,
        IFileProvider fileProvider,
        string? subPath = null)
    {
        return builder.SeedFromFileProvider((_, settings) =>
        {
            settings.FileProvider = fileProvider;
            settings.DirectoryPath = subPath;
        });
    }

    /// <summary>
    /// Seeds the database with test-data from a directory path.
    /// </summary>
    /// <param name="builder">The <see cref="INpgsqlDatabaseSeeder"/>.</param>
    /// <param name="directoryPath">The directory path.</param>
    /// <returns><paramref name="builder"/>.</returns>
    public static INpgsqlDatabaseBuilder SeedFromDirectory(
        this INpgsqlDatabaseBuilder builder,
        Func<IServiceProvider, string> directoryPath)
    {
        return builder.SeedFromFileProvider((sp, settings) =>
        {
            settings.DirectoryPath = directoryPath(sp);
        });
    }

    /// <summary>
    /// Seeds the database with test-data from a directory path.
    /// </summary>
    /// <param name="builder">The <see cref="INpgsqlDatabaseSeeder"/>.</param>
    /// <param name="directoryPath">The directory path.</param>
    /// <returns><paramref name="builder"/>.</returns>
    public static INpgsqlDatabaseBuilder SeedFromDirectory(
        this INpgsqlDatabaseBuilder builder,
        string directoryPath)
    {
        return builder.SeedFromFileProvider((sp, settings) =>
        {
            settings.DirectoryPath = directoryPath;
        });
    }

    private static INpgsqlDatabaseBuilder AddTestDataSeeding(this INpgsqlDatabaseBuilder builder) 
        => builder.TryAddSeeder<TestDataDatabaseSeeder>();
}
