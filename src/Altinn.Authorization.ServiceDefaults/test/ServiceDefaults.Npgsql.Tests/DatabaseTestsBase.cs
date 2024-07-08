namespace Altinn.Authorization.ServiceDefaults.Npgsql.Tests;

public abstract partial class DatabaseTestsBase
    : IClassFixture<DbFixture>
{
    private static int _counter = 0;

    private readonly DbFixture _fixture;

    public DatabaseTestsBase(
        DbFixture fixture)
    {
        _fixture = fixture;
    }
}
