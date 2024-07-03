﻿using Microsoft.Extensions.FileProviders;
using Npgsql;

namespace Altinn.Authorization.ServiceDefaults.Npgsql.Tests;

public class YuniqlTests(DbFixture fixture)
    : DatabaseTestsBase(fixture)
{
    [Fact]
    public async Task TestMigration()
    {
        await using var ctx = await CreateBuilder()
            .AddYuniqlMigrations([
                /*strpsql*/"CREATE TABLE app.test (id BIGINT PRIMARY KEY NOT NULL);",
                /*strpsql*/"INSERT INTO app.test (id) VALUES (1);",
            ]);

        var id = await ctx.ExecuteScalar<long>(/*strpsql*/"SELECT MAX(id) FROM app.test");
        id.Should().Be(1);

        var modified = await ctx.ExecuteNonQuery(/*strpsql*/"INSERT INTO app.test (id) VALUES (2), (3)");
        modified.Should().Be(2);

        id = await ctx.ExecuteScalar<long>(/*strpsql*/"SELECT MAX(id) FROM app.test");
        id.Should().Be(3);

        modified = await ctx.ExecuteNonQuery(/*strpsql*/"DELETE FROM app.test WHERE id = 3");
        modified.Should().Be(1);

        id = await ctx.ExecuteScalar<long>(/*strpsql*/"SELECT MAX(id) FROM app.test");
        id.Should().Be(2);
    }

    [Fact]
    public async Task TestSequencedTable()
    {
        await using var ctx = await CreateBuilder()
            .AddYuniqlMigrations([
                /*strpsql*/"CREATE TABLE app.test (id BIGSERIAL PRIMARY KEY NOT NULL, value BIGINT NOT NULL)",
            ]);

        var id1 = await ctx.ExecuteScalar<long>(/*strpsql*/"INSERT INTO app.test (value) VALUES (1) RETURNING id");
        var id2 = await ctx.ExecuteScalar<long>(/*strpsql*/"INSERT INTO app.test (value) VALUES (2) RETURNING id");

        var value1 = await ctx.ExecuteScalar<long>(/*strpsql*/"SELECT value FROM app.test WHERE id = @id", new NpgsqlParameter<long>("id", id1));
        var value2 = await ctx.ExecuteScalar<long>(/*strpsql*/"SELECT value FROM app.test WHERE id = @id", new NpgsqlParameter<long>("id", id2));

        value1.Should().Be(1);
        value2.Should().Be(2);
    }

    [Fact]
    public async Task TestTableWithCustomType()
    {
        await using var ctx = await CreateBuilder()
            .AddYuniqlMigrations([
                /*strpsql*/"CREATE TYPE app.test_enum AS ENUM ('one', 'two', 'three')",
                /*strpsql*/"CREATE TYPE app.test_composite AS (first app.test_enum, second app.test_enum)",
                /*strpsql*/"CREATE TABLE app.test (id BIGSERIAL PRIMARY KEY NOT NULL, value app.test_composite NOT NULL)",
            ])
            .ConfigureNpgsql(cfg =>
            {
                cfg.MapEnum<TestEnum>("app.test_enum");
                cfg.MapComposite<TestComposite>("app.test_composite");
            });

        var id1 = await ctx.ExecuteScalar<long>(/*strpsql*/"INSERT INTO app.test (value) VALUES (('one'::app.test_enum, 'one'::app.test_enum)::app.test_composite) RETURNING id");
        var id2 = await ctx.ExecuteScalar<long>(/*strpsql*/"INSERT INTO app.test (value) VALUES (('two'::app.test_enum, 'three'::app.test_enum)::app.test_composite) RETURNING id");

        var value1 = await ctx.ExecuteScalar<TestComposite>(/*strpsql*/"SELECT value FROM app.test WHERE id = @id", new NpgsqlParameter<long>("id", id1));
        var value2 = await ctx.ExecuteScalar<TestComposite>(/*strpsql*/"SELECT value FROM app.test WHERE id = @id", new NpgsqlParameter<long>("id", id2));

        value1.First.Should().Be(TestEnum.One);
        value1.Second.Should().Be(TestEnum.One);
        value2.First.Should().Be(TestEnum.Two);
        value2.Second.Should().Be(TestEnum.Three);
    }

    [Fact]
    public async Task TestFunction()
    {
        await using var ctx = await CreateBuilder()
            .AddYuniqlMigrations([
                /*strpsql*/"""
                CREATE FUNCTION app.test_function (value BIGINT)
                RETURNS BIGINT
                AS $$
                BEGIN
                    RETURN value + 1;
                END;
                $$ LANGUAGE plpgsql;
                """
            ]);

        var result = await ctx.ExecuteScalar<long>(/*strpsql*/"SELECT app.test_function(1)");

        result.Should().Be(2);
    }

    [Fact]
    public async Task TestWithEmbeddedFileProvider()
    {
        var fs = new ManifestEmbeddedFileProvider(typeof(YuniqlTests).Assembly, "TestMigrations/Test1");
        await using var ctx = await CreateBuilder().AddYuniqlMigrations(fs);

        var result = await ctx.ExecuteScalar<long>(/*strpsql*/"SELECT app.test_function(1)");

        result.Should().Be(2);
    }

    private enum TestEnum
    {
        One,
        Two,
        Three,
    }

    private record TestComposite
    {
        public TestEnum First { get; init; }
        public TestEnum Second { get; init; }
    }
}

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

        var result = await ctx.ExecuteScalar<long>(/*strpsql*/"SELECT MAX(id) FROM app.test");
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

        var result = await ctx.ExecuteScalar<long>(/*strpsql*/"SELECT MAX(id) FROM app.test");
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

        await using var result = await ctx.ExecuteReader(/*strpsql*/"SELECT value FROM app.test ORDER BY id");
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

        await using var result = await ctx.ExecuteReader(/*strpsql*/"SELECT value FROM app.test ORDER BY id");
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

        await using var result = await ctx.ExecuteReader(/*strpsql*/"SELECT value FROM app.test ORDER BY id");
        var items = new List<string>();

        while (await result.ReadAsync())
        {
            items.Add(result.GetString(0));
        }

        items.Should().BeEquivalentTo(["value"]);
    }
}
