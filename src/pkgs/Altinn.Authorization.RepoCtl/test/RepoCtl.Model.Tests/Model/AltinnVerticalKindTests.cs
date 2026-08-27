using Altinn.Authorization.RepoCtl.Model;

namespace Altinn.Authorization.RepoCtl.Tests.Model;

public class AltinnVerticalKindTests
{
    [Theory]
    [InlineData("app", "app")]
    [InlineData("lib", "lib")]
    [InlineData("pkg", "pkg")]
    [InlineData("tool", "tool")]
    public void Parse_WhenValueIsKnown_ReturnsKind(string value, string expectedString)
    {
        var result = AltinnVerticalKind.Parse(value, provider: null);

        result.HasValue.ShouldBeTrue();
        result.ToString().ShouldBe(expectedString);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("application")]
    [InlineData("APP")]
    [InlineData("app, lib")]
    public void TryParse_WhenValueIsUnknown_ReturnsFalse(string? value)
    {
        var success = AltinnVerticalKind.TryParse(value, provider: null, out var result);

        success.ShouldBeFalse();
        result.ShouldBe(default);
        result.HasValue.ShouldBeFalse();
    }

    [Fact]
    public void Parse_WhenValueIsUnknown_ThrowsFormatException()
    {
        Should.Throw<FormatException>(() => AltinnVerticalKind.Parse("unknown", provider: null));
    }

    [Fact]
    public void TryParse_WithSpan_ParsesKnownKind()
    {
        ReadOnlySpan<char> value = "pkg";

        var success = AltinnVerticalKind.TryParse(value, provider: null, out var result);

        success.ShouldBeTrue();
        result.ShouldBe(AltinnVerticalKind.Package);
    }

    [Fact]
    public void Default_HasNoValueAndFormatsAsNone()
    {
        var kind = default(AltinnVerticalKind);

        kind.HasValue.ShouldBeFalse();
        kind.ToString().ShouldBe("none");
    }

    [Fact]
    public void TryFormat_WritesKindText()
    {
        Span<char> destination = stackalloc char[4];

        var success = AltinnVerticalKind.Tool.TryFormat(destination, out var charsWritten, default, provider: null);

        success.ShouldBeTrue();
        charsWritten.ShouldBe(4);
        destination[..charsWritten].ToString().ShouldBe("tool");
    }

    [Fact]
    public void TryFormat_WhenDestinationIsTooSmall_ReturnsFalse()
    {
        Span<char> destination = stackalloc char[2];

        var success = AltinnVerticalKind.Application.TryFormat(destination, out var charsWritten, default, provider: null);

        success.ShouldBeFalse();
        charsWritten.ShouldBe(0);
    }
}
