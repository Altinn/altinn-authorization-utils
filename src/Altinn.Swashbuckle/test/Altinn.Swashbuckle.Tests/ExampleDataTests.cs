using Altinn.Swashbuckle.Examples;

namespace Altinn.Swashbuckle.Tests;

public class ExampleDataTests
{
    public static TheoryData<Type> BuiltinValueTypes => new() {
        typeof(bool),
        typeof(DateTime),
        typeof(DateTimeOffset),
        typeof(DateOnly),
        typeof(TimeOnly),
        typeof(Guid),
        typeof(byte),
        typeof(decimal),
        typeof(double),
        typeof(float),
        typeof(int),
        typeof(long),
        typeof(short),
        typeof(sbyte),
        typeof(uint),
        typeof(ulong),
        typeof(ushort)
    };

    public static TheoryData<Type> BuiltinNullableTypes
    {
        get
        {
            var data = new TheoryData<Type>();
            foreach (var type in BuiltinValueTypes)
            {
                data.Add(typeof(Nullable<>).MakeGenericType(type));
            }

            return data;
        }
    }

    [Theory]
    [MemberData(nameof(BuiltinValueTypes))]
    [MemberData(nameof(BuiltinNullableTypes))]
    public void CanGenerateTestDataForBuiltins(Type type)
    {
        var examples = ExampleData.GetExamples(type);

        Assert.NotNull(examples);
        var enumerator = examples.GetEnumerator();

        // check that it's not empty
        enumerator.MoveNext().Should().BeTrue();
    }

    [Fact]
    public void CanAddProviders()
    {
        var options = new ExampleDataOptions();
        options.Providers.Add(new ExampleStringProvider());

        var examples = ExampleData.GetExamples<string>(options);
        Assert.NotNull(examples);

        examples.Should().NotBeEmpty();
        examples.Should().Contain("foo");
        examples.Should().Contain("bar");

        // can still get builtins
        var intExamples = ExampleData.GetExamples<int>(options);
        Assert.NotNull(intExamples);

        intExamples.Should().NotBeEmpty();
    }

    [Fact]
    public void CanAddResolvers()
    {
        var options = new ExampleDataOptions();
        options.ProviderResolverChain.Add(new ExampleStringProviderResolver());

        var examples = ExampleData.GetExamples<string>(options);
        Assert.NotNull(examples);

        examples.Should().NotBeEmpty();
        examples.Should().Contain("foo");
        examples.Should().Contain("bar");

        // can still get builtins
        var intExamples = ExampleData.GetExamples<int>(options);
        Assert.NotNull(intExamples);

        intExamples.Should().NotBeEmpty();
    }

    [Fact]
    public void SupportsIExampleDataProvider()
    {
        var examples = ExampleData.GetExamples<TestData>();
        Assert.Null(examples);

        var options = new ExampleDataOptions();
        options.ProviderResolverChain.Add(new ExampleStringProviderResolver());

        examples = ExampleData.GetExamples<TestData>(options);
        Assert.NotNull(examples);

        examples.Should().NotBeEmpty();
        examples.Should().Contain(new TestData { Name = "foo" });
        examples.Should().Contain(new TestData { Name = "bar" });
    }

    private sealed class ExampleStringProvider 
        : ExampleDataProvider<string>
    {
        public override IEnumerable<string>? GetExamples(ExampleDataOptions options)
            => ["foo", "bar"];
    }

    private sealed class ExampleStringProviderResolver 
        : IExampleDataProviderResolver
    {
        public ExampleDataProvider? GetProvider(Type type, ExampleDataOptions options)
        {
            if (type == typeof(string))
            {
                return new ExampleStringProvider();
            }

            return null;
        }
    }

    private sealed record TestData
        : IExampleDataProvider<TestData>
    {
        public required string Name { get; init; }

        public static IEnumerable<TestData>? GetExamples(ExampleDataOptions options)
            => ExampleData.GetExamples<string>(options)?.Select(name => new TestData { Name = name });
    }
}
