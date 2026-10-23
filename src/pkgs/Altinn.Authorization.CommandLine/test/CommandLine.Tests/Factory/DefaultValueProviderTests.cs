using System.CommandLine;
using System.Diagnostics.CodeAnalysis;
using Altinn.Authorization.CommandLine.Factory;
using Altinn.Authorization.CommandLine.Help;
using Spectre.Console;

namespace Altinn.Authorization.CommandLine.Tests.Factory;

public class DefaultValueProviderTests
{
    [Fact]
    public void Providers_WorkWithoutCommandLineParsingContext()
    {
        var context = new DefaultValueProviderContext("value");
        DefaultValueProvider.FromValue(42).TryProvideValue(context, out var value, out var error).ShouldBeTrue();
        value.ShouldBe(42);
        error.ShouldBeNull();
        new FailingProvider().TryProvideValue(context, out _, out error).ShouldBeFalse();
        error.ShouldBe("No default for value.");
    }

    [Fact]
    public void ExplicitNull_IsADefaultValue()
    {
        var provider = DefaultValueProvider.FromValue<string?>(null);
        provider.ValueType.ShouldBe(typeof(string));
        var argument = new DefaultArgumentFactory([]).Create<string?>("value", null, provider);
        var command = new Command("run") { argument };

        var result = command.Parse([]);
        result.Errors.ShouldBeEmpty();
        result.GetValue(argument).ShouldBeNull();
    }

    [Fact]
    public void AbsentProvider_PreservesMissingInputError()
    {
        var option = new DefaultOptionFactory([]).Create<string>("--value", [], null, true, null);
        var command = new Command("run") { option };

        command.Parse([]).Errors.ShouldNotBeEmpty();
        command.Parse(["--value", "supplied"]).GetValue(option).ShouldBe("supplied");
    }

    [Fact]
    public void TypedFactory_AdaptsUntypedProvider()
    {
        var provider = DefaultValueProvider.FromValue<object>(42);
        var argument = new DefaultArgumentFactory([]).Create<int>("number", null, provider.Bind<int>());
        var command = new Command("run") { argument };

        command.Parse([]).GetValue(argument).ShouldBe(42);
    }

    [Fact]
    public void IncompatibleDefault_FailsDuringConstruction()
    {
        var factory = new DefaultOptionFactory([]);
        Should.Throw<ArgumentException>(() => factory.Create(typeof(int), "--number", [], null, false, DefaultValueProvider.FromValue("wrong type")));
        Should.Throw<ArgumentException>(() => DefaultValueProvider.FromValue<object?>(null).Bind<int>());
        Should.Throw<ArgumentException>(() => new FailingProvider().Bind<string>());
    }

    [Fact]
    public void Bind_AdaptsReferenceAndValueTypes()
    {
        var argument = new DefaultArgumentFactory([]).Create<object>("value", null, new TextProvider().Bind<object>());
        new Command("run") { argument }.Parse([]).GetValue(argument).ShouldBe("text");
        var number = new DefaultArgumentFactory([]).Create<object>("number", null, new FailingProvider().Bind<object>());
        new Command("run") { number }.Parse([]).Errors.Single().Message.ShouldBe("No default for number.");
    }

    [Fact]
    public void Bind_PreservesIdentityForExactType()
    {
        var provider = new TextProvider();
        provider.Bind<string>().ShouldBeSameAs(provider);
    }

    private sealed class TextProvider
        : DefaultValueProvider<string>
    {
        public override bool TryProvideValue(DefaultValueProviderContext context, out string value, [NotNullWhen(false)] out string? error)
        {
            value = "text";
            error = null;
            return true;
        }

        public override IEnumerable<TextSegment> GetHelpText(HelpDisplayArgumentContext context) => ["text"];
    }

    [Fact]
    public void Bind_BoxesValueTypesAndPreservesNullableDefaults()
    {
        var argument = new DefaultArgumentFactory([]).Create<object>("number", null, new NumberProvider().Bind<object>());
        new Command("run") { argument }.Parse([]).GetValue(argument).ShouldBe(42);
        var nullable = DefaultValueProvider.FromValue<object?>(null).Bind<int?>();
        nullable.ValueType.ShouldBe(typeof(int?));
    }

    private sealed class NumberProvider
        : DefaultValueProvider<int>
    {
        public override bool TryProvideValue(DefaultValueProviderContext context, out int value, [NotNullWhen(false)] out string? error)
        {
            value = 42;
            error = null;
            return true;
        }

        public override IEnumerable<TextSegment> GetHelpText(HelpDisplayArgumentContext context) => ["42"];
    }

    [Fact]
    public void CustomProviderFailure_ProducesParseErrorWithContext()
    {
        var option = new DefaultOptionFactory([]).Create<int>("--number", [], null, false, new FailingProvider());
        var command = new Command("run") { option };

        command.Parse([]).Errors.Single().Message.ShouldBe("No default for --number.");
        command.Parse(["--number", "42"]).GetValue(option).ShouldBe(42);
    }

    [Fact]
    public void ConstantProvider_UsesSuppliedHelpColor()
    {
        var constant = DefaultValueProvider.FromValue(42);
        var context = new HelpDisplayArgumentContext(Color.Green);

        constant.GetHelpText(context).Single().Style!.Value.Foreground.ShouldBe(Color.Green);
    }

    private sealed class FailingProvider
        : DefaultValueProvider<int>
    {
        public override bool TryProvideValue(DefaultValueProviderContext context, out int value, [NotNullWhen(false)] out string? error)
        {
            value = default;
            error = $"No default for {context.Name}.";
            return false;
        }

        public override IEnumerable<TextSegment> GetHelpText(HelpDisplayArgumentContext context) => ["custom default"];
    }
}
