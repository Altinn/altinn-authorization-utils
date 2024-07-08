namespace Altinn.Authorization.ServiceDefaults.Npgsql.TestSeed;

/// <summary>
/// An exception that is thrown when data seeding fails.
/// </summary>
internal class DataSeedingFailedException 
    : InvalidOperationException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DataSeedingFailedException"/> class.
    /// </summary>
    /// <param name="source">The <see cref="ITestDataSource"/>.</param>
    /// <param name="innerException">The inner exception.</param>
    public DataSeedingFailedException(ITestDataSource source, Exception? innerException)
        : this($"Data seeding failed for {source.DisplayName}.", innerException)
    {
        TestDataSource = source;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DataSeedingFailedException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The inner exception.</param>
    private DataSeedingFailedException(string message, Exception? innerException)
        : base(message, innerException)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DataSeedingFailedException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    public DataSeedingFailedException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DataSeedingFailedException"/> class.
    /// </summary>
    public DataSeedingFailedException()
        : base("Data seeding failed.")
    {
    }

    /// <summary>
    /// Gets the <see cref="ITestDataSource"/> that caused the exception.
    /// </summary>
    public ITestDataSource? TestDataSource { get; }
}
