namespace Altinn.Authorization.RepoCtl.Model.Tests;

public class AltinnVerticalIdTests
{
    private static readonly AltinnVerticalId Id = new(AltinnVerticalKind.Application, "AuthorizationService");

    [Theory]
    [InlineData(null, "app:AuthorizationService")]
    [InlineData("", "app:AuthorizationService")]
    [InlineData("k", "app")]
    [InlineData("kind", "app")]
    [InlineData("n", "AuthorizationService")]
    [InlineData("name", "AuthorizationService")]
    [InlineData("s", "app-authorizationservice")]
    [InlineData("slug", "app-authorizationservice")]
    [InlineData("tag-prefix", "app/AuthorizationService")]
    public void ToString_WithFormat_ReturnsExpectedValue(string? format, string expected)
    {
        var result = Id.ToString(format, formatProvider: null);

        result.ShouldBe(expected);
    }

    [Fact]
    public void ToString_WithUnsupportedFormat_ThrowsFormatException()
    {
        Should.Throw<FormatException>(() => Id.ToString("unsupported", formatProvider: null));
    }

    [Theory]
    [InlineData("", "app:AuthorizationService")]
    [InlineData("k", "app")]
    [InlineData("kind", "app")]
    [InlineData("n", "AuthorizationService")]
    [InlineData("name", "AuthorizationService")]
    [InlineData("s", "app-authorizationservice")]
    [InlineData("slug", "app-authorizationservice")]
    [InlineData("tag-prefix", "app/AuthorizationService")]
    public void TryFormat_WithSupportedFormat_WritesExpectedValue(string format, string expected)
    {
        Span<char> destination = stackalloc char[expected.Length];

        var success = Id.TryFormat(destination, out var charsWritten, format, provider: null);

        success.ShouldBeTrue();
        charsWritten.ShouldBe(expected.Length);
        destination[..charsWritten].ToString().ShouldBe(expected);
    }

    [Fact]
    public void TryFormat_WhenDestinationIsTooSmall_ReturnsFalse()
    {
        Span<char> destination = stackalloc char[Id.ToString().Length - 1];

        var success = Id.TryFormat(destination, out _, default, provider: null);

        success.ShouldBeFalse();
    }

    [Fact]
    public void TryFormat_WithUnsupportedFormat_ThrowsFormatException()
    {
        Should.Throw<FormatException>(static () =>
        {
            Span<char> destination = stackalloc char[32];
            Id.TryFormat(destination, out _, "unsupported", provider: null);
        });
    }

    [Theory]
    [InlineData("app:my-app", "app", "my-app")]
    [InlineData("lib:common", "lib", "common")]
    [InlineData("pkg:ServiceDefaults", "pkg", "ServiceDefaults")]
    [InlineData("pkg:Altinn.Authorization", "pkg", "Altinn.Authorization")]
    [InlineData("tool:repo-ctl", "tool", "repo-ctl")]
    public void Parse_WithValidString_ReturnsExpectedId(string value, string expectedKind, string expectedName)
    {
        var result = AltinnVerticalId.Parse(value, provider: null);

        result.Kind.ToString().ShouldBe(expectedKind);
        result.Name.ShouldBe(expectedName);
    }

    [Fact]
    public void Parse_WithValidSpan_ReturnsExpectedId()
    {
        ReadOnlySpan<char> value = "pkg:Altinn.Authorization";

        var result = AltinnVerticalId.Parse(value, provider: null);

        result.ShouldBe(new AltinnVerticalId(AltinnVerticalKind.Package, "Altinn.Authorization"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("app")]
    [InlineData("app:")]
    [InlineData("unknown:name")]
    [InlineData("APP:name")]
    [InlineData("prefix:name")]
    [InlineData("app:authorization service")]
    [InlineData("app:authorization\tservice")]
    [InlineData("pkg: ServiceDefaults")]
    public void TryParse_WithInvalidString_ReturnsFalse(string? value)
    {
        var success = AltinnVerticalId.TryParse(value, provider: null, out var result);

        success.ShouldBeFalse();
        result.ShouldBe(default);
    }

    [Fact]
    public void TryParse_WithValidSpan_ReturnsExpectedId()
    {
        ReadOnlySpan<char> value = "tool:repo-ctl";

        var success = AltinnVerticalId.TryParse(value, provider: null, out var result);

        success.ShouldBeTrue();
        result.ShouldBe(new AltinnVerticalId(AltinnVerticalKind.Tool, "repo-ctl"));
    }

    [Fact]
    public void TryParse_WithInvalidSpan_ReturnsFalse()
    {
        ReadOnlySpan<char> value = "not-an-id";

        var success = AltinnVerticalId.TryParse(value, provider: null, out var result);

        success.ShouldBeFalse();
        result.ShouldBe(default);
    }

    [Theory]
    [InlineData("not-an-id")]
    [InlineData("unknown:name")]
    [InlineData("app:authorization service")]
    public void Parse_WithInvalidString_ThrowsFormatException(string value)
    {
        Should.Throw<FormatException>(() => AltinnVerticalId.Parse(value, provider: null));
    }

    [Fact]
    public void Parse_WithInvalidSpan_ThrowsFormatException()
    {
        Should.Throw<FormatException>(() => AltinnVerticalId.Parse("not-an-id".AsSpan(), provider: null));
    }

    [Theory]
    [InlineData("authorization service")]
    [InlineData("authorization\tservice")]
    public void Constructor_WithWhitespaceInName_ThrowsArgumentException(string name)
    {
        Should.Throw<ArgumentException>(() => new AltinnVerticalId(AltinnVerticalKind.Application, name));
    }
}
