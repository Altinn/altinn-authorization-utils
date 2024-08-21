using Altinn.Urn.Json;
using System.Text.Json;

namespace Altinn.Urn.Tests;

public class KeyValueUrnTests
{
    [Fact]
    public void Create_ThrowsIf_Null()
    {
        Action act = () => KeyValueUrn.Create(null!, 0);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Create_ThrowsIf_ValueIndexNegative()
    {
        Action act = () => KeyValueUrn.Create("urn:example:123", -1);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Create_ThrowsIf_ValueIndexGreaterOrEqualToUrnLength()
    {
        Action act = () => KeyValueUrn.Create("urn:example:123", 15);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Create_ThrowsIf_ValueIndexLessOrEqualTo4()
    {
        Action act = () => KeyValueUrn.Create("urn:example:123", 4);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Create_ThrowsIf_UrnDoesNotStartWithUrn()
    {
        Action act = () => KeyValueUrn.Create("example:123", 5);
        act.Should().Throw<ArgumentException>()
            .WithMessage("Urn must start with 'urn:'.*");
    }

    [Fact]
    public void Create_ThrowsIf_ValueNotPrecededBySeparator()
    {
        Action act = () => KeyValueUrn.Create("urn:example123", 7);
        act.Should().Throw<ArgumentException>()
            .WithMessage("Urn value must be preceded by a ':' separator.*");
    }

    [Theory]
    [InlineData("urn:example:123", 12, "example")]
    public void Key_ReturnsExpected(string urn, int valueIndex, string expected)
    {
        var sut = KeyValueUrn.Create(urn, valueIndex);
        sut.KeySpan.ToString().Should().Be(expected);
        sut.KeyMemory.ToString().Should().Be(expected);
    }

    [Theory]
    [InlineData("urn:example:123", 12, "123")]
    public void Value_ReturnsExpected(string urn, int valueIndex, string expected)
    {
        var sut = KeyValueUrn.Create(urn, valueIndex);
        sut.ValueSpan.ToString().Should().Be(expected);
        sut.ValueMemory.ToString().Should().Be(expected);
    }

    [Fact]
    public void Urn_ReturnsExpected()
    {
        var sut = KeyValueUrn.Create("urn:example:123", 12);
        sut.Urn.Should().Be("urn:example:123");
        sut.AsSpan().ToString().Should().Be("urn:example:123");
        sut.AsMemory().ToString().Should().Be("urn:example:123");
    }

    [Fact]
    public void HasValue_ReturnsTrue()
    {
        var sut = KeyValueUrn.Create("urn:example:123", 12);
        sut.HasValue.Should().BeTrue();
    }

    [Fact]
    public void Equals_ReturnsTrue_WhenEqual()
    {
        var sut1 = KeyValueUrn.Create("urn:example:123", 12);
        var sut2 = KeyValueUrn.Create("urn:example:123", 12);
        sut1.Equals(sut2).Should().BeTrue();
        sut1.GetHashCode().Should().Be(sut2.GetHashCode());
    }

    [Fact]
    public void Equals_ReturnsFalse_WhenDifferent_Urn()
    {
        var sut1 = KeyValueUrn.Create("urn:example:123", 12);
        var sut2 = KeyValueUrn.Create("urn:example:456", 12);
        sut1.Equals(sut2).Should().BeFalse();
    }

    [Fact]
    public void Equals_ReturnsFalse_WhenDifferent_ValueIndex()
    {
        var sut1 = KeyValueUrn.Create("urn:ex:ample:123", 7);
        var sut2 = KeyValueUrn.Create("urn:ex:ample:123", 13);
        sut1.Equals(sut2).Should().BeFalse();
    }

    [Fact]
    public void ToString_ReturnsExpected()
    {
        var sut = KeyValueUrn.Create("urn:example:123", 12);
        sut.ToString(null, null).Should().Be("urn:example:123");
        sut.ToString("P", null).Should().Be("example");
        sut.ToString("S", null).Should().Be("123");
        sut.ToString("V", null).Should().Be("123");
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
        sut.TryFormat([], out _, [], null).Should().BeFalse();

        static void Check(in KeyValueUrn sut, Span<char> chard, ReadOnlySpan<char> format, string expected)
        {
            sut.TryFormat(chard, out var charsWritten, format, null).Should().BeTrue();
            chard[..charsWritten].ToString().Should().Be(expected);
        }
    }

    [Fact]
    public void TypeValueJsonConverter_RoundTrip()
    {
        var json = """{"type":"urn:altinn:foo", "value":"1234"}""";
        var obj = JsonSerializer.Deserialize<UrnJsonTypeValue>(json);

        obj.Value.KeySpan.ToString().Should().Be("altinn:foo");
        obj.Value.ValueSpan.ToString().Should().Be("1234");

        var serialized = JsonSerializer.SerializeToDocument(json);
        serialized.RootElement.GetProperty("type").GetString().Should().Be("urn:altinn:foo");
        serialized.RootElement.GetProperty("value").GetString().Should().Be("1234");
    }
}
