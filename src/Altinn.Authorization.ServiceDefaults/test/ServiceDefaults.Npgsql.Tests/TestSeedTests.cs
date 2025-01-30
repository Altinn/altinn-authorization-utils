namespace Altinn.Authorization.ServiceDefaults.Npgsql.Tests;

public class TestSeedTests(DbFixture fixture)
    : DatabaseTestsBase(fixture)
{
    [Fact]
    public async Task TableSeedScript_Runs_IfTableIsEmpty()
    {
        await using var ctx = await CreateBuilder()
            .AddYuniqlMigrations([
                /*strpsql*/"CREATE TABLE app.test (id BIGINT PRIMARY KEY NOT NULL);",
            ])
            .AddTestSeedData([
                KeyValuePair.Create("[app.test].sql", /*strpsql*/"INSERT INTO app.test (id) VALUES (2);")
            ]);

        var result = await ctx.Database.ExecuteScalar<long>(/*strpsql*/"SELECT MAX(id) FROM app.test");
        result.Should().Be(2);
    }

    [Fact]
    public async Task TableSeedScript_DoesNotRun_IfTableIsNotEmpty()
    {
        await using var ctx = await CreateBuilder()
            .AddYuniqlMigrations([
                /*strpsql*/"CREATE TABLE app.test (id BIGINT PRIMARY KEY NOT NULL);",
                /*strpsql*/"INSERT INTO app.test (id) VALUES (1);",
            ])
            .AddTestSeedData([
                KeyValuePair.Create("[app.test].sql", /*strpsql*/"INSERT INTO app.test (id) VALUES (2);")
            ]);

        var result = await ctx.Database.ExecuteScalar<long>(/*strpsql*/"SELECT MAX(id) FROM app.test");
        result.Should().Be(1);
    }

    [Fact]
    public async Task ScriptGroupOrdering()
    {
        await using var ctx = await CreateBuilder()
            .AddYuniqlMigrations([
                /*strpsql*/"CREATE TABLE app.test (id BIGSERIAL PRIMARY KEY NOT NULL, value TEXT NOT NULL);",
            ])
            .AddTestSeedData([
                KeyValuePair.Create("[app.test]/01-normal.sql", /*strpsql*/"INSERT INTO app.test (value) VALUES ('01-normal');"),
                KeyValuePair.Create("[app.test]/02-normal.sql", /*strpsql*/"INSERT INTO app.test (value) VALUES ('02-normal');"),
                KeyValuePair.Create("[app.test]/_prepare.sql", /*strpsql*/"INSERT INTO app.test (value) VALUES ('prepare');"),
                KeyValuePair.Create("[app.test]/_finalize.sql", /*strpsql*/"INSERT INTO app.test (value) VALUES ('finalize');"),
            ]);

        await using var result = await ctx.Database.ExecuteReader(/*strpsql*/"SELECT value FROM app.test ORDER BY id");
        var items = new List<string>();

        while (await result.ReadAsync())
        {
            items.Add(result.GetString(0));
        }

        items.Should().BeEquivalentTo(["prepare", "01-normal", "02-normal", "finalize"]);
    }

    [Fact]
    public async Task CheckScript_CanDisable_WholeGroup()
    {
        await using var ctx = await CreateBuilder()
            .AddYuniqlMigrations([
                /*strpsql*/"CREATE TABLE app.test (id BIGSERIAL PRIMARY KEY NOT NULL, value TEXT NOT NULL);",
            ])
            .AddTestSeedData([
                KeyValuePair.Create("test/_check.sql", /*strpsql*/"SELECT FALSE;"),
                KeyValuePair.Create("test/01-normal.sql", /*strpsql*/"INSERT INTO app.test (value) VALUES ('01-normal');"),
                KeyValuePair.Create("test/02-normal.sql", /*strpsql*/"INSERT INTO app.test (value) VALUES ('02-normal');"),
                KeyValuePair.Create("test/_prepare.sql", /*strpsql*/"INSERT INTO app.test (value) VALUES ('prepare');"),
                KeyValuePair.Create("test/_finalize.sql", /*strpsql*/"INSERT INTO app.test (value) VALUES ('finalize');"),
            ]);

        await using var result = await ctx.Database.ExecuteReader(/*strpsql*/"SELECT value FROM app.test ORDER BY id");
        var items = new List<string>();

        while (await result.ReadAsync())
        {
            items.Add(result.GetString(0));
        }

        items.Should().BeEmpty();
    }

    [Fact]
    public async Task CheckScript_CanEnable_WholeGroup()
    {
        await using var ctx = await CreateBuilder()
            .AddYuniqlMigrations([
                /*strpsql*/"CREATE TABLE app.test (id BIGSERIAL PRIMARY KEY NOT NULL, value TEXT NOT NULL);",
            ])
            .AddTestSeedData([
                KeyValuePair.Create("test/_check.sql", /*strpsql*/"SELECT TRUE;"),
                KeyValuePair.Create("test/value.sql", /*strpsql*/"INSERT INTO app.test (value) VALUES ('value');"),
            ]);

        await using var result = await ctx.Database.ExecuteReader(/*strpsql*/"SELECT value FROM app.test ORDER BY id");
        var items = new List<string>();

        while (await result.ReadAsync())
        {
            items.Add(result.GetString(0));
        }

        items.Should().BeEquivalentTo(["value"]);
    }

    [Fact]
    public async Task Check_AltinnServiceDefaultsNpgsqlSeedDataV1_Support()
    {
        await using var ctx = await CreateBuilder()
            .AddYuniqlMigrations([
                /*strpsql*/"CREATE TABLE app.test (id BIGINT PRIMARY KEY NOT NULL, value TEXT NOT NULL);",
            ])
            .AddTestSeedData([
                KeyValuePair.Create("[app.test]/01-normal.sql", /*strpsql*/"INSERT INTO app.test (id, value) VALUES (1, '01-normal');"),
                KeyValuePair.Create(
                    "[app.test]/02-asdn-v1.asdn-v1",
                    """
                    COPY app.test (id, value) FROM stdin (FORMAT csv)
                    "2","02-asdn-v1"
                    "3","03-asdn-v1"
                    "4","04-asdn-v1"
                    """),
                KeyValuePair.Create("[app.test]/02-normal.sql", /*strpsql*/"INSERT INTO app.test (id, value) VALUES (5, '02-normal');"),
            ]);

        await using var result = await ctx.Database.ExecuteReader(/*strpsql*/"SELECT value FROM app.test ORDER BY id");
        var items = new List<string>();

        while (await result.ReadAsync())
        {
            items.Add(result.GetString(0));
        }

        items.Should().BeEquivalentTo(["01-normal", "02-asdn-v1", "03-asdn-v1", "04-asdn-v1", "02-normal"]);
    }
}
