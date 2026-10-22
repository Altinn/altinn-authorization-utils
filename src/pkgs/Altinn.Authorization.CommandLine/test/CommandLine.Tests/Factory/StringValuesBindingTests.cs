using System.CommandLine;
using System.Runtime.CompilerServices;
using Altinn.Authorization.CommandLine.Factory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;

namespace Altinn.Authorization.CommandLine.Tests.Factory;

public class StringValuesBindingTests
{
    [Theory]
    [InlineData("--values", "one two")]
    [InlineData("--values", "one", "--values", "two")]
    [InlineData("--values", " \tone\r\ntwo  ")]
    [InlineData("--values", "one ", "--values", "\ttwo\n")]
    [InlineData("--values", "one\n\ntwo\n")]
    [InlineData("--values", "one", "--values", "\n\n", "--values", "two")]
    public async Task Option_SplitsValuesBeforeInvokingHandler(params string[] args)
    {
        StringValues? recorded = null;
        await using var application = CliApplication.CreateBuilder("StringValues tests").Build();
        application.AddCommand("run", "Run", ([Option("--values")] StringValues values) => { recorded = values; });

        (await application.RunAsync(["run", .. args], TestContext.Current.CancellationToken)).ShouldBe(0);

        recorded.ShouldNotBeNull();
        recorded.Value.ToArray().ShouldBe(["one", "two"]);
    }

    [Fact]
    public async Task Option_PreservesOrderAndDuplicates()
    {
        StringValues recorded = default;
        await using var application = CliApplication.CreateBuilder("StringValues tests").Build();
        application.AddCommand("run", "Run", ([Option("--values")] StringValues values) => { recorded = values; });

        (await application.RunAsync(["run", "--values", "two one two"], TestContext.Current.CancellationToken)).ShouldBe(0);

        recorded.ToArray().ShouldBe(["two", "one", "two"]);
    }

    [Theory]
    [InlineData()]
    [InlineData("--values")]
    [InlineData("--values", "")]
    [InlineData("--values", " \t\n")]
    public async Task RequiredOption_RejectsMissingOrEmptyValues(params string[] args)
    {
        var invoked = false;
        await using var application = CliApplication.CreateBuilder("StringValues tests").Build();
        application.AddCommand("run", "Run", ([Option("--values")] StringValues values) => { invoked = true; });

        (await application.RunAsync(["run", .. args], TestContext.Current.CancellationToken)).ShouldNotBe(0);

        invoked.ShouldBeFalse();
    }

    [Fact]
    public async Task OptionalOption_BindsEmptyValuesWhenOmitted()
    {
        StringValues? recorded = null;
        await using var application = CliApplication.CreateBuilder("StringValues tests").Build();
        application.AddCommand("run", "Run", ([Option("--values")] StringValues values = default) => { recorded = values; });

        (await application.RunAsync(["run"], TestContext.Current.CancellationToken)).ShouldBe(0);

        recorded.ShouldNotBeNull();
        recorded.Value.Count.ShouldBe(0);
    }

    [Theory]
    [InlineData("--values")]
    [InlineData("--values", "")]
    [InlineData("--values", " \t\n")]
    public async Task OptionalOption_RejectsExplicitEmptyValues(params string[] args)
    {
        var invoked = false;
        await using var application = CliApplication.CreateBuilder("StringValues tests").Build();
        application.AddCommand("run", "Run", ([Option("--values")] StringValues values = default) => { invoked = true; });

        (await application.RunAsync(["run", .. args], TestContext.Current.CancellationToken)).ShouldNotBe(0);
        invoked.ShouldBeFalse();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" \t\n")]
    public async Task OptionalArgument_RejectsExplicitEmptyValues(string value)
    {
        var invoked = false;
        await using var application = CliApplication.CreateBuilder("StringValues tests").Build();
        application.AddCommand("run", "Run", ([Argument] StringValues values = default) => { invoked = true; });

        (await application.RunAsync(["run", value], TestContext.Current.CancellationToken)).ShouldNotBe(0);
        invoked.ShouldBeFalse();
    }

    [Theory]
    [InlineData("one two")]
    [InlineData("one", "two")]
    [InlineData("\tone\n", "two")]
    public async Task Argument_SplitsValuesBeforeInvokingHandler(params string[] args)
    {
        StringValues recorded = default;
        await using var application = CliApplication.CreateBuilder("StringValues tests").Build();
        application.AddCommand("run", "Run", ([Argument] StringValues values) => { recorded = values; });

        (await application.RunAsync(["run", .. args], TestContext.Current.CancellationToken)).ShouldBe(0);

        recorded.ToArray().ShouldBe(["one", "two"]);
    }

    [Theory]
    [InlineData()]
    [InlineData(" \t\n")]
    public async Task RequiredArgument_RejectsMissingOrEmptyValues(params string[] args)
    {
        var invoked = false;
        await using var application = CliApplication.CreateBuilder("StringValues tests").Build();
        application.AddCommand("run", "Run", ([Argument] StringValues values) => { invoked = true; });

        (await application.RunAsync(["run", .. args], TestContext.Current.CancellationToken)).ShouldNotBe(0);

        invoked.ShouldBeFalse();
    }

    [Fact]
    public async Task OptionalArgument_BindsEmptyValuesWhenOmitted()
    {
        StringValues? recorded = null;
        await using var application = CliApplication.CreateBuilder("StringValues tests").Build();
        application.AddCommand("run", "Run", ([Argument] StringValues values = default) => { recorded = values; });

        (await application.RunAsync(["run"], TestContext.Current.CancellationToken)).ShouldBe(0);

        recorded.ShouldNotBeNull();
        recorded.Value.Count.ShouldBe(0);
    }

    [Fact]
    public async Task Option_DoesNotConsumeFollowingOptions()
    {
        StringValues recorded = default;
        var flag = false;
        await using var application = CliApplication.CreateBuilder("StringValues tests").Build();
        application.AddCommand("run", "Run", ([Option("--values")] StringValues values, [Option("--flag")] bool enabled = false) =>
        {
            recorded = values;
            flag = enabled;
        });

        (await application.RunAsync(["run", "--values", "one two", "--flag"], TestContext.Current.CancellationToken)).ShouldBe(0);

        recorded.ToArray().ShouldBe(["one", "two"]);
        flag.ShouldBeTrue();
    }

    [Fact]
    public async Task Option_LeavesFollowingTokenForPositionalArgument()
    {
        StringValues recorded = default;
        string? positional = null;
        await using var application = CliApplication.CreateBuilder("StringValues tests").Build();
        application.AddCommand("run", "Run", ([Option("--values")] StringValues values, [Argument] string argument) =>
        {
            recorded = values;
            positional = argument;
        });

        (await application.RunAsync(["run", "--values", "one", "two"], TestContext.Current.CancellationToken)).ShouldBe(0);

        recorded.ToArray().ShouldBe(["one"]);
        positional.ShouldBe("two");
    }

    [Fact]
    public async Task StringArrayOption_PreservesWhitespaceWithinTokens()
    {
        string[]? recorded = null;
        await using var application = CliApplication.CreateBuilder("StringValues tests").Build();
        application.AddCommand("run", "Run", ([Option("--values")] string[] values) => { recorded = values; });

        (await application.RunAsync(["run", "--values", "one two"], TestContext.Current.CancellationToken)).ShouldBe(0);

        recorded.ShouldBe(["one two"]);
    }

    [Fact]
    public async Task Factories_PreserveDefaultsAndParseExplicitValues()
    {
        await using var application = CliApplication.CreateBuilder("StringValues tests").Build();
        var optionFactory = application.ApplicationServices.GetRequiredService<OptionFactory>();
        var argumentFactory = application.ApplicationServices.GetRequiredService<ArgumentFactory>();
        var option = optionFactory.Create<StringValues>("--values", [], null, false, new StrongBox<StringValues>(new("option-default")));
        var argument = argumentFactory.Create<StringValues>("argument", null, new StrongBox<StringValues>(new("argument-default")));
        var command = new RootCommand { option, argument };

        var omitted = command.Parse([]);
        omitted.GetValue(option).ToArray().ShouldBe(["option-default"]);
        omitted.GetValue(argument).ToArray().ShouldBe(["argument-default"]);

        var provided = command.Parse(["arg one", "--values", "opt two"]);
        provided.GetValue(option).ToArray().ShouldBe(["opt", "two"]);
        provided.GetValue(argument).ToArray().ShouldBe(["arg", "one"]);
    }
}
