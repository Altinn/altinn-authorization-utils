using Altinn.Urn.SourceGenerator.IntegrationTests.Utils;
using FluentAssertions;
using System.Text.Json;

namespace Altinn.Urn.SourceGenerator.IntegrationTests;

public partial class UrnDictionaryTests
{
    [Fact]
    public void NewDictionary_IsEmpty()
    {
        var dict = new KeyValueUrnDictionary<TestUrn, TestUrn.Type>();

        dict.Should().BeEmpty();
    }

    [Fact]
    public void Dictionary_IsReadOnly_False()
    {
        var dict = new KeyValueUrnDictionary<TestUrn, TestUrn.Type>();

        dict.IsReadOnly.Should().BeFalse();
    }

    [Fact]
    public void EmptyDictionary_GetItem_ThrowsKeyNotFound()
    {
        var dict = new KeyValueUrnDictionary<TestUrn, TestUrn.Type>();

        dict.Invoking(d => d[TestUrn.Type.Organization])
            .Should().Throw<KeyNotFoundException>();
    }

    [Fact]
    public void EmptyDictionary_ContainsKey_False()
    {
        var dict = new KeyValueUrnDictionary<TestUrn, TestUrn.Type>();

        dict.ContainsKey(TestUrn.Type.Organization).Should().BeFalse();
    }

    [Fact]
    public void EmptyDictionary_TryGetValue_False()
    {
        var dict = new KeyValueUrnDictionary<TestUrn, TestUrn.Type>();

        dict.TryGetValue(TestUrn.Type.Organization, out var value).Should().BeFalse();
        value.Should().BeNull();
    }

    [Fact]
    public void AddItem_Adds_If_Not_KeyExists()
    {
        var dict = new KeyValueUrnDictionary<TestUrn, TestUrn.Type>();

        dict.Add(TestUrn.Organization.Create(123456789));
        dict.Should().ContainKey(TestUrn.Type.Organization);
    }

    [Fact]
    public void AddItem_Throws_If_KeyExists()
    {
        var dict = new KeyValueUrnDictionary<TestUrn, TestUrn.Type>();

        dict.Add(TestUrn.Organization.Create(123456789));
        dict.Invoking(d => d.Add(TestUrn.Organization.Create(987654321)))
            .Should().Throw<ArgumentException>();
    }

    [Fact]
    public void TryAddItem_Adds_If_Not_KeyExists()
    {
        var dict = new KeyValueUrnDictionary<TestUrn, TestUrn.Type>();

        dict.TryAdd(TestUrn.Organization.Create(123456789)).Should().BeTrue();
        dict.Should().ContainKey(TestUrn.Type.Organization);
    }

    [Fact]
    public void TryAddItem_False_If_KeyExists()
    {
        var dict = new KeyValueUrnDictionary<TestUrn, TestUrn.Type>();

        var org1 = TestUrn.Organization.Create(123456789);
        var org2 = TestUrn.Organization.Create(987654321);

        dict.TryAdd(org1).Should().BeTrue();
        dict.TryAdd(org2).Should().BeFalse();

        dict.Should().ContainKey(TestUrn.Type.Organization)
            .WhoseValue.Should().Be(org1);
    }

    [Fact]
    public void ContainsKey_True_If_KeyExists()
    {
        var dict = new KeyValueUrnDictionary<TestUrn, TestUrn.Type>();

        dict.Add(TestUrn.Organization.Create(123456789));
        dict.ContainsKey(TestUrn.Type.Organization).Should().BeTrue();
    }

    [Fact]
    public void TryGetValue_True_If_KeyExists()
    {
        var dict = new KeyValueUrnDictionary<TestUrn, TestUrn.Type>();

        var org = TestUrn.Organization.Create(123456789);
        dict.Add(org);

        dict.TryGetValue(TestUrn.Type.Organization, out var value).Should().BeTrue();
        value.Should().Be(org);
    }

    [Fact]
    public void Clear_RemovesAllItems()
    {
        var dict = new KeyValueUrnDictionary<TestUrn, TestUrn.Type>();

        dict.Add(TestUrn.Organization.Create(123456789));
        dict.Add(TestUrn.Party.Create(987654321));

        dict.Clear();
        dict.Should().BeEmpty();
    }

    [Fact]
    public void ContainsKey_False_If_KeyDoesNotExists()
    {
        var dict = new KeyValueUrnDictionary<TestUrn, TestUrn.Type>();

        dict.ContainsKey(TestUrn.Type.Organization).Should().BeFalse();
    }

    [Fact]
    public void TryGetValue_False_If_KeyDoesNotExists()
    {
        var dict = new KeyValueUrnDictionary<TestUrn, TestUrn.Type>();

        dict.TryGetValue(TestUrn.Type.Organization, out var value).Should().BeFalse();
        value.Should().BeNull();
    }

    [Fact]
    public void ContainsItem_True_If_ItemExists()
    {
        var dict = new KeyValueUrnDictionary<TestUrn, TestUrn.Type>();

        TestUrn org = TestUrn.Organization.Create(123456789);
        dict.Add(org);

        ((ICollection<TestUrn>)dict).Contains(org).Should().BeTrue();
        ((ICollection<KeyValuePair<TestUrn.Type, TestUrn>>)dict).Contains(KeyValuePair.Create(org.UrnType, org)).Should().BeTrue();
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
                "urn:altinn:party:identifier": 42,
                "urn:altinn:organization:role": 29
            }
            """;

        var actual = JsonDocument.Parse(actualString);
        var expected = JsonDocument.Parse(expectedString);

        actual.Should().BeEquivalentTo(expected, opt => opt.Using(JsonEqualityComparer.Instance));
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
        dict.Should().HaveCount(3);
        dict.Should().ContainKey(TestUrn.Type.Organization)
            .WhoseValue.Should().Be(TestUrn.Organization.Create(123456789));
        dict.Should().ContainKey(TestUrn.Type.Party)
            .WhoseValue.Should().Be(TestUrn.Party.Create(42));
        dict.Should().ContainKey(TestUrn.Type.OrganizationRole)
            .WhoseValue.Should().Be(TestUrn.OrganizationRole.Create(29));
    }

    [KeyValueUrn]
    public abstract partial record TestUrn
    {
        [UrnKey("altinn:organization:identifier")]
        public partial bool IsOrganization(out int value);

        [UrnKey("altinn:organization:role")]
        public partial bool IsOrganizationRole(out int value);

        [UrnKey("altinn:party:identifier")]
        public partial bool IsParty(out int value);
    }
}
