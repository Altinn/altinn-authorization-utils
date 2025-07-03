using Altinn.Authorization.ModelUtils.Tests.Utils;
using System.Text.Json;

namespace Altinn.Authorization.ModelUtils.Tests;

public class JsonExtensionDataTests
{
    [Fact]
    public void Default_Equals_Default()
    {
        CheckEquals(default, default);
    }

    [Theory]
    [MemberData(nameof(JsonCases))]
    public void Same_Equals_Same(string json)
    {
        var left = (JsonExtensionData)Json.Deserialize<JsonElement>(json);
        var right = (JsonExtensionData)Json.Deserialize<JsonElement>(json);

        CheckEquals(left, right);
    }

    [Fact]
    public void Equality_Property_Order_Independent()
    {
        var left = (JsonExtensionData)Json.Deserialize<JsonElement>(
            """
            {
                "a": 1,
                "b": 2
            }
            """);
        var right = (JsonExtensionData)Json.Deserialize<JsonElement>(
            """
            {
                "b": 2,
                "a": 1
            }
            """);
        
        CheckEquals(left, right);
    }

    [Fact]
    public void Different_Values_Not_Equals()
    {
        var left = (JsonExtensionData)Json.Deserialize<JsonElement>(
            """
            {
                "a": 1,
                "b": 2
            }
            """);
        var right = (JsonExtensionData)Json.Deserialize<JsonElement>(
            """
            {
                "a": 1,
                "b": 3
            }
            """);
        
        CheckNotEquals(left, right);
    }

    [Fact]
    public void Dictionary_Implementations()
    {
        IReadOnlyDictionary<string, JsonElement> data = (JsonExtensionData)Json.Deserialize<JsonElement>(
            """
            {
                "str": "value1",
                "num": 42,
                "true": true,
                "false": false,
                "null": null,
                "arr": [1, 2, 3],
                "obj": { "nested": "value" }
            }
            """);

        data.Count.ShouldBe(7);
        data.Keys.ShouldBe(["str", "num", "true", "false", "null", "arr", "obj"]);
        data.Values.Select(static v => v.ValueKind).ShouldBe([
            JsonValueKind.String,
            JsonValueKind.Number,
            JsonValueKind.True,
            JsonValueKind.False,
            JsonValueKind.Null,
            JsonValueKind.Array,
            JsonValueKind.Object,
        ]);

        data["str"].GetString().ShouldBe("value1");
        data.TryGetValue("str", out var strValue).ShouldBeTrue();
        strValue.GetString().ShouldBe("value1");

        data.TryGetValue("nonexistent", out _).ShouldBeFalse();

        data.ContainsKey("str").ShouldBeTrue();
        data.ContainsKey("nonexistent").ShouldBeFalse();

        data.Select(static kvp => (kvp.Key, kvp.Value.ValueKind)).ShouldBe([
            ("str", JsonValueKind.String),
            ("num", JsonValueKind.Number),
            ("true", JsonValueKind.True),
            ("false", JsonValueKind.False),
            ("null", JsonValueKind.Null),
            ("arr", JsonValueKind.Array),
            ("obj", JsonValueKind.Object),
        ]);
    }

    [Fact]
    public void Requires_Object()
    {
        Validate("42");
        Validate("true");
        Validate("false");
        Validate("null");
        Validate("[]");
        Validate("1.23");
        Validate("\"string\"");

        static void Validate(string json)
        {
            var element = Json.Deserialize<JsonElement>(json);
            Should.Throw<ArgumentException>(() => new JsonExtensionData(element));
        }
    }

    private static void CheckEquals(JsonExtensionData left, JsonExtensionData right)
    {
        left.GetHashCode().ShouldBe(right.GetHashCode());
        left.Equals(right).ShouldBeTrue();
        right.Equals(left).ShouldBeTrue();
        (left == right).ShouldBeTrue();
        (right == left).ShouldBeTrue();
    }

    private static void CheckNotEquals(JsonExtensionData left, JsonExtensionData right)
    {
        left.Equals(right).ShouldBeFalse();
        right.Equals(left).ShouldBeFalse();
        (left == right).ShouldBeFalse();
        (right == left).ShouldBeFalse();
    }

    public static TheoryData<string> JsonCases => [
        """{}""",
        """
        {
            "str": "value1"
        }
        """,
        """
        {
            "str": "value1",
            "num": 42,
            "true": true,
            "false": false,
            "null": null,
            "arr": [1, 2, 3],
            "obj": { "nested": "value" }
        }
        """,
    ];
}
