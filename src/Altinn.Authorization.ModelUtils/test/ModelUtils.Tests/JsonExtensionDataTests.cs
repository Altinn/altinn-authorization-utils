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
    public void Property_Order_Independent()
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
