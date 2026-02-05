
namespace Altinn.Authorization.ProblemDetails.Tests;

public class ProblemExtensionDataBuilderTests
    : CollectionTests<KeyValuePair<string, string>, ProblemExtensionDataBuilder, ProblemExtensionDataBuilder.Enumerator>
{
    private uint _nextId = 0;

    protected override ProblemExtensionDataBuilder Create(ReadOnlySpan<KeyValuePair<string, string>> items)
    {
        var builder = ProblemExtensionData.CreateBuilder();

        foreach (ref readonly var item in items)
        {
            builder.Add(item.Key, item.Value);
        }

        return builder;
    }

    protected override KeyValuePair<string, string> CreateDistinctItem()
    {
        var id = _nextId++;

        return new($"key-{id:D5}", $"value-{id:D5}");
    }

    protected override ProblemExtensionDataBuilder.Enumerator GetEnumerator(ProblemExtensionDataBuilder list)
        => list.GetEnumerator();

    [Theory]
    [MemberData(nameof(Counts))]
    public void Indexer_Returns_CorrectValue(int count)
    {
        var list = Create(count);

        var newItem = CreateDistinctItem();
        Should.Throw<KeyNotFoundException>(() => list[newItem.Key]);

        list.Add(newItem.Key, newItem.Value);
        list[newItem.Key].ShouldBe(newItem.Value);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    public void Indexer_CanModifyValue(int index)
    {
        var list = Create(5);
        var key = list.AsEnumerable().ElementAt(index).Key;

        list[key] = "new-value";
        list[key].ShouldBe("new-value");
    }

    [Fact]
    public void Indexer_CanAddValue()
    {
        var list = Create(5);
        list["new-key"] = "new-value";
        list.Count.ShouldBe(6);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    public void Remove_ByKey_WorksAsExpected(int index)
    {
        var list = Create(5);
        var key = list.AsEnumerable().ElementAt(index).Key;

        list.ShouldContainKey(key);
        list.Remove(key).ShouldBeTrue();
        list.ShouldNotContainKey(key);
        list.Remove(key).ShouldBeFalse();
    }
}
