namespace Altinn.Authorization.ServiceDefaults.Npgsql;

internal class TempSharedNonPooledNpgsqlConnectionProvider
    : INpgsqlConnectionProvider
    , IAsyncDisposable
{
    private readonly string _connectionString;
    private readonly AsyncLazy<NpgsqlConnection> _connection;

    public TempSharedNonPooledNpgsqlConnectionProvider(string connectionString)
    {
        var builder = new NpgsqlConnectionStringBuilder(connectionString)
        {
            Pooling = false,
        };

        _connectionString = connectionString = builder.ConnectionString;
        _connection = CreateLazyConnection(connectionString);

        static AsyncLazy<NpgsqlConnection> CreateLazyConnection(string connectionString)
        {
            return new(async cancellationToken =>
            {
                NpgsqlConnection? connection = null;
                try
                {
                    connection = new NpgsqlConnection(connectionString);
                    await connection.OpenAsync(cancellationToken);

                    cancellationToken.ThrowIfCancellationRequested();

                    // prevent the disposal in the finally block
                    var c = connection;
                    connection = null;
                    return c;
                }
                finally
                {
                    if (connection is not null)
                    {
                        await connection.DisposeAsync();
                    }
                }
            });
        }
    }

    /// <inheritdoc />
    public string ConnectionString => _connectionString;

    /// <inheritdoc />
    public Task<NpgsqlConnection> GetConnection(CancellationToken cancellationToken)
        => _connection.WithCancellation(cancellationToken);

    /// <inheritdoc />
    ValueTask IAsyncDisposable.DisposeAsync()
        => ((IAsyncDisposable)_connection).DisposeAsync();
}
