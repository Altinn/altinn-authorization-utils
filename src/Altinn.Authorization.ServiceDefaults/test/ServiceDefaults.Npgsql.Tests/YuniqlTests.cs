using Altinn.Authorization.ServiceDefaults.Npgsql.Tests.Utils;
using Microsoft.Extensions.FileProviders;
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

        var id = await ctx.Database.ExecuteScalar<long>(/*strpsql*/"SELECT MAX(id) FROM app.test");
        id.Should().Be(1);

        var modified = await ctx.Database.ExecuteNonQuery(/*strpsql*/"INSERT INTO app.test (id) VALUES (2), (3)");
        modified.Should().Be(2);

        id = await ctx.Database.ExecuteScalar<long>(/*strpsql*/"SELECT MAX(id) FROM app.test");
        id.Should().Be(3);

        modified = await ctx.Database.ExecuteNonQuery(/*strpsql*/"DELETE FROM app.test WHERE id = 3");
        modified.Should().Be(1);

        id = await ctx.Database.ExecuteScalar<long>(/*strpsql*/"SELECT MAX(id) FROM app.test");
        id.Should().Be(2);
    }

    [Fact]
    public async Task TestSequencedTable()
    {
        await using var ctx = await CreateBuilder()
            .AddYuniqlMigrations([
                /*strpsql*/"CREATE TABLE app.test (id BIGSERIAL PRIMARY KEY NOT NULL, value BIGINT NOT NULL)",
            ]);

        var id1 = await ctx.Database.ExecuteScalar<long>(/*strpsql*/"INSERT INTO app.test (value) VALUES (1) RETURNING id");
        var id2 = await ctx.Database.ExecuteScalar<long>(/*strpsql*/"INSERT INTO app.test (value) VALUES (2) RETURNING id");

        var value1 = await ctx.Database.ExecuteScalar<long>(/*strpsql*/"SELECT value FROM app.test WHERE id = @id", new NpgsqlParameter<long>("id", id1));
        var value2 = await ctx.Database.ExecuteScalar<long>(/*strpsql*/"SELECT value FROM app.test WHERE id = @id", new NpgsqlParameter<long>("id", id2));

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

        var id1 = await ctx.Database.ExecuteScalar<long>(/*strpsql*/"INSERT INTO app.test (value) VALUES (('one'::app.test_enum, 'one'::app.test_enum)::app.test_composite) RETURNING id");
        var id2 = await ctx.Database.ExecuteScalar<long>(/*strpsql*/"INSERT INTO app.test (value) VALUES (('two'::app.test_enum, 'three'::app.test_enum)::app.test_composite) RETURNING id");

        var value1 = await ctx.Database.ExecuteScalar<TestComposite>(/*strpsql*/"SELECT value FROM app.test WHERE id = @id", new NpgsqlParameter<long>("id", id1));
        var value2 = await ctx.Database.ExecuteScalar<TestComposite>(/*strpsql*/"SELECT value FROM app.test WHERE id = @id", new NpgsqlParameter<long>("id", id2));

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

        var result = await ctx.Database.ExecuteScalar<long>(/*strpsql*/"SELECT app.test_function(1)");

        result.Should().Be(2);
    }

    [Fact]
    public async Task TestWithEmbeddedFileProvider()
    {
        var fs = new ManifestEmbeddedFileProvider(typeof(YuniqlTests).Assembly, "TestMigrations/Test1");
        await using var ctx = await CreateBuilder().AddYuniqlMigrations(fs);

        var result = await ctx.Database.ExecuteScalar<long>(/*strpsql*/"SELECT app.test_function(1)");

        result.Should().Be(2);
    }

    [Fact]
    public async Task Multiple_Yuniqls_With_Different_Configs()
    {
        await using var ctx = await CreateBuilder()
            // default migration
            .AddYuniqlMigrations(cfg =>
            {
                cfg.WorkspaceFileProvider = YuniqlTestFileProvider.Create([
                    /*strpsql*/"GRANT USAGE ON SCHEMA yuniql TO \"${APP-USER}\"",
                    /*strpsql*/"CREATE TABLE app.test1 (id BIGSERIAL PRIMARY KEY NOT NULL, value TEXT NOT NULL)",
                    /*strpsql*/"INSERT INTO app.test1 (value) VALUES ('${TEST-VALUE}'), ('${APP-USER}'), ('${YUNIQL-USER}')",
                    /*strpsql*/"GRANT SELECT ON TABLE yuniql.migrations1 TO \"${APP-USER}\"",
                ]);
                cfg.MigrationsTable.Schema = "yuniql";
                cfg.MigrationsTable.Name = "migrations1";
                cfg.Tokens.Add("TEST-VALUE", "1");
            })
            .AddYuniqlMigrations(typeof(TestEnum), cfg =>
            {
                cfg.WorkspaceFileProvider = YuniqlTestFileProvider.Create([
                    /*strpsql*/"GRANT USAGE ON SCHEMA yuniql TO \"${APP-USER}\"",
                    /*strpsql*/"CREATE TABLE app.test2 (id BIGSERIAL PRIMARY KEY NOT NULL, value TEXT NOT NULL)",
                    /*strpsql*/"INSERT INTO app.test2 (value) VALUES ('${TEST-VALUE}')",
                    /*strpsql*/"INSERT INTO app.test2 (value) VALUES ('${APP-USER}')",
                    /*strpsql*/"INSERT INTO app.test2 (value) VALUES ('${YUNIQL-USER}')",
                    /*strpsql*/"GRANT SELECT ON TABLE yuniql.migrations2 TO \"${APP-USER}\"",
                ]);
                cfg.MigrationsTable.Schema = "yuniql";
                cfg.MigrationsTable.Name = "migrations2";
                cfg.Tokens.Add("TEST-VALUE", "2");
            });

        var migrations1Count = await ctx.Database.ExecuteScalar<long>(/*strpsql*/"SELECT COUNT(*) FROM yuniql.migrations1");
        var migrations2Count = await ctx.Database.ExecuteScalar<long>(/*strpsql*/"SELECT COUNT(*) FROM yuniql.migrations2");

        migrations1Count.Should().Be(4);
        migrations2Count.Should().Be(6);

        var test1Contains = await ctx.Database.ExecuteScalar<long>(/*strpsql*/"SELECT COUNT(*) FROM app.test1 WHERE value = '1'");
        var test2Contains = await ctx.Database.ExecuteScalar<long>(/*strpsql*/"SELECT COUNT(*) FROM app.test2 WHERE value = '2'");

        test1Contains.Should().Be(1);
        test2Contains.Should().Be(1);

        test1Contains = await ctx.Database.ExecuteScalar<long>(/*strpsql*/$"SELECT COUNT(*) FROM app.test1 WHERE value = '{ctx.Database.AppRole}'");
        test2Contains = await ctx.Database.ExecuteScalar<long>(/*strpsql*/$"SELECT COUNT(*) FROM app.test2 WHERE value = '{ctx.Database.AppRole}'");

        test1Contains.Should().Be(1);
        test2Contains.Should().Be(1);

        test1Contains = await ctx.Database.ExecuteScalar<long>(/*strpsql*/$"SELECT COUNT(*) FROM app.test1 WHERE value = '{ctx.Database.MigratorRole}'");
        test2Contains = await ctx.Database.ExecuteScalar<long>(/*strpsql*/$"SELECT COUNT(*) FROM app.test2 WHERE value = '{ctx.Database.MigratorRole}'");

        test1Contains.Should().Be(1);
        test2Contains.Should().Be(1);
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
