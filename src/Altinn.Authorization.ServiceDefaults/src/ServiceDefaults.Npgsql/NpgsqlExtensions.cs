namespace Altinn.Authorization.ServiceDefaults.Npgsql;

/// <summary>
/// Extensions for Npgsql.
/// </summary>
public static class NpgsqlExtensions
{
    /// <inheritdoc cref="NpgsqlConnection.CreateCommand()"/>
    /// <param name="conn">The <see cref="NpgsqlConnection"/>.</param>
    /// <param name="sql">The command text as a string.</param>
    public static NpgsqlCommand CreateCommand(this NpgsqlConnection conn, string sql)
    {
        var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        return cmd;
    }

    /// <inheritdoc cref="NpgsqlBatch.CreateBatchCommand()"/>
    /// <param name="batch">The <see cref="NpgsqlBatch"/>.</param>
    /// <param name="sql">The command text as a string.</param>
    public static NpgsqlBatchCommand CreateBatchCommand(this NpgsqlBatch batch, string sql)
    {
        var cmd = batch.CreateBatchCommand();
        cmd.CommandText = sql;
        batch.BatchCommands.Add(cmd);
        return cmd;
    }
}
