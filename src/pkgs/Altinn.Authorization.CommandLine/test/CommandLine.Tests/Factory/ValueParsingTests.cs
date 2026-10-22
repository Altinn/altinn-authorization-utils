using System.CommandLine;
using Altinn.Authorization.CommandLine.Help;
using Spectre.Console;
using System.Diagnostics.CodeAnalysis;
using Altinn.Authorization.CommandLine.Arguments;
using Altinn.Authorization.CommandLine.Factory;
using Altinn.Authorization.CommandLine.Parsing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using Microsoft.Extensions.Options;

namespace Altinn.Authorization.CommandLine.Tests.Factory;

public class ValueParsingTests
{
    private const string Variable = "COMMANDLINE_SHARED_PARSING_TEST";

    [Fact]
    public async Task Scalars_UseTheSameConversionForArgumentsOptionsAndEnvironment()
    {
        await AssertParity("42", 42);
        await AssertParity("parsed:42", new ParsedNumber(42));
        await AssertParity<int?>("42", 42);
        await AssertParity("false", false);
        await AssertParity("value with spaces", "value with spaces");
        await AssertParity("", "", useEnvironment: false);
        await AssertParity("one\n\ntwo\n", new StringValues(["one", "two"]));
        await AssertParity("second", SampleEnum.Second);
        await AssertParity("alias", AnnotatedEnum.Value);
        await AssertParity("2026-10-21", new DateOnly(2026, 10, 21));
        await AssertParity("7c72f77c-4640-4c27-9800-4b38f74df315", Guid.Parse("7c72f77c-4640-4c27-9800-4b38f74df315"));
    }

    [Fact]
    public async Task Collections_UseTheSameElementParserForBothSources()
    {
        await AssertParity("42", new[] { 42 });
        await AssertParity("42", new List<int> { 42 });
        await AssertParity<IReadOnlyList<int>>("42", new[] { 42 });
        await AssertParity("alias", new[] { AnnotatedEnum.Value });
    }

    [Fact]
    public async Task Paths_UseTheSameConversionForBothSources()
    {
        var path = Path.GetFullPath("test-path");
        await using var app = CliApplication.CreateBuilder("Shared parsing").Build();
        foreach (var fromEnvironment in new[] { false, true })
        {
            using var scope = new EnvironmentScope(path);
            var option = (Option<FileInfo>)new OptionAttribute("--value")
            {
                DefaultFromEnvironment = fromEnvironment ? Variable : null,
            }.CreateOption(app.ApplicationServices, typeof(FileInfo), "value", null);
            var result = new Command("run") { option }.Parse(fromEnvironment ? [] : ["--value", path]);
            result.Errors.ShouldBeEmpty();
            result.GetValue(option)!.FullName.ShouldBe(path);
        }
    }

    [Theory]
    [InlineData(typeof(int), "invalid")]
    [InlineData(typeof(int?), "invalid")]
    [InlineData(typeof(int[]), "invalid")]
    [InlineData(typeof(AnnotatedEnum), "unknown")]
    public async Task InvalidValues_ProduceTheSameDiagnostic(Type type, string text)
    {
        using var scope = new EnvironmentScope(text);
        await using var app = CliApplication.CreateBuilder("Shared parsing").Build();
        var option = new OptionAttribute("--value") { DefaultFromEnvironment = Variable }
            .CreateOption(app.ApplicationServices, type, "value", null);
        var command = new Command("run") { option };

        var explicitError = command.Parse(["--value", text]).Errors.Single().Message;
        command.Parse([]).Errors.Single().Message.ShouldBe($"Invalid {Variable} default for --value: {explicitError}");
        var argument = app.ApplicationServices.GetRequiredService<ArgumentFactory>().Create(type, "value", null, null);
        new Command("run") { argument }.Parse([text]).Errors.Single().Message.ShouldBe(explicitError);
    }

    [Fact]
    public async Task CustomDiParser_IsSharedAndEnvironmentConversionIsLazy()
    {
        using var scope = new EnvironmentScope("invalid");
        var parser = new CountingParser();
        var builder = CliApplication.CreateBuilder("Shared parsing");
        builder.Services.Configure<ParsingOptions>(options => options.AdditionalParsers.Add(parser));
        await using var app = builder.Build();
        var option = (Option<int>)new OptionAttribute("--value") { DefaultFromEnvironment = Variable }
            .CreateOption(app.ApplicationServices, typeof(int), "value", null);
        var command = new Command("run") { option };

        parser.Calls.ShouldBe(0);
        option.HelpCustomization.DefaultValueProvider!.GetHelpText(new HelpDisplayArgumentContext(Color.Green)).ShouldNotBeEmpty();
        parser.Calls.ShouldBe(0);
        command.Parse(["--value", "5"]).GetValue(option).ShouldBe(10);
        parser.Calls.ShouldBe(1);
        command.Parse([]).Errors.Single().Message.ShouldContain("custom failure");
        parser.Calls.ShouldBe(2);

        var argument = app.ApplicationServices.GetRequiredService<ArgumentFactory>().Create<int>("value", null, null);
        new Command("run") { argument }.Parse(["6"]).GetValue(argument).ShouldBe(12);
        parser.Calls.ShouldBe(3);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" \t\n")]
    public async Task BlankEnvironment_RetainsThePreviousProviderWithoutParsing(string? text)
    {
        using var scope = new EnvironmentScope(text);
        var parser = new CountingParser();
        var builder = CliApplication.CreateBuilder("Shared parsing");
        builder.Services.Configure<ParsingOptions>(options => options.AdditionalParsers.Add(parser));
        await using var app = builder.Build();
        var option = (Option<int>)new OptionAttribute("--value") { DefaultFromEnvironment = Variable }
            .CreateOption(app.ApplicationServices, typeof(int), "value", DefaultValueProvider.FromValue(42));

        option.Required.ShouldBeFalse();
        new Command("run") { option }.Parse([]).GetValue(option).ShouldBe(42);
        parser.Calls.ShouldBe(0);
    }

    [Fact]
    public async Task BareBooleanAndRepeatedCollectionOptions_PreserveSyntax()
    {
        await using var app = CliApplication.CreateBuilder("Shared parsing").Build();
        var factory = app.ApplicationServices.GetRequiredService<OptionFactory>();
        var enabled = factory.Create<bool>("--enabled", [], null, false, null);
        var values = factory.Create<int[]>("--values", [], null, false, null);
        var result = new Command("run") { enabled, values }.Parse(["--enabled", "--values", "1", "--values", "2"]);
        result.Errors.ShouldBeEmpty();
        result.GetValue(enabled).ShouldBeTrue();
        result.GetValue(values).ShouldBe([1, 2]);
    }

    [Fact]
    public async Task Delegation_ContinuesForTheSameTypeAndRestartsForInnerTypes()
    {
        using var scope = new EnvironmentScope("prefix:5");
        var counter = new CountingParser();
        var builder = CliApplication.CreateBuilder("Delegation");
        builder.Services.Configure<ParsingOptions>(options =>
        {
            options.AdditionalParsers.Add(new PrefixParser());
            options.AdditionalParsers.Add(counter);
        });
        await using var app = builder.Build();

        Check<int?>(10);
        Check<int[]>([10]);
        counter.Calls.ShouldBe(4);

        void Check<T>(T expected)
        {
            var option = (Option<T>)new OptionAttribute("--value") { DefaultFromEnvironment = Variable }
                .CreateOption(app.ApplicationServices, typeof(T), "value", null);
            var command = new Command("run") { option };
            var explicitResult = command.Parse(["--value", "prefix:5"]);
            explicitResult.Errors.ShouldBeEmpty();
            explicitResult.GetValue(option).ShouldBe(expected);
            var environmentResult = command.Parse([]);
            environmentResult.Errors.ShouldBeEmpty();
            environmentResult.GetValue(option).ShouldBe(expected);
        }
    }

    [Fact]
    public async Task EntryPoint_ReadsOptionsUpdatesAndDelegationKeepsItsSnapshot()
    {
        using var scope = new EnvironmentScope("5");
        var monitor = new ParsingOptionsMonitor();
        monitor.CurrentValue.AdditionalParsers.Add(new UpdateOptionsParser(monitor));
        monitor.CurrentValue.AdditionalParsers.Add(new CountingParser());
        var builder = CliApplication.CreateBuilder("Options updates");
        builder.Services.AddSingleton<IOptionsMonitor<ParsingOptions>>(monitor);
        await using var app = builder.Build();
        var option = (Option<int[]>)new OptionAttribute("--value") { DefaultFromEnvironment = Variable }
            .CreateOption(app.ApplicationServices, typeof(int[]), "value", null);
        var command = new Command("run") { option };

        // Delegated elements keep the original options even when the monitor changes.
        var first = command.Parse([]);
        first.Errors.ShouldBeEmpty();
        first.GetValue(option).ShouldBe([10]);
        command.Parse([]).Errors.Single().Message.ShouldContain("updated parser failure");
    }

    private sealed class PrefixParser
        : IValueParser
    {
        public bool CanParse(Type type) => type == typeof(int);

        public bool TryParse<T>(ValueParsingContext context, out T value, [NotNullWhen(false)] out string? error)
        {
            var text = context.Values.Single();
            return context.TryParse<T>([text.StartsWith("prefix:", StringComparison.Ordinal) ? text[7..] : text], out value, out error);
        }
    }

    private sealed class UpdateOptionsParser(ParsingOptionsMonitor monitor)
        : IValueParser
    {
        public bool CanParse(Type type) => type == typeof(int[]);

        public bool TryParse<T>(ValueParsingContext context, out T value, [NotNullWhen(false)] out string? error)
        {
            monitor.CurrentValue = new ParsingOptions();
            monitor.CurrentValue.AdditionalParsers.Add(new RejectingParser());
            return context.TryParse(out value, out error);
        }
    }

    private sealed class RejectingParser
        : IValueParser
    {
        public bool CanParse(Type type) => true;

        public bool TryParse<T>(ValueParsingContext context, out T value, [NotNullWhen(false)] out string? error)
        {
            value = default!;
            error = "updated parser failure";
            return false;
        }
    }

    private sealed class ParsingOptionsMonitor
        : IOptionsMonitor<ParsingOptions>
    {
        public ParsingOptions CurrentValue { get; set; } = new();
        public ParsingOptions Get(string? name) => CurrentValue;
        public IDisposable? OnChange(Action<ParsingOptions, string?> listener) => null;
    }

    [Fact]
    public void EntryPoint_ParsesWithoutACommandAndReportsFailuresWithoutFallback()
    {
        var monitor = new ParsingOptionsMonitor();
        var parser = new ValueParser(monitor);
        parser.TryParse<int[]>(["1", "2"], out var values, out var error).ShouldBeTrue();
        values.ShouldBe([1, 2]);
        error.ShouldBeNull();

        parser.TryParse<int>(["invalid"], out _, out error).ShouldBeFalse();
        error.ShouldBe("Invalid value: invalid");

        monitor.CurrentValue.AdditionalParsers.Add(new RejectingParser());
        parser.TryParse<int>(["42"], out _, out error).ShouldBeFalse();
        error.ShouldBe("updated parser failure");
    }

    private static async Task AssertParity<T>(string text, T expected, bool useEnvironment = true)
    {
        using var scope = new EnvironmentScope(text);
        await using var app = CliApplication.CreateBuilder("Shared parsing").Build();
        var option = (Option<T>)new OptionAttribute("--value") { DefaultFromEnvironment = useEnvironment ? Variable : null }
            .CreateOption(app.ApplicationServices, typeof(T), "value", null);
        var command = new Command("run") { option };
        var explicitResult = command.Parse(["--value", text]);
        explicitResult.Errors.ShouldBeEmpty();
        explicitResult.GetValue(option).ShouldBe(expected);
        if (useEnvironment)
        {
            var environmentResult = command.Parse([]);
            environmentResult.Errors.ShouldBeEmpty();
            environmentResult.GetValue(option).ShouldBe(expected);
        }

        var argument = app.ApplicationServices.GetRequiredService<ArgumentFactory>().Create<T>("value", null, null);
        var argumentResult = new Command("run") { argument }.Parse([text]);
        argumentResult.Errors.ShouldBeEmpty();
        argumentResult.GetValue(argument).ShouldBe(expected);
    }

    private sealed class CountingParser
        : IValueParser
    {
        public int Calls { get; private set; }

        public bool CanParse(Type type) => type == typeof(int);

        public bool TryParse<T>(ValueParsingContext context, out T value, [NotNullWhen(false)] out string? error)
        {
            Calls++;
            if (int.TryParse(context.Values.Single(), out var parsed))
            {
                value = (T)(object)(parsed * 2);
                error = null;
                return true;
            }

            value = default!;
            error = "custom failure";
            return false;
        }
    }

    private readonly record struct ParsedNumber(int Value)
        : IParsable<ParsedNumber>
    {
        public static ParsedNumber Parse(string text, IFormatProvider? provider)
            => TryParse(text, provider, out var value) ? value : throw new FormatException();

        public static bool TryParse(string? text, IFormatProvider? provider, out ParsedNumber value)
        {
            if (text is not null && text.StartsWith("parsed:", StringComparison.Ordinal)
                && int.TryParse(text.AsSpan(7), provider, out var number))
            {
                value = new(number);
                return true;
            }

            value = default;
            return false;
        }
    }

    private enum SampleEnum { First, Second }

    [ArgumentEnum]
    private enum AnnotatedEnum
    {
        [ArgumentValues("canonical", "alias")]
        Value,
    }

    private sealed class EnvironmentScope
        : IDisposable
    {
        private readonly string? _previous = Environment.GetEnvironmentVariable(Variable);
        public EnvironmentScope(string? text) => Environment.SetEnvironmentVariable(Variable, text);
        public void Dispose() => Environment.SetEnvironmentVariable(Variable, _previous);
    }
}
