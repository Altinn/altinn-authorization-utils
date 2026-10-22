using System.CommandLine;
using System.Diagnostics.CodeAnalysis;
using Altinn.Authorization.CommandLine.Arguments;
using Altinn.Authorization.CommandLine.Factory;
using Altinn.Authorization.CommandLine.Parsing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;

namespace Altinn.Authorization.CommandLine.Tests.Parsing;

public class ValueParserTests
{
    [Fact]
    public async Task BuiltInConversions_WorkWithoutCommandLineContext()
    {
        await using var app = CliApplication.CreateBuilder("Parsing").Build();
        var parser = app.ApplicationServices.GetRequiredService<ValueParser>();
        Check<int>(["42"], 42);
        Check<int?>(["42"], 42);
        Check<int[]>(["1", "2"], [1, 2]);
        Check<IReadOnlyList<int>>(["1", "2"], new[] { 1, 2 });
        Check<string>(["with spaces"], "with spaces");
        Check<StringValues>(["one\n\ntwo\n"], new StringValues(["one", "two"]));
        Check<bool>([], true);
        Check<bool?>([], true);
        Check<int?>([], null);
        Check<SampleEnum>(["alias"], SampleEnum.Value);
        Check<DateOnly>(["2026-10-21"], new DateOnly(2026, 10, 21));
        parser.TryParse<int>(["invalid"], out _, out var error).ShouldBeFalse();
        error.ShouldBe("Invalid value: invalid");
        parser.TryParse<object>(["unsupported"], out _, out error).ShouldBeFalse();
        error.ShouldContain("No value parser");

        void Check<T>(string[] values, T expected)
        {
            parser.TryParse<T>(values, out var parsed, out var error).ShouldBeTrue();
            parsed.ShouldBe(expected);
            error.ShouldBeNull();
        }
    }

    [Fact]
    public async Task CliAdaptersAndInnerValues_UseTheConfiguredPipeline()
    {
        var builder = CliApplication.CreateBuilder("Parsing");
        builder.Services.Configure<ParsingOptions>(options => options.AdditionalParsers.Add(new PrefixParser()));
        await using var app = builder.Build();
        var parser = app.ApplicationServices.GetRequiredService<ValueParser>();
        parser.TryParse<int?>(["prefix:42"], out var nullable, out _).ShouldBeTrue();
        nullable.ShouldBe(42);
        parser.TryParse<int[]>(["prefix:1", "prefix:2"], out var array, out _).ShouldBeTrue();
        array.ShouldBe([1, 2]);
        var option = app.ApplicationServices.GetRequiredService<OptionFactory>().Create<int>("--value", [], null, true, null);
        var argument = app.ApplicationServices.GetRequiredService<ArgumentFactory>().Create<int>("value", null, null);
        var command = new Command("run") { option, argument };
        var result = command.Parse(["--value", "prefix:3", "prefix:4"]);
        result.Errors.ShouldBeEmpty();
        result.GetValue(option).ShouldBe(3);
        result.GetValue(argument).ShouldBe(4);
    }

    [Theory]
    [InlineData(new string[] { }, null)]
    [InlineData(new[] { "--flag" }, true)]
    [InlineData(new[] { "--flag", "true" }, true)]
    [InlineData(new[] { "--flag", "false" }, false)]
    public async Task NullableBooleanOption_PreservesFlagSemantics(string[] args, bool? expected)
    {
        await using var app = CliApplication.CreateBuilder("Parsing").Build();
        var option = app.ApplicationServices.GetRequiredService<OptionFactory>().Create<bool?>("--flag", [], null, false, null);
        var result = new Command("run") { option }.Parse(args);

        result.Errors.ShouldBeEmpty();
        result.GetValue(option).ShouldBe(expected);
    }

    [Fact]
    public void OptionsUpdates_AreReadAtEntryAndFailuresDoNotFallThrough()
    {
        var monitor = new TestMonitor();
        var parser = new ValueParser(monitor);
        parser.TryParse<int>(["42"], out var value, out _).ShouldBeTrue();
        value.ShouldBe(42);
        monitor.CurrentValue = new ParsingOptions();
        monitor.CurrentValue.AdditionalParsers.Add(new RejectingParser());
        parser.TryParse<int>(["42"], out _, out var error).ShouldBeFalse();
        error.ShouldBe("Rejected value.");
    }

    [Fact]
    public async Task FileTypes_UseFileConversion()
    {
        await using var app = CliApplication.CreateBuilder("Parsing").Build();
        var parser = app.ApplicationServices.GetRequiredService<ValueParser>();
        var path = Path.GetFullPath("test-path");
        parser.TryParse<FileInfo>([path], out var file, out _).ShouldBeTrue();
        file.FullName.ShouldBe(path);
        parser.TryParse<DirectoryInfo>([path], out var directory, out _).ShouldBeTrue();
        directory.FullName.ShouldBe(path);
        parser.TryParse<FileSystemInfo>([path], out var info, out _).ShouldBeTrue();
        info.FullName.ShouldBe(path);
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

    private sealed class RejectingParser
        : IValueParser
    {
        public bool CanParse(Type type) => type == typeof(int);
        public bool TryParse<T>(ValueParsingContext context, out T value, [NotNullWhen(false)] out string? error)
        {
            value = default!;
            error = "Rejected value.";
            return false;
        }
    }

    private sealed class TestMonitor
        : IOptionsMonitor<ParsingOptions>
    {
        public ParsingOptions CurrentValue { get; set; } = new();
        public ParsingOptions Get(string? name) => CurrentValue;
        public IDisposable? OnChange(Action<ParsingOptions, string?> listener) => null;
    }

    [ArgumentEnum]
    private enum SampleEnum
    {
        [ArgumentValues("value", "alias")]
        Value,
    }
}
