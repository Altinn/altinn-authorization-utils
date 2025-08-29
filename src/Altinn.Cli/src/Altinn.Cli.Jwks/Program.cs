using Altinn.Cli.Jwks.Commands;
using Altinn.Cli.Jwks.Console;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Spectre.Console;
using System.CommandLine;
using System.CommandLine.Help;
using System.Diagnostics.CodeAnalysis;

namespace Altinn.Cli.Jwks;

[ExcludeFromCodeCoverage]
static class Program
{
    static async Task<int> Main(string[] args)
    {
        var builder = Host.CreateEmptyApplicationBuilder(new()
        {
            DisableDefaults = true,
            Args = null, // do not pass args here, as it will be used by the Host to configure itself.
            EnvironmentName = "cli",
            ApplicationName = "altinn-jwks",
            ContentRootPath = null,
        });

        var consoleService = new ConsoleService(args);
        ConfigureServices(builder, consoleService);

        builder.Services.AddSingleton<Option>(BaseCommand.StoreOption);
        builder.Services.AddSingleton<Command, CreateCommand>();
        builder.Services.AddSingleton<Command, ExportCommand>();
        builder.Services.AddSingleton<Command, ListCommand>();

        builder.Services.AddKeyedSingleton<Command, ExportKeyCommand>(serviceKey: typeof(ExportCommand));

        builder.Services.AddSingleton(s =>
        {
            var rootCommand = new RootCommand("Console app for creating Json Web Keys")
                .AddOptionMiddleware<HelpOption>(x => new CustomHelpRenderer((HelpAction)x.Action!));

            rootCommand.TreatUnmatchedTokensAsErrors = true;

            foreach (var option in s.GetRequiredService<IEnumerable<Option>>())
            {
                rootCommand.Options.Add(option);
            }

            foreach (var command in s.GetRequiredService<IEnumerable<Command>>())
            {
                rootCommand.Subcommands.Add(command);
            }

            return rootCommand;
        });

        var host = builder.Build();
        var console = host.Services.GetRequiredService<IConsole>();
        try
        {
            await host.RunAsync();
            return consoleService.GetResult();
        }
        catch (OperationCanceledException e)
        {
            // TODO: write different error.
            console.StdErr.WriteException(e);
            return -1;
        }
        catch (Exception e)
        {
            console.StdErr.WriteException(e);
            return 1;
        }
        finally
        {
            if (host is IAsyncDisposable iad)
            {
                await iad.DisposeAsync();
            }
            else
            {
                host.Dispose();
            }
        }
    }

    static void ConfigureServices(HostApplicationBuilder builder, ConsoleService result)
    {
        var services = builder.Services;

        services.AddSingleton<IConsole, Console.Console>();
        services.AddSingleton(s => s.GetRequiredService<IConsole>().StdOut);
        services.AddKeyedSingleton(ConsoleTarget.StdOut, (s, _) => s.GetRequiredService<IConsole>().StdOut);
        services.AddKeyedSingleton(ConsoleTarget.StdErr, (s, _) => s.GetRequiredService<IConsole>().StdErr);

        services.AddSingleton(result);
        services.AddHostedService<ConsoleHostedService>();

        services.AddOptions<InvocationConfiguration>()
            .PostConfigure((InvocationConfiguration config, IConsole console) =>
            {
                config.Output = new AnsiWriter(console.StdOut);
                config.Error = new AnsiWriter(console.StdErr);
            });
    }
}
