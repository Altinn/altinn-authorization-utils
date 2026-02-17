using Altinn.Authorization.ServiceDefaults.Npgsql.Telemetry;
using Npgsql;
using NpgsqlTypes;
using System.Data;

namespace Altinn.Authorization.ServiceDefaults.Npgsql.Tests;

public class TelemetryTests(DbFixture fixture)
    : DatabaseTestsBase(fixture)
{
    [Fact]
    public async Task Telemetry_CanBeDisabled()
    {
        await using var ctx = await CreateBuilder();

        using (NpgsqlTelemetry.DisableTracing())
        {
            await ctx.Database.ExecuteScalar<int>("SELECT 2");
            
            await using var batch = ctx.Database.DataSource.CreateBatch();
            batch.CreateBatchCommand("SELECT 2");
            batch.CreateBatchCommand("SELECT 3");
            await using var reader = await batch.ExecuteReaderAsync(TestContext.Current.CancellationToken);
            await Drain(reader);
        }

        var activities = ctx.Activities;
        activities.ShouldBeEmpty();
    }

    [Fact]
    public async Task Telemetry_HealthCheckCommand_IsNotTraced()
    {
        await using var ctx = await CreateBuilder();

        await ctx.Database.ExecuteScalar<int>("SELECT 1");

        var activities = ctx.Activities;
        activities.ShouldBeEmpty();
    }

    [Fact]
    public async Task Command_GetsCleanedAndEnriched()
    {
        await using var ctx = await CreateBuilder();

        await ctx.Database.ExecuteScalar<int>("SELECT 2");

        var activities = ctx.Activities;
        var activity = activities.ShouldHaveSingleItem();

        activity.GetTagItem("db.connection_id").ShouldBeNull();
        activity.GetTagItem("db.connection_string").ShouldBeNull();
        activity.GetTagItem("db.name").ShouldBeNull();
        activity.GetTagItem("db.user").ShouldBeNull();
        activity.GetTagItem("net.peer.ip").ShouldBeNull();
        activity.GetTagItem("net.peer.name").ShouldBeNull();
        activity.GetTagItem("net.peer.port").ShouldBeNull();
        activity.GetTagItem("net.transport").ShouldBeNull();
        activity.GetTagItem("db.statement").ShouldBeNull();
        activity.GetTagItem("db.query.text").ShouldBeNull();
        activity.GetTagItem("db.query.summary").ShouldBeNull();
        activity.GetTagItem("db.query.hash").ShouldBe(AltinnNpgsqlTelemetry.QueryHasher.ComputeHashAndString("SELECT 2").HexString);
    }

    [Fact]
    public async Task Batch_Single_GetsCleanedAndEnriched()
    {
        await using var ctx = await CreateBuilder();

        {
            await using var batch = ctx.Database.DataSource.CreateBatch();
            batch.CreateBatchCommand("SELECT 2");
            await using var reader = await batch.ExecuteReaderAsync(TestContext.Current.CancellationToken);
            await Drain(reader);
        }

        var activities = ctx.Activities;
        var activity = activities.ShouldHaveSingleItem();

        activity.GetTagItem("db.connection_id").ShouldBeNull();
        activity.GetTagItem("db.connection_string").ShouldBeNull();
        activity.GetTagItem("db.name").ShouldBeNull();
        activity.GetTagItem("db.user").ShouldBeNull();
        activity.GetTagItem("net.peer.ip").ShouldBeNull();
        activity.GetTagItem("net.peer.name").ShouldBeNull();
        activity.GetTagItem("net.peer.port").ShouldBeNull();
        activity.GetTagItem("net.transport").ShouldBeNull();
        activity.GetTagItem("db.statement").ShouldBeNull();
        activity.GetTagItem("db.query.text").ShouldBeNull();
        activity.GetTagItem("db.query.summary").ShouldBeNull();
        activity.GetTagItem("db.query.hash").ShouldBe(AltinnNpgsqlTelemetry.QueryHasher.ComputeHashAndString("SELECT 2").HexString);
    }

    [Fact]
    public async Task Batch_Multiple_GetsCleanedAndEnriched()
    {
        await using var ctx = await CreateBuilder();

        {
            await using var batch = ctx.Database.DataSource.CreateBatch();
            batch.CreateBatchCommand("SELECT 2");
            batch.CreateBatchCommand("SELECT 3");
            await using var reader = await batch.ExecuteReaderAsync(TestContext.Current.CancellationToken);
            await Drain(reader);
        }

        var activities = ctx.Activities;
        var activity = activities.ShouldHaveSingleItem();

        activity.GetTagItem("db.connection_id").ShouldBeNull();
        activity.GetTagItem("db.connection_string").ShouldBeNull();
        activity.GetTagItem("db.name").ShouldBeNull();
        activity.GetTagItem("db.user").ShouldBeNull();
        activity.GetTagItem("net.peer.ip").ShouldBeNull();
        activity.GetTagItem("net.peer.name").ShouldBeNull();
        activity.GetTagItem("net.peer.port").ShouldBeNull();
        activity.GetTagItem("net.transport").ShouldBeNull();
        activity.GetTagItem("db.statement").ShouldBeNull();
        activity.GetTagItem("db.query.text").ShouldBeNull();
        activity.GetTagItem("db.query.summary").ShouldBeNull();
        activity.GetTagItem("db.query.hash").ShouldBe(
            $"{AltinnNpgsqlTelemetry.QueryHasher.ComputeHashAndString("SELECT 2").HexString},{AltinnNpgsqlTelemetry.QueryHasher.ComputeHashAndString("SELECT 3").HexString}");
    }

    [Fact]
    public async Task CanSetSummary()
    {
        await using var ctx = await CreateBuilder();

        using (NpgsqlTelemetry.Configure(summary: "test summary")) 
        {
            await ctx.Database.ExecuteNonQuery("SELECT 2");
        }

        var activities = ctx.Activities;
        var activity = activities.ShouldHaveSingleItem();

        activity.GetTagItem("db.query.summary").ShouldBe("test summary");
    }

    [Fact]
    public async Task CanSetSpanName()
    {
        await using var ctx = await CreateBuilder();

        using (NpgsqlTelemetry.Configure(spanName: "test name"))
        {
            await ctx.Database.ExecuteNonQuery("SELECT 2");
        }

        var activities = ctx.Activities;
        var activity = activities.ShouldHaveSingleItem();

        activity.DisplayName.ShouldBe("test name");
    }

    [Fact]
    public async Task Parameters_AreTraced_WithRedaction()
    {
        await using var ctx = await CreateBuilder();

        var inNumber = Random.Shared.Next();
        var inGuid = Guid.NewGuid();
        var inDate = new DateOnly(2020, Random.Shared.Next(1, 11), Random.Shared.Next(1, 25));
        var inDateTime = new DateTimeOffset(inDate, new TimeOnly(Random.Shared.Next(1, 20), Random.Shared.Next(1, 55)), TimeSpan.Zero);
        var inString = Guid.NewGuid().ToString();

        {
            await using var reader = await ctx.Database.ExecuteReader(
                /*strpsql*/"""
                SELECT @num "num", @guid "guid", @date "date", @dateTime "dateTime", @string "string"
                """,
                new NpgsqlParameter<int?>("num", NpgsqlDbType.Integer) { TypedValue = inNumber },
                new NpgsqlParameter<Guid?>("guid", NpgsqlDbType.Uuid) { TypedValue = inGuid },
                new NpgsqlParameter<DateOnly?>("date", NpgsqlDbType.Date) { TypedValue = inDate },
                new NpgsqlParameter<DateTimeOffset?>("dateTime", NpgsqlDbType.TimestampTz) { TypedValue = inDateTime },
                new NpgsqlParameter<string>("string", NpgsqlDbType.Text) { TypedValue = inString });

            await Drain(reader);
        }

        var activities = ctx.Activities;
        var activity = activities.ShouldHaveSingleItem();

        activity.GetTagItem("db.query.parameters.num").ShouldBe(inNumber);
        activity.GetTagItem("db.query.parameters.guid").ShouldBe(inGuid);
        activity.GetTagItem("db.query.parameters.date").ShouldBe(inDate);
        activity.GetTagItem("db.query.parameters.dateTime").ShouldBe(inDateTime);
        activity.GetTagItem("db.query.parameters.string").ShouldBe(AltinnNpgsqlTelemetry.RedactedPlaceholder);
    }

    [Fact]
    public async Task Parameters_Redaction_CanBeConfigured_ByType()
    {
        await using var ctx = await CreateBuilder();

        var inNumber = Random.Shared.Next();
        var inGuid = Guid.NewGuid();
        var inDate = new DateOnly(2020, Random.Shared.Next(1, 11), Random.Shared.Next(1, 25));
        var inDateTime = new DateTimeOffset(inDate, new TimeOnly(Random.Shared.Next(1, 20), Random.Shared.Next(1, 55)), TimeSpan.Zero);
        var inString1 = Guid.NewGuid().ToString();
        var inString2 = Guid.NewGuid().ToString();

        using (NpgsqlTelemetry.Configure(traceParameterTypes: [typeof(string)]))
        {
            await using var reader = await ctx.Database.ExecuteReader(
                /*strpsql*/"""
                SELECT @num "num", @guid "guid", @date "date", @dateTime "dateTime", @string1 "string1", @string2 "string2"
                """,
                new NpgsqlParameter<int?>("num", NpgsqlDbType.Integer) { TypedValue = inNumber },
                new NpgsqlParameter<Guid?>("guid", NpgsqlDbType.Uuid) { TypedValue = inGuid },
                new NpgsqlParameter<DateOnly?>("date", NpgsqlDbType.Date) { TypedValue = inDate },
                new NpgsqlParameter<DateTimeOffset?>("dateTime", NpgsqlDbType.TimestampTz) { TypedValue = inDateTime },
                new NpgsqlParameter<string>("string1", NpgsqlDbType.Text) { TypedValue = inString1 },
                new NpgsqlParameter<string>("string2", NpgsqlDbType.Text) { TypedValue = inString2 });

            await Drain(reader);
        }

        var activities = ctx.Activities;
        var activity = activities.ShouldHaveSingleItem();

        activity.GetTagItem("db.query.parameters.num").ShouldBe(inNumber);
        activity.GetTagItem("db.query.parameters.guid").ShouldBe(inGuid);
        activity.GetTagItem("db.query.parameters.date").ShouldBe(inDate);
        activity.GetTagItem("db.query.parameters.dateTime").ShouldBe(inDateTime);
        activity.GetTagItem("db.query.parameters.string1").ShouldBe(inString1);
        activity.GetTagItem("db.query.parameters.string2").ShouldBe(inString2);
    }

    [Fact]
    public async Task Parameters_Redaction_CanBeConfigured_ByName()
    {
        await using var ctx = await CreateBuilder();

        var inNumber = Random.Shared.Next();
        var inGuid = Guid.NewGuid();
        var inDate = new DateOnly(2020, Random.Shared.Next(1, 11), Random.Shared.Next(1, 25));
        var inDateTime = new DateTimeOffset(inDate, new TimeOnly(Random.Shared.Next(1, 20), Random.Shared.Next(1, 55)), TimeSpan.Zero);
        var inString1 = Guid.NewGuid().ToString();
        var inString2 = Guid.NewGuid().ToString();

        using (NpgsqlTelemetry.Configure(traceParameterNames: ["string1"]))
        {
            await using var reader = await ctx.Database.ExecuteReader(
                /*strpsql*/"""
                SELECT @num "num", @guid "guid", @date "date", @dateTime "dateTime", @string1 "string1", @string2 "string2"
                """,
                new NpgsqlParameter<int?>("num", NpgsqlDbType.Integer) { TypedValue = inNumber },
                new NpgsqlParameter<Guid?>("guid", NpgsqlDbType.Uuid) { TypedValue = inGuid },
                new NpgsqlParameter<DateOnly?>("date", NpgsqlDbType.Date) { TypedValue = inDate },
                new NpgsqlParameter<DateTimeOffset?>("dateTime", NpgsqlDbType.TimestampTz) { TypedValue = inDateTime },
                new NpgsqlParameter<string>("string1", NpgsqlDbType.Text) { TypedValue = inString1 },
                new NpgsqlParameter<string>("string2", NpgsqlDbType.Text) { TypedValue = inString2 });

            await Drain(reader);
        }

        var activities = ctx.Activities;
        var activity = activities.ShouldHaveSingleItem();

        activity.GetTagItem("db.query.parameters.num").ShouldBe(inNumber);
        activity.GetTagItem("db.query.parameters.guid").ShouldBe(inGuid);
        activity.GetTagItem("db.query.parameters.date").ShouldBe(inDate);
        activity.GetTagItem("db.query.parameters.dateTime").ShouldBe(inDateTime);
        activity.GetTagItem("db.query.parameters.string1").ShouldBe(inString1);
        activity.GetTagItem("db.query.parameters.string2").ShouldBe(AltinnNpgsqlTelemetry.RedactedPlaceholder);
    }

    [Fact]
    public async Task Parameters_Redaction_CanBeOverridden()
    {
        await using var ctx = await CreateBuilder();

        var inNumber = Random.Shared.Next();
        var inGuid = Guid.NewGuid();
        var inDate = new DateOnly(2020, Random.Shared.Next(1, 11), Random.Shared.Next(1, 25));
        var inDateTime = new DateTimeOffset(inDate, new TimeOnly(Random.Shared.Next(1, 20), Random.Shared.Next(1, 55)), TimeSpan.Zero);
        var inString1 = Guid.NewGuid().ToString();
        var inString2 = Guid.NewGuid().ToString();

        using (NpgsqlTelemetry.Configure(static opts =>
        {
            opts.SpanName = "test span name";
            opts.Summary = "test summary";
            opts.SetParameterFilter(typeof(Guid), NpgsqlTelemetryParameterFilterResult.RedactValue);
            opts.SetParameterFilter("num", NpgsqlTelemetryParameterFilterResult.RedactValue);
            opts.SetParameterFilter("string2", NpgsqlTelemetryParameterFilterResult.Include);
            opts.SetParameterFilter("string1", NpgsqlTelemetryParameterFilterResult.Ignore);
        }))
        {
            await using var reader = await ctx.Database.ExecuteReader(
                /*strpsql*/"""
                SELECT @num "num", @guid "guid", @date "date", @dateTime "dateTime", @string1 "string1", @string2 "string2"
                """,
                new NpgsqlParameter<int?>("num", NpgsqlDbType.Integer) { TypedValue = inNumber },
                new NpgsqlParameter<Guid?>("guid", NpgsqlDbType.Uuid) { TypedValue = inGuid },
                new NpgsqlParameter<DateOnly?>("date", NpgsqlDbType.Date) { TypedValue = inDate },
                new NpgsqlParameter<DateTimeOffset?>("dateTime", NpgsqlDbType.TimestampTz) { TypedValue = inDateTime },
                new NpgsqlParameter<string>("string1", NpgsqlDbType.Text) { TypedValue = inString1 },
                new NpgsqlParameter<string>("string2", NpgsqlDbType.Text) { TypedValue = inString2 });

            await Drain(reader);
        }

        var activities = ctx.Activities;
        var activity = activities.ShouldHaveSingleItem();

        activity.DisplayName.ShouldBe("test span name");
        activity.GetTagItem("db.query.summary").ShouldBe("test summary");
        activity.GetTagItem("db.query.parameters.num").ShouldBe(AltinnNpgsqlTelemetry.RedactedPlaceholder);
        activity.GetTagItem("db.query.parameters.guid").ShouldBe(AltinnNpgsqlTelemetry.RedactedPlaceholder);
        activity.GetTagItem("db.query.parameters.date").ShouldBe(inDate);
        activity.GetTagItem("db.query.parameters.dateTime").ShouldBe(inDateTime);
        activity.GetTagItem("db.query.parameters.string1").ShouldBeNull();
        activity.GetTagItem("db.query.parameters.string2").ShouldBe(inString2);
    }

    private static async Task Drain(NpgsqlDataReader reader)
    {
        do
        {
            while (await reader.ReadAsync(TestContext.Current.CancellationToken))
            {
                // No-op
            }
        }
        while (await reader.NextResultAsync(TestContext.Current.CancellationToken));
    }
}
