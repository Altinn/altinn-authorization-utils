namespace Altinn.Authorization.RepoCtl.Model.Tests;

public class AltinnVerticalIdTests
{
    private static readonly AltinnVerticalId Id = new(AltinnVerticalKind.Application, "AuthorizationService");

    [Theory]
    [InlineData("Altinn.Authorization.RepoCtl.Model", "RepoCtl.Model", "repo-ctl-model")]
    [InlineData("aLtInN.aUtHoRiZaTiOn.RepoCtl.Model", "RepoCtl.Model", "repo-ctl-model")]
    [InlineData("Altinn.RepoCtl.Model", "RepoCtl.Model", "repo-ctl-model")]
    [InlineData("ALTINN.RepoCtl.Model", "RepoCtl.Model", "repo-ctl-model")]
    [InlineData("Other.RepoCtl.Model", "Other.RepoCtl.Model", "other-repo-ctl-model")]
    [InlineData("RepoCtl.Model", "RepoCtl.Model", "repo-ctl-model")]
    [InlineData("Altinn.Authorization", "Authorization", "authorization")]
    [InlineData("Altinnish.RepoCtl.Model", "Altinnish.RepoCtl.Model", "altinnish-repo-ctl-model")]
    [InlineData("Other.Altinn.RepoCtl", "Other.Altinn.RepoCtl", "other-altinn-repo-ctl")]
    public void ShortFormats_RemoveOnlyKnownLeadingPrefixes(string name, string shortName, string shortSlug)
    {
        var id = new AltinnVerticalId(AltinnVerticalKind.Package, name);
        id.Name.ShouldBe(name);
        id.ToString().ShouldBe($"pkg:{name}");
        id.ToString("name", null).ShouldBe(name);
        id.ToString("tag-prefix", null).ShouldBe($"pkg/{shortName}");

        foreach (var (format, expected) in new[]
        {
            ("short-name", shortName),
            ("short-slug", shortSlug),
            ("slug", $"pkg-{shortSlug}"),
            ("s", $"pkg-{shortSlug}"),
            ("tag-prefix", $"pkg/{shortName}"),
        })
        {
            id.ToString(format, null).ShouldBe(expected);
            var destination = new char[expected.Length];
            id.TryFormat(destination, out var written, format, null).ShouldBeTrue();
            written.ShouldBe(expected.Length);
            new string(destination).ShouldBe(expected);
            id.TryFormat(destination.AsSpan(1), out written, format, null).ShouldBeFalse();
            written.ShouldBe(0);
        }
    }

    [Theory]
    [InlineData(null, "app:AuthorizationService")]
    [InlineData("", "app:AuthorizationService")]
    [InlineData("k", "app")]
    [InlineData("kind", "app")]
    [InlineData("n", "AuthorizationService")]
    [InlineData("name", "AuthorizationService")]
    [InlineData("s", "app-authorization-service")]
    [InlineData("slug", "app-authorization-service")]
    [InlineData("short-name", "AuthorizationService")]
    [InlineData("short-slug", "authorization-service")]
    [InlineData("tag-prefix", "app/AuthorizationService")]
    public void ToString_WithFormat_ReturnsExpectedValue(string? format, string expected)
    {
        var result = Id.ToString(format, formatProvider: null);

        result.ShouldBe(expected);
    }

    public static IEnumerable<object?[]> FormattingCases()
    {
        foreach (var kind in new[] { "app", "lib", "pkg", "tool" })
        {
            foreach (var (name, shortName, shortSlug) in new[]
            {
                ("AuthorizationService", "AuthorizationService", "authorization-service"),
                ("Altinn.Authorization.RepoCtl.Model", "RepoCtl.Model", "repo-ctl-model"),
                ("aLtInN.aUtHoRiZaTiOn.RepoCtl.Model", "RepoCtl.Model", "repo-ctl-model"),
                ("Altinn.RepoCtl.Model", "RepoCtl.Model", "repo-ctl-model"),
                ("Other.RepoCtl.Model", "Other.RepoCtl.Model", "other-repo-ctl-model"),
                ("Altinn.Authorization", "Authorization", "authorization"),
            })
            {
                foreach (var (format, expected) in new (string? Format, string Expected)[]
                {
                    (null, $"{kind}:{name}"),
                    ("", $"{kind}:{name}"),
                    ("k", kind),
                    ("kind", kind),
                    ("n", name),
                    ("name", name),
                    ("s", $"{kind}-{shortSlug}"),
                    ("slug", $"{kind}-{shortSlug}"),
                    ("short-name", shortName),
                    ("short-slug", shortSlug),
                    ("tag-prefix", $"{kind}/{shortName}"),
                })
                {
                    yield return [kind, name, format, expected];
                }
            }
        }
    }

    [Theory]
    [MemberData(nameof(FormattingCases))]
    public void Formatting_AllKindsAndNameVariants_ReturnExpectedValue(string kind, string name, string? format, string expected)
    {
        var id = new AltinnVerticalId(AltinnVerticalKind.Parse(kind, null), name);
        id.ToString(format, null).ShouldBe(expected);
        string.Format(System.Globalization.CultureInfo.InvariantCulture, $"{{0:{format}}}", id).ShouldBe(expected);
        if (string.IsNullOrEmpty(format))
        {
            id.ToString().ShouldBe(expected);
            $"{id}".ShouldBe(expected);
        }

        var exact = new char[expected.Length];
        id.TryFormat(exact, out var written, format, null).ShouldBeTrue();
        written.ShouldBe(expected.Length);
        new string(exact).ShouldBe(expected);

        var oversized = Enumerable.Repeat('#', expected.Length + 3).ToArray();
        id.TryFormat(oversized, out written, format, null).ShouldBeTrue();
        written.ShouldBe(expected.Length);
        new string(oversized, 0, written).ShouldBe(expected);
        new string(oversized, written, 3).ShouldBe("###");

        for (var length = 0; length < expected.Length; length++)
        {
            id.TryFormat(exact.AsSpan(0, length), out _, format, null).ShouldBeFalse();
        }
    }

    [Theory]
    [InlineData("unsupported")]
    [InlineData("SLUG")]
    [InlineData("Short-Name")]
    [InlineData("tag-prefix ")]
    public void ToString_WithUnsupportedFormat_ThrowsFormatException(string format)
    {
        Should.Throw<FormatException>(() => Id.ToString(format, formatProvider: null));
    }

    [Theory]
    [InlineData("", "app:AuthorizationService")]
    [InlineData("k", "app")]
    [InlineData("kind", "app")]
    [InlineData("n", "AuthorizationService")]
    [InlineData("name", "AuthorizationService")]
    [InlineData("s", "app-authorization-service")]
    [InlineData("slug", "app-authorization-service")]
    [InlineData("short-name", "AuthorizationService")]
    [InlineData("short-slug", "authorization-service")]
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

    [Theory]
    [InlineData("unsupported")]
    [InlineData("SLUG")]
    [InlineData("Short-Name")]
    [InlineData("tag-prefix ")]
    public void TryFormat_WithUnsupportedFormat_ThrowsFormatException(string format)
    {
        Should.Throw<FormatException>(() =>
        {
            Span<char> destination = stackalloc char[32];
            Id.TryFormat(destination, out _, format, provider: null);
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
