using System.CommandLine;
using System.Diagnostics.CodeAnalysis;
using Altinn.Authorization.CommandLine.Arguments;
using Altinn.Authorization.CommandLine.Parsing;
using Microsoft.Extensions.DependencyInjection;
using Altinn.Authorization.CommandLine.Factory;
using Altinn.Authorization.CommandLine.Help;
using Microsoft.Extensions.Primitives;
using Spectre.Console;

namespace Altinn.Authorization.CommandLine.Tests.Factory;

public class EnvironmentDefaultTests
{
    private const string Variable = "COMMANDLINE_TEST_ENV_DEFAULT";

    [Fact]
    public void EnvironmentProvider_UsesSuppliedHelpColor()
    {
        using var services = new ServiceCollection().AddOptions().AddSingleton<ValueParser>().BuildServiceProvider();
        var environment = new EnvironmentValueDefaultValueProvider<int>("PROVIDER_TEST_VARIABLE", "42", services.GetRequiredService<ValueParser>());
        var context = new HelpDisplayArgumentContext(Color.Green);

        environment.GetHelpText(context).Single(segment => segment.Text == "PROVIDER_TEST_VARIABLE")
            .Style!.Value.Foreground.ShouldBe(Color.Green);
    }

    [Theory]
    [InlineData(null, true)]
    [InlineData("one two", false)]
    public async Task Option_InfersRequirednessFromWrappedProvider(string? environment, bool required)
    {
        using var scope = new EnvironmentScope(environment);
        await using var app = CliApplication.CreateBuilder("Environment defaults").Build();
        var option = new OptionAttribute("--values") { DefaultFromEnvironment = Variable }.CreateOption(
            app.ApplicationServices, typeof(StringValues), "values", null);

        option.Required.ShouldBe(required);
    }

    [Theory]
    [InlineData("one\n\ntwo\n", null, 0)]
    [InlineData("one two", " \t\n", 1)]
    [InlineData("one two", "", 1)]
    [InlineData(" \t\n", null, 1)]
    public async Task EnvironmentDefault_DoesNotRelaxStringValuesParsing(string environment, string? explicitValue, int expectedCode)
    {
        using var scope = new EnvironmentScope(environment);
        StringValues? recorded = null;
        await using var app = CliApplication.CreateBuilder("Environment defaults").Build();
        app.AddCommand("run", "Run", ([Option("--values", DefaultFromEnvironment = Variable)] StringValues values) => { recorded = values; });

        string[] args = explicitValue is null ? ["run"] : ["run", "--values", explicitValue];
        (await app.RunAsync(args, TestContext.Current.CancellationToken)).ShouldBe(expectedCode);
        if (expectedCode == 0)
        {
            recorded.ShouldBe(new StringValues(["one", "two"]));
        }
        else
        {
            recorded.ShouldBeNull();
        }
    }

    [Fact]
    public async Task EnvironmentDefault_CapturesValueDuringResolution()
    {
        using var scope = new EnvironmentScope("42");
        await using var app = CliApplication.CreateBuilder("Environment defaults").Build();
        var option = (Option<int>)new OptionAttribute("--number") { DefaultFromEnvironment = Variable }
            .CreateOption(app.ApplicationServices, typeof(int), "number", null);
        var command = new Command("run") { option };

        Environment.SetEnvironmentVariable(Variable, "43");
        command.Parse([]).GetValue(option).ShouldBe(42);
        command.Parse(["--number", "44"]).GetValue(option).ShouldBe(44);
    }

    [Fact]
    public async Task Argument_ParsesCapturedEnvironmentLazily()
    {
        await using var app = CliApplication.CreateBuilder("Environment defaults").Build();
        var argument = app.ApplicationServices.GetRequiredService<ArgumentFactory>().Create<int>("number", null,
            new EnvironmentValueDefaultValueProvider<int>(Variable, "42", app.ApplicationServices.GetRequiredService<ValueParser>()));
        var command = new Command("run") { argument };

        command.Parse([]).GetValue(argument).ShouldBe(42);
        command.Parse(["43"]).GetValue(argument).ShouldBe(43);
    }

    [Fact]
    public async Task RequiredOption_ExplicitValueWorksWithoutEnvironment()
    {
        using var scope = new EnvironmentScope(null);
        int? recorded = null;
        await using var app = CliApplication.CreateBuilder("Environment defaults").Build();
        app.AddCommand("run", "Run", ([Option("--value", DefaultFromEnvironment = Variable)] int value) => { recorded = value; });

        (await app.RunAsync(["run", "--value", "42"], TestContext.Current.CancellationToken)).ShouldBe(0);
        recorded.ShouldBe(42);
    }

    [Theory]
    [InlineData("true", true)]
    [InlineData("false", false)]
    public async Task BooleanOption_ParsesEnvironment(string environment, bool expected)
    {
        using var scope = new EnvironmentScope(environment);
        await using var app = CliApplication.CreateBuilder("Environment defaults").Build();
        var option = (Option<bool>)new OptionAttribute("--enabled") { DefaultFromEnvironment = Variable }
            .CreateOption(app.ApplicationServices, typeof(bool), "enabled", DefaultValueProvider.FromValue(false));
        var command = new Command("run");
        command.Options.Add(option);

        command.Parse("").GetValue(option).ShouldBe(expected);
    }

    [Fact]
    public async Task RequiredStringValues_RejectsWhitespaceEnvironment()
    {
        using var scope = new EnvironmentScope(" \t ");
        var invoked = false;
        await using var app = CliApplication.CreateBuilder("Environment defaults").Build();
        app.AddCommand("run", "Run", ([Option("--value", DefaultFromEnvironment = Variable)] StringValues values) => { invoked = true; });

        (await app.RunAsync(["run"], TestContext.Current.CancellationToken)).ShouldNotBe(0);
        invoked.ShouldBeFalse();
    }

    [Theory]
    [InlineData("environment", null, "environment")]
    [InlineData("environment", "explicit", "explicit")]
    [InlineData(null, null, "fallback")]
    [InlineData("", null, "fallback")]
    [InlineData(" \t\n", null, "fallback")]
    [InlineData("-value with spaces", null, "-value with spaces")]
    public async Task Option_PrefersExplicitThenEnvironmentThenParameterDefault(string? environment, string? supplied, string expected)
    {
        using var scope = new EnvironmentScope(environment);
        string? recorded = null;
        await using var app = CliApplication.CreateBuilder("Environment defaults").Build();
        app.AddCommand("run", "Run", ([Option("--value", DefaultFromEnvironment = Variable)] string value = "fallback") => { recorded = value; });
        (await app.RunAsync(supplied is null ? ["run"] : ["run", "--value", supplied], TestContext.Current.CancellationToken)).ShouldBe(0);
        recorded.ShouldBe(expected);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("invalid")]
    public async Task RequiredOption_RejectsMissingOrInvalidEnvironment(string? value)
    {
        using var scope = new EnvironmentScope(value);
        var invoked = false;
        await using var app = CliApplication.CreateBuilder("Environment defaults").Build();
        app.AddCommand("run", "Run", ([Option("--value", DefaultFromEnvironment = Variable)] int number) => { invoked = true; });
        (await app.RunAsync(["run"], TestContext.Current.CancellationToken)).ShouldNotBe(0);
        invoked.ShouldBeFalse();
    }

    [Fact]
    public async Task RequiredOption_ParsesEnvironmentUsingConfiguredParser()
    {
        using var scope = new EnvironmentScope("one two");
        StringValues recorded = default;
        await using var app = CliApplication.CreateBuilder("Environment defaults").Build();
        app.AddCommand("run", "Run", ([Option("--value", DefaultFromEnvironment = Variable)] StringValues values) => { recorded = values; });
        (await app.RunAsync(["run"], TestContext.Current.CancellationToken)).ShouldBe(0);
        recorded.ShouldBe(new StringValues(["one", "two"]));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" \t\n")]
    [InlineData("42")]
    [InlineData("not-an-integer")]
    public async Task Help_ShowsVariableNameWithoutParsingItsValue(string? environment)
    {
        using var scope = new EnvironmentScope(environment);
        await using var app = CliApplication.CreateBuilder("Environment defaults").Build();
        var option = (Option<int>)new OptionAttribute("--value") { DefaultFromEnvironment = Variable }
            .CreateOption(app.ApplicationServices, typeof(int), "value", null);
        var command = new Command("run");
        command.Options.Add(option);
        var help = RenderHelp(command);
        help.ShouldContain($"${Variable}");
        help.ShouldNotContain("not-an-integer");
        option.Required.ShouldBe(string.IsNullOrWhiteSpace(environment));
        if (option.Required)
        {
            command.Parse([]).Errors.ShouldNotBeEmpty();
        }
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" \t\n")]
    [InlineData("second")]
    [InlineData("invalid")]
    public async Task EnumHelp_ShowsOnlyVariableWithoutParsingEnvironment(string? environment)
    {
        using var scope = new EnvironmentScope(environment);
        await using var app = CliApplication.CreateBuilder("Environment defaults").Build();
        var option = (Option<Mode>)new OptionAttribute("--mode") { DefaultFromEnvironment = Variable }
            .CreateOption(app.ApplicationServices, typeof(Mode), "mode", DefaultValueProvider.FromValue(Mode.First));
        var help = RenderHelp(new Command("run") { option });

        help.ShouldContain($"[default: ${Variable}]");
        help.ShouldNotContain("fallback:");
        help.ShouldNotContain("default: second");
        help.ShouldNotContain("invalid");
        option.Required.ShouldBeFalse();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("second")]
    [InlineData("invalid")]
    public async Task EnumHelpWithoutFallback_ShowsVariable(string? environment)
    {
        using var scope = new EnvironmentScope(environment);
        await using var app = CliApplication.CreateBuilder("Environment defaults").Build();
        var option = (Option<Mode>)new OptionAttribute("--mode") { DefaultFromEnvironment = Variable }
            .CreateOption(app.ApplicationServices, typeof(Mode), "mode", null);

        RenderHelp(new Command("run") { option }).ShouldContain($"${Variable}");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task HelpRendering_DoesNotInvokeConfiguredParser(bool annotatedEnum)
    {
        using var scope = new EnvironmentScope("invalid");
        var parser = new RejectingParser();
        var builder = CliApplication.CreateBuilder("Environment defaults");
        builder.Services.Configure<ParsingOptions>(options => options.AdditionalParsers.Add(parser));
        await using var app = builder.Build();
        var type = annotatedEnum ? typeof(Mode) : typeof(int);
        var option = new OptionAttribute("--value") { DefaultFromEnvironment = Variable }
            .CreateOption(app.ApplicationServices, type, "value", null);

        var callsBeforeRendering = 0;
        RenderHelp(new Command("run") { option }, () => callsBeforeRendering = parser.Calls).ShouldContain($"${Variable}");
        parser.Calls.ShouldBe(callsBeforeRendering);
    }

    private sealed class RejectingParser : IValueParser
    {
        public int Calls { get; private set; }

        public bool CanParse(Type type) => true;

        public bool TryParse<T>(ValueParsingContext context, out T value, [NotNullWhen(false)] out string? error)
        {
            Calls++;
            value = default!;
            error = "Invalid environment value.";
            return false;
        }
    }

    private static string RenderHelp(Command command, Action? beforeRender = null)
    {
        using var writer = new StringWriter();
        var console = AnsiConsole.Create(new AnsiConsoleSettings
        {
            Out = new AnsiConsoleOutput(writer),
            Ansi = AnsiSupport.No,
            ColorSystem = ColorSystemSupport.NoColors,
            Enrichment = new() { UseDefaultEnrichers = false },
        });
        console.Profile.Width = 200;
        var result = command.Parse("--help");
        beforeRender?.Invoke();
        console.Write(new DefaultHelpBuilder().Build(new HelpContext(new DefaultHelpBuilder(), result.CommandResult, console.Profile)));
        return writer.ToString();
    }

    [ArgumentEnum]
    private enum Mode
    {
        [ArgumentValues("first")]
        First,
        [ArgumentValues("second")]
        Second,
    }

    private sealed class EnvironmentScope
        : IDisposable
    {
        private readonly string? _previous = Environment.GetEnvironmentVariable(Variable);
        public EnvironmentScope(string? value) => Environment.SetEnvironmentVariable(Variable, value);
        public void Dispose() => Environment.SetEnvironmentVariable(Variable, _previous);
    }
}
