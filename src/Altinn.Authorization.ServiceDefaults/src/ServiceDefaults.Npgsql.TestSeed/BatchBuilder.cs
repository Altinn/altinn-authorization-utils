using Npgsql;

namespace Altinn.Authorization.ServiceDefaults.Npgsql.TestSeed;

/// <summary>
/// A wrapper around a <see cref="NpgsqlBatch"/> for building a batch of commands.
/// </summary>
public sealed class BatchBuilder
{
    private readonly NpgsqlBatch _batch;

    /// <summary>
    /// Constructs a new instance of <see cref="BatchBuilder"/>.
    /// </summary>
    /// <param name="batch">The <see cref="NpgsqlBatch"/>.</param>
    public BatchBuilder(NpgsqlBatch batch)
    {
        _batch = batch;
    }

    /// <inheritdoc cref="NpgsqlBatch.CreateBatchCommand()"/>
    /// <param name="command">The command text.</param>
    public NpgsqlBatchCommand CreateBatchCommand(string command)
        => _batch.CreateBatchCommand(command);
}
