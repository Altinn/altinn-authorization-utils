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
        obj.Urn.Value!.Urn.Should().Be("urn:test:1234");
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
        obj.Urn.Value!.Urn.Should().Be("urn:test:1234");
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
        obj.StringConverter.Value!.Urn.Should().Be("urn:test:2345");
        obj.ObjectConverter.Value!.Urn.Should().Be("urn:test:3456");
    }

    [Fact]
    public void Lists()
    {
        var json = """{"urns":[{"type":"urn:test","value":"1234"},{"type":"urn:test","value":"2345"}]}""";
        var obj = JsonSerializer.Deserialize<ObjectList>(json, _options);

        Assert.NotNull(obj);
        obj.Urns.Should().HaveCount(2);
        obj.Urns[0].Value!.Urn.Should().Be("urn:test:1234");
        obj.Urns[1].Value!.Urn.Should().Be("urn:test:2345");

        var serialized = JsonSerializer.Serialize(obj, _options);
        serialized.Should().Be(json);

        json = """{"urns":["urn:test:1234"]}""";
        var act = () => JsonSerializer.Deserialize<ObjectList>(json, _options);
        act.Should().Throw<JsonException>()
            .Which.Message.Should().Be($"Expected {nameof(TestUrn)} as object, but got {JsonTokenType.String}");
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
        public required UrnJsonString<TestUrn> Urn { get; init; }
    }

    public record ObjectObject
    {
        public required UrnJsonTypeValue<TestUrn> Urn { get; init; }
    }

    public record MixedObject
    {
        public required TestUrn DefaultConverter { get; init; }

        public required UrnJsonString<TestUrn> StringConverter { get; init; }

        public required UrnJsonTypeValue<TestUrn> ObjectConverter { get; init; }
    }

    public record ObjectList
    {
        public required List<UrnJsonTypeValue<TestUrn>> Urns { get; init; }
    }

    [KeyValueUrn]
    public abstract partial record TestUrn
    {
        [UrnKey("test")]
        public partial bool IsTest(out int value);
    }
}
