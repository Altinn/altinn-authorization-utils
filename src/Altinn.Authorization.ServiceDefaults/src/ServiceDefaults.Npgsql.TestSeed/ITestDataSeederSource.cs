namespace Altinn.Authorization.ServiceDefaults.Npgsql.TestSeed;

/// <summary>
/// A test-data source.
/// </summary>
public interface ITestDataSource
{
    /// <summary>
    /// Gets the display name of the test-data source.
    /// </summary>
    string DisplayName { get; }
}
