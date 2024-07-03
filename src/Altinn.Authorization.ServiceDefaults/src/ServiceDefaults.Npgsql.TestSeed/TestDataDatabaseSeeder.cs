using Altinn.Authorization.ServiceDefaults.Npgsql.Seeding;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Npgsql;
using System.Data;

namespace Altinn.Authorization.ServiceDefaults.Npgsql.TestSeed;

/// <summary>
/// A database seeder.
/// </summary>
internal partial class TestDataDatabaseSeeder
    : INpgsqlDatabaseSeeder
{
    private readonly ILogger<TestDataDatabaseSeeder> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="TestDataDatabaseSeeder"/> class.
    /// </summary>
    public TestDataDatabaseSeeder(ILogger<TestDataDatabaseSeeder> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc/>
    async Task INpgsqlDatabaseSeeder.SeedDatabaseAsync(INpgsqlConnectionProvider connectionProvider, IServiceProvider scopedServices, CancellationToken cancellationToken)
        => await SeedDatabase(await connectionProvider.GetConnection(cancellationToken), scopedServices, cancellationToken);

    /// <summary>
    /// Seeds the database.
    /// </summary>
    /// <param name="connection">The database connection.</param>
    /// <param name="scopedServices">A scoped service provider.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    public async Task SeedDatabase(NpgsqlConnection connection, IServiceProvider scopedServices, CancellationToken cancellationToken)
    {
        Log.StartingDataSeeding(_logger);

        try
        {
            await SeedDataAsync(scopedServices, connection, cancellationToken);
        }
        catch (OperationCanceledException ex) when (ex.CancellationToken == cancellationToken)
        {
            Log.DataSeedingCancelled(_logger);
            throw;
        }
        catch (Exception ex)
        {
            Log.DataSeedingFailed(_logger, ex);
            throw;
        }

        Log.DataSeedingCompleted(_logger);
    }

    private async Task SeedDataAsync(IServiceProvider services, NpgsqlConnection connection, CancellationToken cancellationToken)
    {

        var providers = services.GetServices<ITestDataSeederProvider>();

        if (!providers.Any())
        {
            Log.NoProvidersConfigured(_logger);
            return;
        }

        var seeders = await CollectSeeders(connection, providers, cancellationToken);

        if (seeders.Count == 0)
        {
            Log.NoSeeders(_logger);
            return;
        }

        seeders.Sort((a, b) => a.Order.CompareTo(b.Order));
        await RunSeeders(connection, seeders, cancellationToken);
    }

    private async Task RunSeeders(
        NpgsqlConnection connection,
        List<ITestDataSeeder> seeders,
        CancellationToken cancellationToken)
    {
        await using var tx = await connection.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);

        await using var batch = connection.CreateBatch();
        batch.Transaction = tx;

        var builder = new BatchBuilder(batch);
        foreach (var seeder in seeders)
        {
            using var scope = seeder.BeginLoggerScope(_logger);
            try
            {
                await seeder.SeedData(builder, cancellationToken);
            }
            catch (OperationCanceledException ex) when (ex.CancellationToken == cancellationToken)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new DataSeedingFailedException(seeder, ex);
            }
        }

        await batch.ExecuteNonQueryAsync(cancellationToken);
        await tx.CommitAsync(cancellationToken);
    }

    private async Task<List<ITestDataSeeder>> CollectSeeders(
        NpgsqlConnection connection, 
        IEnumerable<ITestDataSeederProvider> providers, 
        CancellationToken cancellationToken)
    {
        const string COLLECT_SAVEPOINT = "collect";

        await using var tx = await connection.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);

        await tx.SaveAsync(COLLECT_SAVEPOINT, cancellationToken);

        var seeders = new List<ITestDataSeeder>();
        foreach (var provider in providers)
        {
            using var scope = provider.BeginLoggerScope(_logger);
            try
            {
                await foreach (var seeder in provider.GetSeeders(connection, cancellationToken))
                {
                    Log.GotSeeder(_logger, seeder.DisplayName, seeder.Order);
                    seeders.Add(seeder);
                }
            }
            catch (OperationCanceledException ex) when (ex.CancellationToken == cancellationToken)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new DataSeedingFailedException(provider, ex);
            }

            await tx.RollbackAsync(COLLECT_SAVEPOINT, cancellationToken);
        }

        await tx.RollbackAsync(cancellationToken);

        return seeders;
    }

    private static partial class Log
    {
        [LoggerMessage(0, LogLevel.Information, "Starting data seeding.")]
        public static partial void StartingDataSeeding(ILogger logger);

        [LoggerMessage(1, LogLevel.Information, "Data seeding completed.")]
        public static partial void DataSeedingCompleted(ILogger logger);

        [LoggerMessage(2, LogLevel.Error, "Data seeding failed.")]
        public static partial void DataSeedingFailed(ILogger logger, Exception exception);

        [LoggerMessage(3, LogLevel.Information, "Data seeding cancelled.")]
        public static partial void DataSeedingCancelled(ILogger logger);

        [LoggerMessage(4, LogLevel.Information, "No providers configured.")]
        public static partial void NoProvidersConfigured(ILogger logger);

        [LoggerMessage(5, LogLevel.Debug, "Got seeder {SeederName} with order {Order}.")]
        public static partial void GotSeeder(ILogger logger, string seederName, uint order);

        [LoggerMessage(6, LogLevel.Information, "No seeders provided, nothing to do.")]
        public static partial void NoSeeders(ILogger logger);
    }
}
