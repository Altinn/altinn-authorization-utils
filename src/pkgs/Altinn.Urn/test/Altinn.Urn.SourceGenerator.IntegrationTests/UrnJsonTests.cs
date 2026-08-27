using Altinn.Urn.Json;
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
        var obj = JsonSerializer.Deserialize<DefaultObject>(json, _options).ShouldNotBeNull();
        obj.Urn.Urn.ShouldBe("urn:test:1234");
    }

    [Fact]
    public void DefaultConverter_CanParse_Objects()
    {
        var json = """{"urn":{"type":"urn:test","value":"1234"}}""";
        var obj = JsonSerializer.Deserialize<DefaultObject>(json, _options).ShouldNotBeNull();
        obj.Urn.Urn.ShouldBe("urn:test:1234");
    }

    [Fact]
    public void DefaultConverter_SerializesAs_Strings()
    {
        var obj = new DefaultObject { Urn = TestUrn.Test.Create(4321) };
        var json = JsonSerializer.Serialize(obj, _options);

        json.ShouldBe("""{"urn":"urn:test:4321"}""");
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
        var obj = JsonSerializer.Deserialize<DefaultObject>(json, _options).ShouldNotBeNull();
        obj.Urn.ShouldBeNull();

        var nullableObj = JsonSerializer.Deserialize<DefaultNullableObject>(json, _options).ShouldNotBeNull();
        nullableObj.Urn.ShouldBeNull();
    }

    [Fact]
    public void DefaultConverter_HandlesMissingPropertyCorrectly()
    {
        // System.Text.Json does not care about nullable reference types
        var json = """{}""";
        Should.Throw<JsonException>(() => JsonSerializer.Deserialize<DefaultObject>(json, _options));

        var nullableObj = JsonSerializer.Deserialize<DefaultNullableObject>(json, _options).ShouldNotBeNull();
        nullableObj.Urn.ShouldBeNull();
    }

    [Fact]
    public void StringConverter_CanParse_Strings()
    {
        var json = """{"urn":"urn:test:1234"}""";
        var obj = JsonSerializer.Deserialize<StringObject>(json, _options).ShouldNotBeNull();
        obj.Urn.Value!.Urn.ShouldBe("urn:test:1234");
    }

    [Fact]
    public void StringConverter_CanNotParse_Objects()
    {
        var json = """{"urn":{"type":"urn:test","value":"1234"}}""";
        Should.Throw<JsonException>(() => JsonSerializer.Deserialize<StringObject>(json, _options))
            .Message.ShouldBe($"Expected {nameof(TestUrn)} as string, but got {JsonTokenType.StartObject}");
    }

    [Fact]
    public void StringConverter_SerializesAs_Strings()
    {
        var obj = new StringObject { Urn = TestUrn.Test.Create(4321) };
        var json = JsonSerializer.Serialize(obj, _options);

        json.ShouldBe("""{"urn":"urn:test:4321"}""");
    }

    [Fact]
    public void ObjectConverter_CanParse_Objects()
    {
        var json = """{"urn":{"type":"urn:test","value":"1234"}}""";
        var obj = JsonSerializer.Deserialize<ObjectObject>(json, _options).ShouldNotBeNull();
        obj.Urn.Value!.Urn.ShouldBe("urn:test:1234");
    }

    [Fact]
    public void ObjectConverter_CanNotParse_Strings()
    {
        var json = """{"urn":"urn:test:1234"}""";
        Should.Throw<JsonException>(() => JsonSerializer.Deserialize<ObjectObject>(json, _options))
            .Message.ShouldBe($"Expected {nameof(TestUrn)} as object, but got {JsonTokenType.String}");
    }

    [Fact]
    public void ObjectConverter_SerializesAs_Objects()
    {
        var obj = new ObjectObject { Urn = TestUrn.Test.Create(4321) };
        var json = JsonSerializer.Serialize(obj, _options);

        json.ShouldBe("""{"urn":{"type":"urn:test","value":"4321"}}""");
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
        var obj = JsonSerializer.Deserialize<MixedObject>(json, _options).ShouldNotBeNull();
        obj.DefaultConverter.Urn.ShouldBe("urn:test:1234");
        obj.StringConverter.Value!.Urn.ShouldBe("urn:test:2345");
        obj.ObjectConverter.Value!.Urn.ShouldBe("urn:test:3456");
    }

    [Fact]
    public void Lists()
    {
        var json = """{"urns":[{"type":"urn:test","value":"1234"},{"type":"urn:test","value":"2345"}]}""";
        var obj = JsonSerializer.Deserialize<ObjectList>(json, _options).ShouldNotBeNull();
        obj.Urns.Count.ShouldBe(2);
        obj.Urns[0].Value!.Urn.ShouldBe("urn:test:1234");
        obj.Urns[1].Value!.Urn.ShouldBe("urn:test:2345");

        var serialized = JsonSerializer.Serialize(obj, _options);
        serialized.ShouldBe(json);

        json = """{"urns":["urn:test:1234"]}""";
        Should.Throw<JsonException>(() => JsonSerializer.Deserialize<ObjectList>(json, _options))
            .Message.ShouldBe($"Expected {nameof(TestUrn)} as object, but got {JsonTokenType.String}");
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
