using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Npgsql;
using System.Data;

namespace Altinn.Authorization.ServiceDefaults.Npgsql.Tests;

public abstract partial class DatabaseTestsBase
{
    protected partial class AppContext
        : IAsyncDisposable
    {
        private readonly IHost _host;

        public AppContext(
            IHost host)
        {
            _host = host;
        }

        public NpgsqlDataSource DataSource =>
            _host.Services.GetRequiredService<NpgsqlDataSource>();

        public async Task<T> ExecuteScalar<T>(string sql, params NpgsqlParameter[] parameters)
        {
            await using var cmd = DataSource.CreateCommand();
            cmd.CommandText = sql;

            foreach (var p in parameters)
            {
                cmd.Parameters.Add(p);
            }

            await using var reader = await cmd.ExecuteReaderAsync(CommandBehavior.SingleRow);
            var hasResult = await reader.ReadAsync();

            hasResult.Should().BeTrue("ExecuteScalar expects a result");
            return await reader.GetFieldValueAsync<T>(0);
        }

        public async Task<int> ExecuteNonQuery(string sql)
        {
            await using var cmd = DataSource.CreateCommand();
            cmd.CommandText = sql;

            return await cmd.ExecuteNonQueryAsync();
        }

        public async ValueTask DisposeAsync()
        {
            await _host.StopAsync();

            if (_host is IAsyncDisposable h)
            {
                await h.DisposeAsync();
            }

            _host.Dispose();
        }
    }
}
