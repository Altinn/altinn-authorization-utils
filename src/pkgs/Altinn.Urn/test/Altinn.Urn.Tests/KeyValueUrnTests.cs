using Altinn.Urn.Json;
using System.Text.Json;

namespace Altinn.Urn.Tests;

public class KeyValueUrnTests
{
    [Fact]
    public void Create_ThrowsIf_Null()
    {
        Action act = () => KeyValueUrn.Create(null!, 0);
        act.ShouldThrow<ArgumentNullException>();
    }

    [Fact]
    public void Create_ThrowsIf_ValueIndexNegative()
    {
        Action act = () => KeyValueUrn.Create("urn:example:123", -1);
        act.ShouldThrow<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Create_ThrowsIf_ValueIndexGreaterOrEqualToUrnLength()
    {
        Action act = () => KeyValueUrn.Create("urn:example:123", 15);
        act.ShouldThrow<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Create_ThrowsIf_ValueIndexLessOrEqualTo4()
    {
        Action act = () => KeyValueUrn.Create("urn:example:123", 4);
        act.ShouldThrow<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Create_ThrowsIf_UrnDoesNotStartWithUrn()
    {
        Action act = () => KeyValueUrn.Create("example:123", 5);
        act.ShouldThrow<ArgumentException>()
            .Message.ShouldStartWith("Urn must start with 'urn:'.");
    }

    [Fact]
    public void Create_ThrowsIf_ValueNotPrecededBySeparator()
    {
        Action act = () => KeyValueUrn.Create("urn:example123", 7);
        act.ShouldThrow<ArgumentException>()
            .Message.ShouldStartWith("Urn value must be preceded by a ':' separator.");
    }

    [Theory]
    [InlineData("urn:example:123", 12, "example")]
    public void Key_ReturnsExpected(string urn, int valueIndex, string expected)
    {
        var sut = KeyValueUrn.Create(urn, valueIndex);
        sut.KeySpan.ToString().ShouldBe(expected);
        sut.KeyMemory.ToString().ShouldBe(expected);
    }

    [Theory]
    [InlineData("urn:example:123", 12, "123")]
    public void Value_ReturnsExpected(string urn, int valueIndex, string expected)
    {
        var sut = KeyValueUrn.Create(urn, valueIndex);
        sut.ValueSpan.ToString().ShouldBe(expected);
        sut.ValueMemory.ToString().ShouldBe(expected);
    }

    [Fact]
    public void Urn_ReturnsExpected()
    {
        var sut = KeyValueUrn.Create("urn:example:123", 12);
        sut.Urn.ShouldBe("urn:example:123");
        sut.AsSpan().ToString().ShouldBe("urn:example:123");
        sut.AsMemory().ToString().ShouldBe("urn:example:123");
    }

    [Fact]
    public void HasValue_ReturnsTrue()
    {
        var sut = KeyValueUrn.Create("urn:example:123", 12);
        sut.HasValue.ShouldBeTrue();
    }

    [Fact]
    public void Equals_ReturnsTrue_WhenEqual()
    {
        var sut1 = KeyValueUrn.Create("urn:example:123", 12);
        var sut2 = KeyValueUrn.Create("urn:example:123", 12);
        sut1.Equals(sut2).ShouldBeTrue();
        sut1.GetHashCode().ShouldBe(sut2.GetHashCode());
    }

    [Fact]
    public void Equals_ReturnsFalse_WhenDifferent_Urn()
    {
        var sut1 = KeyValueUrn.Create("urn:example:123", 12);
        var sut2 = KeyValueUrn.Create("urn:example:456", 12);
        sut1.Equals(sut2).ShouldBeFalse();
    }

    [Fact]
    public void Equals_ReturnsFalse_WhenDifferent_ValueIndex()
    {
        var sut1 = KeyValueUrn.Create("urn:ex:ample:123", 7);
        var sut2 = KeyValueUrn.Create("urn:ex:ample:123", 13);
        sut1.Equals(sut2).ShouldBeFalse();
    }

    [Fact]
    public void ToString_ReturnsExpected()
    {
        var sut = KeyValueUrn.Create("urn:example:123", 12);
        sut.ToString(null, null).ShouldBe("urn:example:123");
        sut.ToString("P", null).ShouldBe("example");
        sut.ToString("S", null).ShouldBe("123");
        sut.ToString("V", null).ShouldBe("123");
    }

    [Fact]
    public void TryFormat_ReturnsExpected()
    {
        var sut = KeyValueUrn.Create("urn:example:123", 12);
        Span<char> chars = stackalloc char[20];
        Check(sut, chars, [], "urn:example:123");
        Check(sut, chars, ['P'], "example");
        Check(sut, chars, ['S'], "123");
        Check(sut, chars, ['V'], "123");
        sut.TryFormat([], out _, [], null).ShouldBeFalse();

        static void Check(in KeyValueUrn sut, Span<char> chard, ReadOnlySpan<char> format, string expected)
        {
            sut.TryFormat(chard, out var charsWritten, format, null).ShouldBeTrue();
            chard[..charsWritten].ToString().ShouldBe(expected);
        }
    }

    [Fact]
    public void TypeValueJsonConverter_RoundTrip()
    {
        var json = """{"type":"urn:altinn:foo", "value":"1234"}""";
        var obj = JsonSerializer.Deserialize<UrnJsonTypeValue>(json);

        obj.Value.KeySpan.ToString().ShouldBe("altinn:foo");
        obj.Value.ValueSpan.ToString().ShouldBe("1234");
        obj.ToString().ShouldBe("urn:altinn:foo:1234");

        var serialized = JsonSerializer.SerializeToDocument(obj);
        serialized.RootElement.GetProperty("type").GetString().ShouldBe("urn:altinn:foo");
        serialized.RootElement.GetProperty("value").GetString().ShouldBe("1234");
    }

    [Fact]
    public void UrnJsonTypeValue_Equality()
    {
        var urn1 = KeyValueUrn.Create("urn:altinn:foo", 11);
        var urn2 = KeyValueUrn.Create("urn:altinn:foo", 11);

        UrnJsonTypeValue obj1 = urn1;
        UrnJsonTypeValue obj2 = urn2;

        (obj1 == obj2).ShouldBeTrue();
        (obj1 != obj2).ShouldBeFalse();
    }

    [Fact]
    public void KeyValueUrn_IsSerializeable()
    {
        var sut = KeyValueUrn.Create("urn:example:123", 12);
        var json = JsonSerializer.Serialize(sut);

        json.ShouldBe(@"""urn:example:123""");
    }

    [Fact]
    public void KeyValueUrn_Deserialize_ShouldThrow()
    {
        var json = @"""urn:example:123""";
        Action act = () => JsonSerializer.Deserialize<KeyValueUrn>(json);

        act.ShouldThrow<NotSupportedException>()
            .Message.ShouldStartWith($"Deserialization of {nameof(KeyValueUrn)} is not supported.");
    }
}
