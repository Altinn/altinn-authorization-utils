using System.Collections.Immutable;

namespace Altinn.Authorization.ProblemDetails.Tests;

public class ProblemExtensionDataTests
{
    [Fact]
    public static void Default_AreEqual()
    {
        AssertEqual(default, default);
    }

    [Fact]
    public static void Empty_AreEqual()
    {
        AssertEqual(ProblemExtensionData.Empty, ProblemExtensionData.Empty);
    }

    [Fact]
    public static void Default_And_Empty_AreEqual()
    {
        AssertEqual(default, ProblemExtensionData.Empty);
    }

    [Fact]
    public static void SingleItem_Equal_AreEqual()
    {
        ProblemExtensionData expected = [
            KeyValuePair.Create("key", "value"),
        ];

        ProblemExtensionData actual = [
            KeyValuePair.Create("key", "value"),
        ];

        AssertEqual(expected, actual);
    }

    [Fact]
    public static void SingleItem_DifferentValue_AreNotEqual()
    {
        ProblemExtensionData expected = [
            KeyValuePair.Create("key", "value"),
        ];

        ProblemExtensionData actual = [
            KeyValuePair.Create("key", "value 2"),
        ];

        AssertNotEqual(expected, actual);
    }

    [Fact]
    public static void SingleItem_DifferentKey_AreNotEqual()
    {
        ProblemExtensionData expected = [
            KeyValuePair.Create("key", "value"),
        ];

        ProblemExtensionData actual = [
            KeyValuePair.Create("key-2", "value"),
        ];

        AssertNotEqual(expected, actual);
    }

    [Fact]
    public static void SingleItem_DifferentKeyAndValue_AreNotEqual()
    {
        ProblemExtensionData expected = [
            KeyValuePair.Create("key", "value"),
        ];

        ProblemExtensionData actual = [
            KeyValuePair.Create("key-2", "value 2"),
        ];

        AssertNotEqual(expected, actual);
    }

    [Fact]
    public static void MultipleItems_Equal_SameOrder_AreEqual()
    {
        ProblemExtensionData expected = [
            KeyValuePair.Create("key", "value"),
            KeyValuePair.Create("key-2", "value 2"),
        ];

        ProblemExtensionData actual = [
            KeyValuePair.Create("key", "value"),
            KeyValuePair.Create("key-2", "value 2"),
        ];

        AssertEqual(expected, actual);
    }

    [Fact]
    public static void MultipleItems_Equal_DifferentOrder_AreEqual()
    {
        ProblemExtensionData expected = [
            KeyValuePair.Create("key", "value"),
            KeyValuePair.Create("key-2", "value 2"),
        ];

        ProblemExtensionData actual = [
            KeyValuePair.Create("key-2", "value 2"),
            KeyValuePair.Create("key", "value"),
        ];

        AssertEqual(expected, actual);
    }

    [Fact]
    public static void MultipleItems_Different_AreNotEqual()
    {
        ProblemExtensionData expected = [
            KeyValuePair.Create("key", "value"),
            KeyValuePair.Create("key-2", "value 2"),
        ];

        ProblemExtensionData actual = [
            KeyValuePair.Create("key-2", "not value"),
            KeyValuePair.Create("key", "value"),
        ];

        AssertNotEqual(expected, actual);
    }

    [Fact]
    public static void MultipleItems_SameKey_DifferentOrder_AreEqual()
    {
        ProblemExtensionData expected = [
            KeyValuePair.Create("key", "value"),
            KeyValuePair.Create("key", "value 2"),
        ];

        ProblemExtensionData actual = [
            KeyValuePair.Create("key", "value 2"),
            KeyValuePair.Create("key", "value"),
        ];

        AssertEqual(expected, actual);
    }

    [Fact]
    public static void MultipleItems_SameKey_SameOrder_AreEqual()
    {
        ProblemExtensionData expected = [
            KeyValuePair.Create("key", "value"),
            KeyValuePair.Create("key", "value 2"),
        ];

        ProblemExtensionData actual = [
            KeyValuePair.Create("key", "value"),
            KeyValuePair.Create("key", "value 2"),
        ];

        AssertEqual(expected, actual);
    }

    [Fact]
    public static void MoreThan50Items_AreEqual()
    {
        var builder = ImmutableArray.CreateBuilder<KeyValuePair<string, string>>();
        for (var i = 0; i < 100; i++)
        {
            builder.Add(KeyValuePair.Create($"key-{i}", $"value-{i}"));
        }

        ProblemExtensionData expected = builder.ToImmutable();

        // this sorts alphabetically by the key, thus key-11 will be before key-2
        builder.Sort((x, y) => string.Compare(x.Key, y.Key, StringComparison.Ordinal));

        ProblemExtensionData actual = builder.ToImmutable();

        AssertEqual(expected, actual);
    }

    [Fact]
    public static void Empty_KeyLookup_DoesNotThrow()
    {
        var data = ProblemExtensionData.Empty;

        var values = data["key"];
        Assert.Empty(values);
    }

    [Fact]
    public static void MultipleItems_SameKey_KeyLookup_ReturnsAllMatchingValues()
    {
        ProblemExtensionData data = [
            KeyValuePair.Create("first key", "first value"),
            KeyValuePair.Create("key", "value"),
            KeyValuePair.Create("key", "value 2"),
            KeyValuePair.Create("other key", "other value"),
        ];

        var values = data["key"];
        Assert.Collection(values,
            value => Assert.Equal("value", value),
            value => Assert.Equal("value 2", value));
    }

    [Fact]
    public static void Dictionary_Values_Tests()
    {
        ProblemExtensionData data = [
            KeyValuePair.Create("first key", "first value"),
            KeyValuePair.Create("key", "value"),
            KeyValuePair.Create("key", "value 2"),
            KeyValuePair.Create("other key", "other value"),
        ];

        var values = (data as IDictionary<string, object?>).Values;
        values.Should().HaveCount(4);
        values.Should().BeEquivalentTo(["first value", "value", "value 2", "other value"]);
        values.IsReadOnly.Should().BeTrue();
        values.Contains("value").Should().BeTrue();
    }

    [Fact]
    public static void Dictionary_Keys_Tests()
    {
        ProblemExtensionData data = [
            KeyValuePair.Create("first key", "first value"),
            KeyValuePair.Create("key", "value"),
            KeyValuePair.Create("key", "value 2"),
            KeyValuePair.Create("other key", "other value"),
        ];

        var keys = (data as IDictionary<string, object?>).Keys;
        keys.Should().HaveCount(4);
        keys.Should().BeEquivalentTo(["first key", "key", "key", "other key"]);
        keys.IsReadOnly.Should().BeTrue();
        keys.Contains("key").Should().BeTrue();
    }

    private static void AssertEqual(ProblemExtensionData expected, ProblemExtensionData actual)
    {
        Assert.Equal(expected, actual);
        Assert.Equal(expected.GetHashCode(), actual.GetHashCode());
        Assert.True(expected.Equals(actual));
        Assert.True(expected == actual);
        Assert.False(expected != actual);
    }

    private static void AssertNotEqual(ProblemExtensionData expected, ProblemExtensionData actual)
    {
        Assert.NotEqual(expected, actual);
        Assert.False(expected.Equals(actual));
        Assert.False(expected == actual);
        Assert.True(expected != actual);
    }
}
