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

        UrnEncoded.TryUnescape(expected, out var decoded).ShouldBeTrue();
        decoded.ShouldBe(encoded);
    }

    [Theory]
    [InlineData("%C3%98vreb%C3%B8%2C%20%C3%85stein%20%C3%86ser", "Øvrebø, Åstein Æser")]
    public void Decode(string value, string expected)
    {
        UrnEncoded.TryUnescape(value, out var decoded).ShouldBeTrue();
        decoded.Value.ShouldBe(expected);
    }

    static string GetEncoded(UrnEncoded encoded)
        => ((IFormattable)encoded).ToString("u", null);
}
