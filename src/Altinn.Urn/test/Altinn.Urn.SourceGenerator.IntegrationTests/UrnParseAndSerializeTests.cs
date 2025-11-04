using System.Diagnostics.CodeAnalysis;

namespace Altinn.Urn.SourceGenerator.IntegrationTests;

public partial class UrnParseAndSerializeTests
{
    [Theory]
    [InlineData("urn:altinn:person:identifier-no:12345678901", PersonUrn.Type.IdentifierNo, "altinn:person:identifier-no", "12345678901")]
    [InlineData("urn:altinn:party:id:123456", PersonUrn.Type.PartyId, "altinn:party:id", "123456")]
    [InlineData("urn:altinn:party:uuid:12345678-1234-1234-1234-123456789012", PersonUrn.Type.PartyUuid, "altinn:party:uuid", "12345678-1234-1234-1234-123456789012")]
    [InlineData("urn:altinn:person:name:Øvrebø,+Åstein+Æser", PersonUrn.Type.PersonName, "altinn:person:name", "Øvrebø,+Åstein+Æser")]
    public void GeneralUrnTests(string urn, PersonUrn.Type type, string keyString, string valueString)
    {
        Assert.True(PersonUrn.TryParse(urn, out var personUrn));
        personUrn.Urn.Should().Be(urn);
        personUrn.UrnType.Should().Be(type);

        new string(personUrn.KeySpan).Should().Be(keyString);
        new string(personUrn.ValueSpan).Should().Be(valueString);

        var formatted = $"urn:{personUrn:P}:{personUrn:S}";
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

    [Fact]
    public void UrnsCanBeHierarchical()
    {
        Assert.True(HierarchicalUrn.TryParse("urn:parent:123", out var parent));
        Assert.True(parent.IsParent(out var parentId));
        parentId.Should().Be(123);

        Assert.True(HierarchicalUrn.TryParse("urn:parent:child:456", out var child));
        Assert.True(child.IsChild(out var childId));
        childId.Should().Be(456);
    }

    [KeyValueUrn]
    public abstract partial record PersonUrn
    {
        [UrnKey("altinn:person:identifier-no")]
        public partial bool IsIdentifierNo(out PersonIdentifier personId);

        [UrnKey("altinn:organization:org-no")]
        public partial bool IsOrganizationNo(out OrgNo orgNo);

        [UrnKey("altinn:party:id")]
        public partial bool IsPartyId(out int partyId);

        [UrnKey("altinn:party:uuid")]
        public partial bool IsPartyUuid(out Guid partyUuid);

        [UrnKey("altinn:person:d-number")]
        public partial bool IsDNumber(out int dNumber);

        [UrnKey("altinn:person:name")]
        public partial bool IsPersonName(out UrnEncoded name);
    }

    [KeyValueUrn]
    public abstract partial record AnyUrn
    {
        [UrnKey("any")]
        public partial bool IsAny(out AnyValue value);
    }

    [KeyValueUrn]
    public abstract partial record HierarchicalUrn
    {
        [UrnKey("parent")]
        public partial bool IsParent(out int parent);

        [UrnKey("parent:child")]
        public partial bool IsChild(out int child);
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
