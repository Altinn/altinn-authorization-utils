using FluentAssertions;
using System.Diagnostics.CodeAnalysis;

namespace Altinn.Urn.SourceGenerator.IntegrationTests;

public partial class PersonUrnTests
{
    [Theory]
    [InlineData("urn:altinn:person:identifier-no:12345678901", PersonUrn.Type.IdentifierNo, "urn:altinn:person:identifier-no", "12345678901")]
    [InlineData("urn:altinn:party:id:123456", PersonUrn.Type.PartyId, "urn:altinn:party:id", "123456")]
    [InlineData("urn:altinn:party:uuid:12345678-1234-1234-1234-123456789012", PersonUrn.Type.PartyUuid, "urn:altinn:party:uuid", "12345678-1234-1234-1234-123456789012")]
    public void GeneralUrnTests(string urn, PersonUrn.Type type, string prefixString, string valueString)
    {
        Assert.True(PersonUrn.TryParse(urn, out var personUrn));
        personUrn.Urn.Should().Be(urn);
        personUrn.UrnType.Should().Be(type);

        new string(personUrn.PrefixSpan).Should().Be(prefixString);
        new string(personUrn.ValueSpan).Should().Be(valueString);

        var formatted = $"{personUrn:P}:{personUrn:S}";
        formatted.Should().Be(urn);

        formatted = $"{personUrn}";
        formatted.Should().Be(urn);
    }

    [Fact]
    public void ValueCanHaveColon()
    {
        Assert.True(AnyUrn.TryParse("urn:any:foo:bar", out var anyUrn));
        Assert.True(anyUrn.IsAny(out var value));
        value.Value.Should().Be("foo:bar");
    }

    [Urn]
    public abstract partial record PersonUrn
    {
        [UrnType("altinn:person:identifier-no")]
        public partial bool IsIdentifierNo(out PersonIdentifier personId);

        [UrnType("altinn:organization:org-no")]
        public partial bool IsOrganizationNo(out OrgNo orgNo);

        [UrnType("altinn:party:id")]
        public partial bool IsPartyId(out int partyId);

        [UrnType("altinn:party:uuid")]
        public partial bool IsPartyUuid(out Guid partyUuid);

        [UrnType("altinn:person:d-number")]
        public partial bool IsDNumber(out int dNumber);
    }

    [Urn]
    public abstract partial record AnyUrn
    {
        [UrnType("any")]
        public partial bool IsAny(out AnyValue value);
    }

    public record AnyValue(string Value)
        : IParsable<AnyValue>
        , ISpanParsable<AnyValue>
        , IFormattable
        , ISpanFormattable
    {
        public static AnyValue Parse(string s, IFormatProvider? provider)
        {
            return new(s);
        }

        public static AnyValue Parse(ReadOnlySpan<char> s, IFormatProvider? provider)
        {
            return new(new string(s));
        }

        public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out AnyValue result)
        {
            if (s is null)
            {
                result = null;
                return false;
            }

            result = new(s);
            return true;
        }

        public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, [MaybeNullWhen(false)] out AnyValue result)
        {
            result = new(new string(s));
            return true;
        }

        public string ToString(string? format, IFormatProvider? formatProvider)
        {
            return Value;
        }

        public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
        {
            if (Value.AsSpan().TryCopyTo(destination))
            {
                charsWritten = Value.Length;
                return true;
            }

            charsWritten = 0;
            return false;
        }
    }
}
