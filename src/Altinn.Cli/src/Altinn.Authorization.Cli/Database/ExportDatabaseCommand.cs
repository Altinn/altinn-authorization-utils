using Npgsql;
using Spectre.Console;
using Spectre.Console.Cli;
using System.Collections.Immutable;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Altinn.Authorization.Cli.Database;

/// <summary>
/// Exports all tables in a database schema.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed class ExportDatabaseCommand(CancellationToken cancellationToken)
    : AsyncCommand<ExportDatabaseSettings>
{
    /// <inheritdoc/>
    public override async Task<int> ExecuteAsync(CommandContext context, ExportDatabaseSettings settings)
    {
        await using var dataSource = CreateDataSource(settings);
        await using var conn = await dataSource.OpenConnectionAsync(cancellationToken);

        var tables = await GetTablesWithDeps(conn, settings.SchemaName!, cancellationToken);

        var selectionPrompt = new MultiSelectionPrompt<TableInfo>()
            .Title("Select [green]tables[/] to export")
            .NotRequired()
            .PageSize(10)
            .MoreChoicesText("[grey](Move up and down to reveal more tables)[/]")
            .InstructionsText("[grey](Press [blue]<space>[/] to toggle a table, [green]<enter>[/] to accept)[/]")
            .UseConverter(static table =>
            {
                var sb = new StringBuilder(table.Name);
                sb.Append(" [[");

                var first = true;
                foreach (var col in table.Cols)
                {
                    if (first)
                    {
                        first = false;
                    }
                    else
                    {
                        sb.Append(", ");
                    }

                    sb.Append("[yellow]").Append(col).Append("[/]");
                }

                sb.Append("]]");

                if (table.Deps.Length == 0)
                {
                    return sb.ToString();
                }

                sb.Append(" [grey]<-[/] ");
                first = true;
                foreach (var dep in table.Deps)
                {
                    if (first)
                    {
                        first = false;
                    }
                    else
                    {
                        sb.Append(", ");
                    }
                    sb.Append("[grey]").Append(dep.Name).Append("[/]");
                }

                return sb.ToString();
            });

        foreach (var table in tables)
        {
            selectionPrompt.AddChoices(table, i => i.Select());
        }

        var selected = AnsiConsole.Prompt(selectionPrompt).Select(static t => t.Name).ToHashSet();

        await AnsiConsole.Progress()
            .AutoClear(false)
            .Columns([
                new TaskDescriptionColumn(),
                new ProgressBarColumn(),
                new PercentageColumn(),
                new RemainingTimeColumn(),
                new TransferSpeedColumn(),
            ])
            .StartAsync(async (ctx) =>
            {
                var index = 0;
                List<ExportTask> tasks = new(selected.Count);

                await using var tx = await conn.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
                foreach (var table in tables.Where(t => selected.Contains(t.Name)))
                {
                    if (table.EstimatedRows < 0)
                    {
                        continue;
                    }

                    var task = CreateExportTask(conn, table, index++, ctx);
                    tasks.Add(task);
                }

                await tx.RollbackAsync(cancellationToken);
                
                foreach (var task in tasks)
                {
                    var query = new StringBuilder();
                    query.Append(/*strpsql*/$"""COPY "{task.Table.Schema}"."{task.Table.Name}" (""");
                    var first = true;
                    foreach (var col in task.Table.Cols)
                    {
                        if (first)
                        {
                            first = false;
                        }
                        else
                        {
                            query.Append(", ");
                        }
                        query.Append($"\"{col}\"");
                    }
                    query.Append(/*strpsql*/""") TO STDIN""");


                    task.Progress.StartTask();
                    using var reader = await conn.BeginTextExportAsync(query.ToString(), cancellationToken);

                    query.Remove(query.Length - "TO STDIN".Length, "TO STDIN".Length);
                    query.Append(/*strpsql*/$"""FROM STDIN""");

                    await using var writer = File.CreateText(Path.Combine(settings.OutputDirectory!.FullName, $"{task.Index:D2}-{task.Table.Name}.asdn-v1.tsv"));
                    await writer.WriteLineAsync(query);

                    string? line;
                    uint rows = 0;

                    while((line = await reader.ReadLineAsync(cancellationToken)) is not null)
                    {
                        await writer.WriteLineAsync(line.AsMemory(), cancellationToken);

                        if (++rows % 5000 == 0)
                        {
                            task.Progress.Increment(5000);
                            rows = 0;
                        }
                    }

                    task.Progress.Value = task.Progress.MaxValue;
                    task.Progress.StopTask();
                    ctx.Refresh();
                }

                ctx.Refresh();
            });
        await Task.Yield();
        return 0;
    }

    /// <inheritdoc/>
    public override ValidationResult Validate(CommandContext context, ExportDatabaseSettings settings)
    {
        if (string.IsNullOrEmpty(settings.ConnectionString))
        {
            return ValidationResult.Error($"""The "connection-string" option is required.""");
        }

        return base.Validate(context, settings);
    }

    private NpgsqlDataSource CreateDataSource(ExportDatabaseSettings settings)
    {
        var connStrBuilder = new NpgsqlConnectionStringBuilder(settings.ConnectionString);
        connStrBuilder.Pooling = false;

        return NpgsqlDataSource.Create(connStrBuilder);
    }

    private ExportTask CreateExportTask(NpgsqlConnection conn, TableInfo table, int index, ProgressContext ctx)
    {
        var progressTask = ctx.AddTask($"export {table.Name}", autoStart: false, maxValue: table.EstimatedRows);

        return new ExportTask(progressTask, table, index);
    }

    private async Task<ImmutableArray<TableInfo>> GetTablesWithDeps(NpgsqlConnection conn, string schemaName, CancellationToken cancellationToken)
    {
        const string QUERY =
            /*strpsql*/"""
            WITH "tables" AS (
            	SELECT t.table_name
            	FROM information_schema."tables" t
            	WHERE t.table_schema = @schema
            ), "table_deps" AS (
            	SELECT DISTINCT
            		kcu.table_name "from",
            		rel_tco.table_name "to"
            	FROM
            		information_schema.table_constraints tco
            	JOIN information_schema.key_column_usage kcu
            		ON tco.constraint_schema = kcu.constraint_schema
            		AND tco.constraint_name = kcu.constraint_name
            	JOIN information_schema.referential_constraints rco 
            		ON tco.constraint_schema = rco.constraint_schema
            		AND tco.constraint_name = rco.constraint_name
            	JOIN information_schema.table_constraints rel_tco 
            		ON rco.unique_constraint_schema = rel_tco.constraint_schema 
            		AND rco.unique_constraint_name = rel_tco.constraint_name
            	WHERE
            			tco.constraint_type = 'FOREIGN KEY'
            		AND kcu.table_schema = @schema
            		AND rel_tco.table_schema = @schema
            ), "est_rows" AS (
                SELECT
                    relname "table_name",
                    relnamespace::regnamespace::text "schema",
                    reltuples::bigint estimated_rows
                FROM pg_catalog.pg_class
            ), "columns" AS (
                SELECT
                    c.table_name,
                    c.column_name,
                    c.ordinal_position
                FROM information_schema.columns c
                WHERE c.table_schema = @schema
            )
            SELECT 
                t.table_name "name",
                er.estimated_rows "est_rows",
                array_agg(c.column_name ORDER BY c.ordinal_position) "columns",
                array_remove(array_agg(DISTINCT td."to"), NULL) "deps"
            FROM "tables" t
            JOIN "est_rows" er ON t.table_name = er.table_name and er."schema" = @schema
            LEFT JOIN "table_deps" td ON td."from" = t.table_name
            LEFT JOIN "columns" c ON c.table_name = t.table_name
            GROUP BY t.table_name, er.estimated_rows
            """;

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = QUERY;

        cmd.Parameters.AddWithValue("schema", schemaName);

        await cmd.PrepareAsync(cancellationToken);
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);

        var results = new List<(string Name, long EstimatedRows, string[] Columns, string[] Deps)>();
        while (await reader.ReadAsync(cancellationToken))
        {
            var name = reader.GetString("name");
            var rows = reader.GetInt64("est_rows");
            var cols = reader.GetFieldValue<string[]>("columns");
            var deps = reader.GetFieldValue<string[]>("deps");
            results.Add((name, rows, cols, deps));
        }

        // sort by deps, fewest first
        results.Sort((a, b) => a.Deps.Length.CompareTo(b.Deps.Length));

        var lookup = new Dictionary<string, TableInfo>();
        var builder = ImmutableArray.CreateBuilder<TableInfo>(results.Count);
        while (builder.Count < results.Count)
        {
            var added = false;
            foreach (var (table, estRows, cols, deps) in results)
            {
                if (lookup.ContainsKey(table))
                {
                    continue;
                }

                if (deps.All(d => lookup.ContainsKey(d)))
                {
                    var info = new TableInfo(schemaName, table, estRows, cols.ToImmutableArray(), deps.Select(d => lookup[d]).ToImmutableArray());
                    builder.Add(info);
                    lookup[table] = info;
                    added = true;
                }
            }

            if (!added)
            {
                throw new InvalidOperationException("Failed to build table graph");
            }
        }

        return builder.MoveToImmutable();
    }

    private sealed record TableInfo(string Schema, string Name, long EstimatedRows, ImmutableArray<string> Cols, ImmutableArray<TableInfo> Deps);
    private sealed record ExportTask(ProgressTask Progress, TableInfo Table, int Index);
}

/// <summary>
/// Settings for the export database command.
/// </summary>
[ExcludeFromCodeCoverage]
public class ExportDatabaseSettings
    : DatabaseSettings
{
    /// <summary>
    /// Gets the schema name to export.
    /// </summary>
    [CommandArgument(0, "<SCHEMA_NAME>")]
    public string? SchemaName { get; init; }

    /// <summary>
    /// Gets the output directory to write the exported tables to.
    /// </summary>
    [CommandArgument(1, "<OUTPUT_DIRECTORY>")]
    public DirectoryInfo? OutputDirectory { get; init; }
}
