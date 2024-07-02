namespace Altinn.Authorization.ServiceDefaults.Npgsql.TestSeed;

/// <summary>
/// A test-data seeder.
/// </summary>
public interface ITestDataSeeder
    : ITestDataSource
{
    /// <summary>
    /// Gets the order in which the seeder should be executed.
    /// </summary>
    uint Order { get; }

    /// <summary>
    /// Adds the test-data to a batch.
    /// </summary>
    /// <param name="batch">The database batch builder.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/>.</param>
    /// <returns></returns>
    Task SeedData(BatchBuilder batch, CancellationToken cancellationToken = default);
}
