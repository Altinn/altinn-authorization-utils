namespace Altinn.Urn.Tests;

public class UrnEncodedTests
{
    [Theory]
    [InlineData("?", "%3F")]
    [InlineData("&", "&")]
    [InlineData("=", "=")]
    [InlineData(":", "%3A")]
    [InlineData("+", "%2B")]
    [InlineData("%", "%25")]
    [InlineData("Øvrebø, Åstein Æser", "Øvrebø,+Åstein+Æser")]
    [InlineData("æ+ø å", "æ%2Bø+å")]
    public void RoundTrips(string value, string expected)
    {
        var encoded = UrnEncoded.Create(value);

        GetEncoded(encoded).ShouldBe(expected);

        UrnEncoded.TryParse(expected, out var decoded).ShouldBeTrue();
        decoded.ShouldBe(encoded);

        var encodedString = UrnEncoded.Encode(value);
        encodedString.ShouldBe(expected);

        encodedString = UrnEncoded.Encode(value.AsSpan());
        encodedString.ShouldBe(expected);

        UrnEncoded.TryDecode(expected, out var decodedString).ShouldBeTrue();
        decodedString.ShouldBe(value);

        UrnEncoded.TryDecode(expected.AsSpan(), out decodedString).ShouldBeTrue();
        decodedString.ShouldBe(value);
    }

    [Theory]
    [InlineData("%C3%98vreb%C3%B8%2C%20%C3%85stein%20%C3%86ser", "Øvrebø, Åstein Æser")]
    [InlineData("%C3%A6%2B%C3%B8%20%C3%A5", "æ+ø å")]

    public void Decode(string value, string expected)
    {
        UrnEncoded.TryParse(value, out var decoded).ShouldBeTrue();
        decoded.Value.ShouldBe(expected);

        UrnEncoded.TryDecode(value, out var decodedString).ShouldBeTrue();
        decodedString.ShouldBe(expected);
    }

    static string GetEncoded(UrnEncoded encoded)
    {
        var result = encoded.Encoded;
        var formattibleU = ((IFormattable)encoded).ToString("u", null);
        formattibleU.ShouldBe(result);

        return result;
    }
}
