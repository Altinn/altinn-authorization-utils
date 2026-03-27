using Altinn.Authorization.TestUtils.Shouldly;
using System.Text.Json;

namespace Altinn.Urn.SourceGenerator.IntegrationTests;

public partial class UrnDictionaryTests
{
    [Fact]
    public void NewDictionary_IsEmpty()
    {
        var dict = new KeyValueUrnDictionary<TestUrn, TestUrn.Type>();

        dict.Count.ShouldBe(0);
    }

    [Fact]
    public void Dictionary_IsReadOnly_False()
    {
        var dict = new KeyValueUrnDictionary<TestUrn, TestUrn.Type>();

        dict.IsReadOnly.ShouldBeFalse();
    }

    [Fact]
    public void EmptyDictionary_GetItem_ThrowsKeyNotFound()
    {
        var dict = new KeyValueUrnDictionary<TestUrn, TestUrn.Type>();

        Should.Throw<KeyNotFoundException>(() => _ = dict[TestUrn.Type.Organization]);
    }

    [Fact]
    public void EmptyDictionary_ContainsKey_False()
    {
        /// <inheritdoc cref="ISpanParsable{TSelf}.Parse(ReadOnlySpan{char}, IFormatProvider?)"/>
        var dict = new KeyValueUrnDictionary<TestUrn, TestUrn.Type>();

        dict.ContainsKey(TestUrn.Type.Organization).ShouldBeFalse();
    }

    [Fact]
    public void EmptyDictionary_TryGetValue_False()
    {
        var dict = new KeyValueUrnDictionary<TestUrn, TestUrn.Type>();

        dict.TryGetValue(TestUrn.Type.Organization, out var value).ShouldBeFalse();
        value.ShouldBeNull();
    }

    [Fact]
    public void AddItem_Adds_If_Not_KeyExists()
    {
        var dict = new KeyValueUrnDictionary<TestUrn, TestUrn.Type>();

        dict.Add(TestUrn.Organization.Create(123456789));
        dict.ContainsKey(TestUrn.Type.Organization).ShouldBeTrue();
    }

    [Fact]
    public void AddItem_Throws_If_KeyExists()
    {
        var dict = new KeyValueUrnDictionary<TestUrn, TestUrn.Type>();

        dict.Add(TestUrn.Organization.Create(123456789));
        Should.Throw<ArgumentException>(() => dict.Add(TestUrn.Organization.Create(987654321)));
    }

    [Fact]
    public void TryAddItem_Adds_If_Not_KeyExists()
    {
        var dict = new KeyValueUrnDictionary<TestUrn, TestUrn.Type>();

        dict.TryAdd(TestUrn.Organization.Create(123456789)).ShouldBeTrue();
        dict.ContainsKey(TestUrn.Type.Organization).ShouldBeTrue();
    }

    [Fact]
    public void TryAddItem_False_If_KeyExists()
    {
        var dict = new KeyValueUrnDictionary<TestUrn, TestUrn.Type>();

        var org1 = TestUrn.Organization.Create(123456789);
        var org2 = TestUrn.Organization.Create(987654321);

        dict.TryAdd(org1).ShouldBeTrue();
        dict.TryAdd(org2).ShouldBeFalse();

        dict.ContainsKey(TestUrn.Type.Organization).ShouldBeTrue();
        dict[TestUrn.Type.Organization].ShouldBe(org1);
    }

    [Fact]
    public void ContainsKey_True_If_KeyExists()
    {
        var dict = new KeyValueUrnDictionary<TestUrn, TestUrn.Type>();

        dict.Add(TestUrn.Organization.Create(123456789));
        dict.ContainsKey(TestUrn.Type.Organization).ShouldBeTrue();
    }

    [Fact]
    public void TryGetValue_True_If_KeyExists()
    {
        var dict = new KeyValueUrnDictionary<TestUrn, TestUrn.Type>();

        var org = TestUrn.Organization.Create(123456789);
        dict.Add(org);

        dict.TryGetValue(TestUrn.Type.Organization, out var value).ShouldBeTrue();
        value.ShouldBe(org);
    }

    [Fact]
    public void Clear_RemovesAllItems()
    {
        var dict = new KeyValueUrnDictionary<TestUrn, TestUrn.Type>();

        dict.Add(TestUrn.Organization.Create(123456789));
        dict.Add(TestUrn.Party.Create(987654321));

        dict.Clear();
        dict.Count.ShouldBe(0);
    }

    [Fact]
    public void ContainsKey_False_If_KeyDoesNotExists()
    {
        var dict = new KeyValueUrnDictionary<TestUrn, TestUrn.Type>();

        dict.ContainsKey(TestUrn.Type.Organization).ShouldBeFalse();
    }

    [Fact]
    public void TryGetValue_False_If_KeyDoesNotExists()
    {
        var dict = new KeyValueUrnDictionary<TestUrn, TestUrn.Type>();

        dict.TryGetValue(TestUrn.Type.Organization, out var value).ShouldBeFalse();
        value.ShouldBeNull();
    }

    [Fact]
    public void ContainsItem_True_If_ItemExists()
    {
        var dict = new KeyValueUrnDictionary<TestUrn, TestUrn.Type>();

        TestUrn org = TestUrn.Organization.Create(123456789);
        dict.Add(org);

        ((ICollection<TestUrn>)dict).Contains(org).ShouldBeTrue();
        ((ICollection<KeyValuePair<TestUrn.Type, TestUrn>>)dict).Contains(KeyValuePair.Create(org.UrnType, org)).ShouldBeTrue();
    }

    [Fact]
    public void JsonSerialization()
    {
        KeyValueUrnDictionary<TestUrn, TestUrn.Type> dict = [
            TestUrn.Organization.Create(123456789),
            TestUrn.Party.Create(42),
            TestUrn.OrganizationRole.Create(29),
        ];

        var actualString = JsonSerializer.Serialize(dict);
        var expectedString =
            """
            {
                "urn:altinn:organization:identifier": 123456789,
                "urn:altinn:organization:org-no": 123456789,
                "urn:altinn:party:identifier": 42,
                "urn:altinn:organization:role": 29
            }
            """;

        using var actual = JsonDocument.Parse(actualString);
        actual.ShouldBeStructurallyEquivalentTo(expectedString);
    }

    [Fact]
    public void JsonDeserialization()
    {
        var json =
            """
            {
                "urn:altinn:organization:identifier": 123456789,
                "urn:altinn:party:identifier": 42,
                "urn:altinn:organization:role": 29
            }
            """;

        var dict = JsonSerializer.Deserialize<KeyValueUrnDictionary<TestUrn, TestUrn.Type>>(json);
        dict.ShouldNotBeNull();
        dict.Count.ShouldBe(3);
        dict.ContainsKey(TestUrn.Type.Organization).ShouldBeTrue();
        dict[TestUrn.Type.Organization].ShouldBe(TestUrn.Organization.Create(123456789));
        dict.ContainsKey(TestUrn.Type.Party).ShouldBeTrue();
        dict[TestUrn.Type.Party].ShouldBe(TestUrn.Party.Create(42));
        dict.ContainsKey(TestUrn.Type.OrganizationRole).ShouldBeTrue();
        dict[TestUrn.Type.OrganizationRole].ShouldBe(TestUrn.OrganizationRole.Create(29));
    }

    [KeyValueUrn]
    public abstract partial record TestUrn
    {
        [UrnKey("altinn:organization:identifier", Canonical = true)]
        [UrnKey("altinn:organization:org-no")]
        public partial bool IsOrganization(out int value);

        [UrnKey("altinn:organization:role")]
        public partial bool IsOrganizationRole(out int value);

        [UrnKey("altinn:party:identifier")]
        public partial bool IsParty(out int value);
    }
}
