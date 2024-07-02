using Npgsql;

namespace Altinn.Authorization.ServiceDefaults.Npgsql.TestSeed;

/// <summary>
/// A test-data seeder provider.
/// </summary>
public interface ITestDataSeederProvider
    : ITestDataSource
{
    /// <summary>
    /// Gets the seeders provided by this provider.
    /// </summary>
    /// <param name="db">A <see cref="NpgsqlConnection"/> that can be used to decide which seeders to provide.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/>.</param>
    /// <returns>An async enumerable of <see cref="ITestDataSeeder"/>.</returns>
    /// <remarks>
    /// The database is rolled back to before this method is called, such that any changes made to the database
    /// during the execution of this method are discarded.
    /// </remarks>
    IAsyncEnumerable<ITestDataSeeder> GetSeeders(NpgsqlConnection db, CancellationToken cancellationToken = default);
}
