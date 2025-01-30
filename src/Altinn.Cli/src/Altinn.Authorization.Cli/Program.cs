using Altinn.Authorization.Cli.Database;
using Spectre.Console;
using Spectre.Console.Cli;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

using var cancellationTokenSource = new CancellationTokenSource();
using var sigInt = PosixSignalRegistration.Create(PosixSignal.SIGINT, onSignal);
using var sigTerm = PosixSignalRegistration.Create(PosixSignal.SIGTERM, onSignal);
using var sigQuit = PosixSignalRegistration.Create(PosixSignal.SIGQUIT, onSignal);

var app = new CommandApp();

app.Configure(config =>
{
    config.PropagateExceptions();
    config.Settings.Registrar.RegisterInstance(cancellationTokenSource.Token);
    config.AddBranch("db", db =>
    {
        db.AddCommand<ExportDatabaseCommand>("export");
        db.AddCommand<CopyCommand>("cp");
    });
});

try
{
    return await app.RunAsync(args);
}
catch (OperationCanceledException oce) when (oce.CancellationToken == cancellationTokenSource.Token)
{
    AnsiConsole.MarkupLine("[red]The operation was cancelled.[/]");
    return -1;
}
catch (Exception ex)
{
    AnsiConsole.WriteException(ex);
    return 1;
}

void onSignal(PosixSignalContext ctx)
{
    ctx.Cancel = true;
    cancellationTokenSource.Cancel();
}

/// <summary>
/// Program entry point.
/// </summary>
[ExcludeFromCodeCoverage]
public partial class Program
{
}
