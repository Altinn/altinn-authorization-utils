using Npgsql;

namespace Altinn.Authorization.ServiceDefaults.Npgsql.TestSeed;

/// <summary>
/// A wrapper around a <see cref="NpgsqlBatch"/> for building a batch of commands.
/// </summary>
public sealed class BatchBuilder
{
    private readonly NpgsqlConnection _connection;
    private readonly NpgsqlBatch _batch;

    /// <summary>
    /// Constructs a new instance of <see cref="BatchBuilder"/>.
    /// </summary>
    /// <param name="connection">The <see cref="NpgsqlConnection"/>.</param>
    /// <param name="batch">The <see cref="NpgsqlBatch"/>.</param>
    public BatchBuilder(
        NpgsqlConnection connection,
        NpgsqlBatch batch)
    {
        _connection = connection;
        _batch = batch;
    }

    /// <inheritdoc cref="NpgsqlBatch.CreateBatchCommand()"/>
    /// <param name="command">The command text.</param>
    public NpgsqlBatchCommand CreateBatchCommand(string command)
        => _batch.CreateBatchCommand(command);

    internal async Task<TextWriter> CopyIn(string query, CancellationToken cancellationToken)
    {
        if (_batch.BatchCommands.Count > 0)
        {
            await _batch.ExecuteNonQueryAsync(cancellationToken);
        }

        _batch.BatchCommands.Clear();

        return await _connection.BeginTextImportAsync(query, cancellationToken);
    }
}
