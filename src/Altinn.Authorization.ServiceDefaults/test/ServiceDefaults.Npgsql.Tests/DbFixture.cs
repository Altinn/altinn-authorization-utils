using Testcontainers.PostgreSql;

namespace Altinn.Authorization.ServiceDefaults.Npgsql.Yuniql.Tests;

public class DbFixture
    : IAsyncLifetime
{
    private PostgreSqlContainer _container 
        = new PostgreSqlBuilder()
            .WithImage("docker.io/postgres:16.1-alpine")
            .WithUsername("superadmin_user")
            .WithPassword("superadmin_password")
            .Build();

    public async Task DisposeAsync()
    {
        await _container.DisposeAsync();
    }

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
    }

    public string ConnectionString =>
        _container.GetConnectionString();
}
