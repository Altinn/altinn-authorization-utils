using Altinn.Urn.Json;
using FluentAssertions;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Altinn.Urn.SourceGenerator.IntegrationTests;

public partial class UrnJsonTests 
{
    private static readonly JsonSerializerOptions _options = new(JsonSerializerDefaults.Web);

    [Fact]
    public void DefaultConverter_CanParse_Strings()
    {
        var json = """{"urn":"urn:test:1234"}""";
        var obj = JsonSerializer.Deserialize<DefaultObject>(json, _options);

        Assert.NotNull(obj);
        obj.Urn.Urn.Should().Be("urn:test:1234");
    }

    [Fact]
    public void DefaultConverter_CanParse_Objects()
    {
        var json = """{"urn":{"type":"urn:test","value":"1234"}}""";
        var obj = JsonSerializer.Deserialize<DefaultObject>(json, _options);

        Assert.NotNull(obj);
        obj.Urn.Urn.Should().Be("urn:test:1234");
    }

    [Fact]
    public void DefaultConverter_SerializesAs_Strings()
    {
        var obj = new DefaultObject { Urn = TestUrn.Test.Create(4321) };
        var json = JsonSerializer.Serialize(obj, _options);

        json.Should().Be("""{"urn":"urn:test:4321"}""");
    }

    public record StringObj
    {
        public required string Value { get; init; }
    }

    [Fact]
    public void DefaultConverter_HandlesNullCorrectly()
    {
        // System.Text.Json does not care about nullable reference types
        var json = """{"urn":null}""";
        var obj = JsonSerializer.Deserialize<DefaultObject>(json, _options);
        Assert.NotNull(obj);
        obj.Urn.Should().BeNull();

        var nullableObj = JsonSerializer.Deserialize<DefaultNullableObject>(json, _options);
        Assert.NotNull(nullableObj);
        nullableObj.Urn.Should().BeNull();
    }

    [Fact]
    public void DefaultConverter_HandlesMissingPropertyCorrectly()
    {
        // System.Text.Json does not care about nullable reference types
        var json = """{}""";
        Action act = () => JsonSerializer.Deserialize<DefaultObject>(json, _options);
        act.Should().Throw<JsonException>();

        var nullableObj = JsonSerializer.Deserialize<DefaultNullableObject>(json, _options);
        Assert.NotNull(nullableObj);
        nullableObj.Urn.Should().BeNull();
    }

    [Fact]
    public void StringConverter_CanParse_Strings()
    {
        var json = """{"urn":"urn:test:1234"}""";
        var obj = JsonSerializer.Deserialize<StringObject>(json, _options);

        Assert.NotNull(obj);
        obj.Urn.Urn.Should().Be("urn:test:1234");
    }

    [Fact]
    public void StringConverter_CanNotParse_Objects()
    {
        var json = """{"urn":{"type":"urn:test","value":"1234"}}""";
        Action act = () => JsonSerializer.Deserialize<StringObject>(json, _options);
        act.Should().Throw<JsonException>()
            .Which.Message.Should().Be($"Expected {nameof(TestUrn)} as string, but got {JsonTokenType.StartObject}");
    }

    [Fact]
    public void StringConverter_SerializesAs_Strings()
    {
        var obj = new StringObject { Urn = TestUrn.Test.Create(4321) };
        var json = JsonSerializer.Serialize(obj, _options);

        json.Should().Be("""{"urn":"urn:test:4321"}""");
    }

    [Fact]
    public void ObjectConverter_CanParse_Objects()
    {
        var json = """{"urn":{"type":"urn:test","value":"1234"}}""";
        var obj = JsonSerializer.Deserialize<ObjectObject>(json, _options);

        Assert.NotNull(obj);
        obj.Urn.Urn.Should().Be("urn:test:1234");
    }

    [Fact]
    public void ObjectConverter_CanNotParse_Strings()
    {
        var json = """{"urn":"urn:test:1234"}""";
        Action act = () => JsonSerializer.Deserialize<ObjectObject>(json, _options);
        act.Should().Throw<JsonException>()
            .Which.Message.Should().Be($"Expected {nameof(TestUrn)} as object, but got {JsonTokenType.String}");
    }

    [Fact]
    public void ObjectConverter_SerializesAs_Objects()
    {
        var obj = new ObjectObject { Urn = TestUrn.Test.Create(4321) };
        var json = JsonSerializer.Serialize(obj, _options);

        json.Should().Be("""{"urn":{"type":"urn:test","value":"4321"}}""");
    }

    [Fact]
    public void MixedObjects()
    {
        var json =
            """
            {
                "defaultConverter": "urn:test:1234",
                "stringConverter": "urn:test:2345",
                "objectConverter": {"type":"urn:test","value":"3456"}
            }
            """;
        var obj = JsonSerializer.Deserialize<MixedObject>(json, _options);

        Assert.NotNull(obj);
        obj.DefaultConverter.Urn.Should().Be("urn:test:1234");
        obj.StringConverter.Urn.Should().Be("urn:test:2345");
        obj.ObjectConverter.Urn.Should().Be("urn:test:3456");
    }

    public record DefaultObject
    {
        public required TestUrn Urn { get; init; }
    }

    public record DefaultNullableObject
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public TestUrn? Urn { get; init; }
    }

    public record StringObject
    {
        [JsonConverter(typeof(StringUrnJsonConverter))]
        public required TestUrn Urn { get; init; }
    }

    public record ObjectObject
    {
        [JsonConverter(typeof(TypeValueObjectUrnJsonConverter))]
        public required TestUrn Urn { get; init; }
    }

    public record MixedObject
    {
        public required TestUrn DefaultConverter { get; init; }

        [JsonConverter(typeof(StringUrnJsonConverter))]
        public required TestUrn StringConverter { get; init; }

        [JsonConverter(typeof(TypeValueObjectUrnJsonConverter))]
        public required TestUrn ObjectConverter { get; init; }
    }

    [Urn]
    public abstract partial record TestUrn
    {
        [UrnType("test")]
        public partial bool IsTest(out int value);
    }
}
