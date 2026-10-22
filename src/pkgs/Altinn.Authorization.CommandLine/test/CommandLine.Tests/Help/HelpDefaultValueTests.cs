using System.CommandLine;
using System.Diagnostics.CodeAnalysis;
using Altinn.Authorization.CommandLine.Arguments;
using Altinn.Authorization.CommandLine.Factory;
using Altinn.Authorization.CommandLine.Help;
using Microsoft.Extensions.DependencyInjection;

namespace Altinn.Authorization.CommandLine.Tests.Help;

public class HelpDefaultValueTests
{
    [Fact]
    public async Task ConstantEnumDefaults_UseCanonicalNameForOptionsAndArguments()
    {
        await using var app = CliApplication.CreateBuilder("Default help").Build();
        var provider = DefaultValueProvider.FromValue<object>(Selection.None).Bind<Selection>();
        var option = app.ApplicationServices.GetRequiredService<OptionFactory>()
            .Create<Selection>("--selection", [], null, false, provider);
        var argument = app.ApplicationServices.GetRequiredService<ArgumentFactory>()
            .Create<Selection>("selection", null, provider);

        GetDefaultHelp(option).ShouldBe("default: none");
        GetDefaultHelp(argument).ShouldBe("default: none");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ConstantBooleanDefaults_MatchExistingHelpFormatting(bool value)
    {
        await AssertConstantHelpMatchesExisting(value);
        await AssertConstantHelpMatchesExisting<bool?>(value);
    }

    [Fact]
    public async Task ConstantCollectionDefaults_MatchExistingHelpFormatting()
    {
        await AssertConstantHelpMatchesExisting(new[] { "foo", "bar" });
        await AssertConstantHelpMatchesExisting(Array.Empty<string>());
        var option = new Option<string[]>("--value") { DefaultValueFactory = _ => ["foo", "bar"] };
        GetDefaultHelp(option).ShouldBe("default: foo,bar");
    }

    [Fact]
    public async Task CustomEnumProvider_UsesCanonicalNameAndNameOnlyContextWhenRenderingHelp()
    {
        await using var app = CliApplication.CreateBuilder("Default help").Build();
        var provider = new CustomEnumProvider();
        var option = app.ApplicationServices.GetRequiredService<OptionFactory>()
            .Create<Selection>("--selection", [], null, false, provider);
        var argument = app.ApplicationServices.GetRequiredService<ArgumentFactory>()
            .Create<Selection>("selection", null, provider);

        provider.Calls.ShouldBe(0);
        GetDefaultHelp(option).ShouldBe("default: none");
        GetDefaultHelp(argument).ShouldBe("default: none");
        option.HelpCustomization.DefaultValueProvider.ShouldBeSameAs(provider);
        argument.HelpCustomization.DefaultValueProvider.ShouldBeSameAs(provider);
        option.HelpCustomization.GetDefaultValue.ShouldNotBeNull();
        argument.HelpCustomization.GetDefaultValue.ShouldNotBeNull();
        provider.Calls.ShouldBe(2);
        provider.Names.ShouldBe(["--selection", "selection"]);
    }

    [Fact]
    public async Task ExplicitHelpOverrides_TakePrecedenceOverProvider()
    {
        await using var app = CliApplication.CreateBuilder("Default help").Build();
        var provider = new CustomEnumProvider();
        var option = app.ApplicationServices.GetRequiredService<OptionFactory>()
            .Create<Selection>("--selection", [], null, false, provider);
        var argument = app.ApplicationServices.GetRequiredService<ArgumentFactory>()
            .Create<Selection>("selection", null, provider);
        option.HelpCustomization.GetDefaultValue = context =>
        {
            context.DefaultValueProvider.ShouldBeSameAs(provider);
            return ["option override"];
        };
        argument.HelpCustomization.GetDefaultValue = context =>
        {
            context.DefaultValueProvider.ShouldBeSameAs(provider);
            return ["argument override"];
        };

        GetDefaultHelp(option).ShouldBe("default: option override");
        GetDefaultHelp(argument).ShouldBe("default: argument override");
        provider.Calls.ShouldBe(0);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task HiddenDefaults_SuppressProvidersAndEnumOverrides(bool hideAtSymbolLevel)
    {
        await using var app = CliApplication.CreateBuilder("Default help").Build();
        var options = app.ApplicationServices.GetRequiredService<OptionFactory>();
        var arguments = app.ApplicationServices.GetRequiredService<ArgumentFactory>();
        var provider = new CustomEnumProvider();
        var builder = new HiddenDefaultsHelpBuilder(hideAtSymbolLevel);
        Symbol[] parameters =
        [
            options.Create<int>("--number", [], null, false, DefaultValueProvider.FromValue(42)),
            arguments.Create<int>("number", null, DefaultValueProvider.FromValue(42)),
            options.Create<Selection>("--selection", [], null, false, provider),
            arguments.Create<Selection>("selection", null, provider),
        ];

        foreach (var parameter in parameters)
        {
            builder.DefaultValue(parameter).ShouldBeEmpty();
        }

        provider.Calls.ShouldBe(0);
    }

    private static async Task AssertConstantHelpMatchesExisting<T>(T value)
    {
        await using var app = CliApplication.CreateBuilder("Default help").Build();
        var provider = DefaultValueProvider.FromValue(value);
        var option = app.ApplicationServices.GetRequiredService<OptionFactory>()
            .Create<T>("--value", [], null, false, provider);
        var argument = app.ApplicationServices.GetRequiredService<ArgumentFactory>()
            .Create<T>("value", null, provider);
        var existingOption = new Option<T>("--value") { DefaultValueFactory = _ => value };
        var existingArgument = new Argument<T>("value") { DefaultValueFactory = _ => value };

        GetDefaultHelp(option).ShouldBe(GetDefaultHelp(existingOption));
        GetDefaultHelp(argument).ShouldBe(GetDefaultHelp(existingArgument));
    }

    private static string GetDefaultHelp(Symbol parameter)
        => string.Concat(new DefaultValueHelpBuilder().DefaultValue(parameter).Select(segment => segment.Text));

    private class DefaultValueHelpBuilder : DefaultHelpBuilder
    {
        public IReadOnlyList<TextSegment> DefaultValue(Symbol parameter)
            => GetArgumentDefaultValue(new Command("run"), parameter, displayArgumentName: true);
    }

    private sealed class HiddenDefaultsHelpBuilder(bool hideAtSymbolLevel) : DefaultValueHelpBuilder
    {
        protected override bool ShouldShowDefaultValue(Symbol symbol)
            => !hideAtSymbolLevel && base.ShouldShowDefaultValue(symbol);

        protected override bool ShouldShowDefaultValue(Option option) => false;

        protected override bool ShouldShowDefaultValue(Argument argument) => false;
    }

    private sealed class CustomEnumProvider : DefaultValueProvider<Selection>
    {
        public int Calls { get; private set; }
        public List<string> Names { get; } = [];

        public override bool TryProvideValue(DefaultValueProviderContext context, out Selection value, [NotNullWhen(false)] out string? error)
        {
            Calls++;
            Names.Add(context.Name);
            value = Selection.None;
            error = null;
            return true;
        }

        public override IEnumerable<TextSegment> GetHelpText(HelpDisplayArgumentContext context)
            => [new("custom selection", context.DefaultArgumentColor)];
    }

    [ArgumentEnum]
    private enum Selection
    {
        [ArgumentValues("none")]
        None,
    }
}
