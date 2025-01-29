using Altinn.Authorization.Cli.Database.Metadata;
using Altinn.Authorization.Cli.Utils;
using CommunityToolkit.Diagnostics;
using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;

namespace Altinn.Authorization.Cli.Database;

[ExcludeFromCodeCoverage]
public sealed class CopyCommand(CancellationToken cancellationToken)
    : AsyncCommand<CopyCommand.Settings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        await using var source = await DbHelper.Create(settings.SourceConnectionString!, cancellationToken)
            .LogOnFailure("[bold red]Failed to connect to the source database[/]");

        await using var target = await DbHelper.Create(settings.TargetConnectionString!, cancellationToken)
            .LogOnFailure("[bold red]Failed to connect to the target database[/]");
        
        var schemaInfo = await source.GetSchemaInfo(settings.SchemaName!, cancellationToken)
            .LogOnFailure($"[bold red]Failed to get table graph for schema \"{settings.SchemaName}\"[/]");

        var graph = schemaInfo.Tables;

        var selectionPrompt = new MultiSelectionPrompt<SelectionItem>()
            .Title("Select what to copy")
            .NotRequired()
            .PageSize(20)
            .MoreChoicesText("[grey](Move up and down to reveal more items)[/]")
            .InstructionsText("[grey](Press [blue]<space>[/] to toggle an item, [green]<enter>[/] to accept)[/]");

        var tablesGroup = new GroupItem("Tables");
        var tableItems = graph.Select(t => new TableSelectionItem(t)).ToArray();
        selectionPrompt.AddChoiceGroup(
            tablesGroup,
            tableItems);

        var sequencesGroup = new GroupItem("Sequences");
        var sequenceItems = schemaInfo.Sequences.Select(s => new SequenceSelectionItem(s)).ToArray();
        selectionPrompt.AddChoiceGroup(
            sequencesGroup,
            sequenceItems);

        IEnumerable<SelectionItem> allItems = [tablesGroup, .. tableItems, sequencesGroup, .. sequenceItems];
        foreach (var sequenceItem in allItems)
        {
            selectionPrompt.Select(sequenceItem);
        }

        var selected = AnsiConsole.Prompt(selectionPrompt);
        
        // we need to keep the ordering from the graph
        var tables = graph.Where(i => selected.Any(s => s is TableSelectionItem ts && ts.Table == i)).ToList();
        var sequences = schemaInfo.Sequences.Where(i => selected.Any(s => s is SequenceSelectionItem ss && ss.Sequence == i)).ToList();

        await AnsiConsole.Progress()
            .AutoClear(false)
            .Columns([
                new TaskDescriptionColumn(),
                new ProgressBarColumn(),
                new PercentageColumn(),
                new ElapsedTimeColumn(),
            ])
            .StartAsync(async (ctx) =>
            {
                await target.BeginTransaction(cancellationToken);
                if (!settings.NoTruncate)
                {
                    var truncTask = ctx.AddTask("Truncating target tables", autoStart: true, maxValue: tables.Count);

                    foreach (var table in tables.AsEnumerable().Reverse())
                    {
                        await TruncateTable(target, table, cancellationToken)
                            .LogOnFailure($"[bold red]Failed to truncate table \"{table.Schema}\".\"{table.Name}\"[/]");
                        truncTask.Increment(1);
                        ctx.Refresh();
                    }

                    truncTask.Value = truncTask.MaxValue;
                    truncTask.StopTask();
                    ctx.Refresh();
                }

                var buffer = CharBuffers.MiB(10);
                foreach (var table in tables)
                {
                    if (table.EstimatedTableRows < 0)
                    {
                        // skip tables that are unallocated on the source
                        continue;
                    }

                    var copyTask = ctx.AddTask($"""Copy table "[yellow]{table.Name}[/]" """.TrimEnd(), autoStart: true, maxValue: table.EstimatedTableRows);
                    var colsString = string.Join(',', table.Columns.Select(c => $"\"{c.Name}\""));
                    var copyFrom = /*strpsql*/$"""COPY "{table.Schema}"."{table.Name}" ({colsString}) FROM STDIN""";
                    var copyTo = /*strpsql*/$"""COPY "{table.Schema}"."{table.Name}" ({colsString}) TO STDOUT""";

                    using var reader = await source.BeginTextExport(copyTo, cancellationToken);
                    await using var writer = await target.BeginTextImport(copyFrom, cancellationToken);

                    // TODO: this can probably be sped up quite a bit by reading and writing concurrently
                    int read;
                    while (true)
                    {
                        read = await reader.ReadAsync(buffer, cancellationToken);
                        if (read == 0)
                        {
                            // end
                            break;
                        }

                        var data = buffer[..read];
                        var newLines = data.Span.Count('\n');
                        copyTask.Increment(newLines);
                        await writer.WriteAsync(data, cancellationToken);
                    }

                    copyTask.Value = copyTask.MaxValue;
                    copyTask.StopTask();
                    ctx.Refresh();
                }

                if (sequences.Count > 0)
                {
                    var seqTask = ctx.AddTask("Updating target sequences", autoStart: true, maxValue: sequences.Count);
                    foreach (var seq in sequences)
                    {
                        await UpdateSequence(target, seq, cancellationToken)
                            .LogOnFailure($"[bold red]Failed to update sequence \"{seq.Schema}\".\"{seq.Name}\"[/]");
                        seqTask.Increment(1);
                        ctx.Refresh();
                    }

                    seqTask.Value = seqTask.MaxValue;
                    seqTask.StopTask();
                    ctx.Refresh();
                }

                await target.Commit(cancellationToken);
            });

        return 0;
    }

    private async Task TruncateTable(DbHelper db, TableRef table, CancellationToken cancellationToken)
    {
        await using var cmd = db.CreateCommand(/*strpsql*/$"""TRUNCATE "{table.Schema}"."{table.Name}" RESTART IDENTITY CASCADE""");
        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }

    private async Task UpdateSequence(DbHelper db, SequenceInfo seq, CancellationToken cancellationToken)
    {
        await using var cmd = db.CreateCommand(/*strpsql*/$"""SELECT setval('{seq.Schema}.{seq.Name}', {seq.Value}, {seq.IsCalled})""");
        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }

    [TypeConverterAttribute(typeof(Converter))]
    private abstract class SelectionItem
    {
        public abstract string GetMarkup();

        private sealed class Converter
            : TypeConverter
        {
            public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
                => sourceType.IsAssignableTo(typeof(SelectionItem));

            public override bool CanConvertTo(ITypeDescriptorContext? context, [NotNullWhen(true)] Type? destinationType)
                => destinationType == typeof(string);

            public override object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType)
                => value is SelectionItem item
                ? item.GetMarkup()
                : ThrowHelper.ThrowArgumentException<object?>("Invalid value type.");
        }
    }

    private sealed class GroupItem(string markup)
        : SelectionItem
    {
        public override string GetMarkup()
            => markup;
    }

    private sealed class TableSelectionItem(TableInfo table)
        : SelectionItem
    {
        public TableInfo Table => table;

        public override string GetMarkup()
        {
            var sb = new StringBuilder(table.Name);
            sb.Append(" [[");

            var first = true;
            foreach (var col in table.Columns)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    sb.Append(", ");
                }

                sb.Append("[yellow]").Append(col.Name).Append("[/]");
            }

            sb.Append("]]");

            return sb.ToString();
        }
    }

    private sealed class SequenceSelectionItem(SequenceInfo sequence)
        : SelectionItem
    {
        public SequenceInfo Sequence => sequence;

        public override string GetMarkup()
            => sequence.Name;
    }

    /// <summary>
    /// Settings for the export database command.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class Settings
        : CommandSettings
    {
        /// <summary>
        /// Gets the connection string to the source database.
        /// </summary>
        [Description("The connection string to the source database.")]
        [CommandArgument(0, "<SOURCE_CONNECTION_STRING>")]
        [ExpandEnvironmentVariables]
        public string? SourceConnectionString { get; init; }

        /// <summary>
        /// Gets the connection string to the target database.
        /// </summary>
        [Description("The connection string to the target database.")]
        [CommandArgument(1, "<TARGET_CONNECTION_STRING>")]
        [ExpandEnvironmentVariables]
        public string? TargetConnectionString { get; init; }

        /// <summary>
        /// Gets the schema name to copy.
        /// </summary>
        [Description("The schema name to copy.")]
        [CommandArgument(2, "<SCHEMA_NAME>")]
        public string? SchemaName { get; init; }

        /// <summary>
        /// Gets a value indicating whether to truncate the target tables before copying.
        /// </summary>
        [Description("Don't truncate the target tables before copying.")]
        [CommandOption("-n|--no-truncate")]
        public bool NoTruncate { get; init; }
    }
}
