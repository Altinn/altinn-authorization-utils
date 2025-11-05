using Altinn.Authorization.ServiceDefaults.Authorization.Scopes;

namespace Altinn.Authorization.ServiceDefaults.Tests;

public class ScopeSearchValuesTests
{
    [Fact]
    public void CannotBeEmpty()
    {
        Should.Throw<ArgumentException>(() => ScopeSearchValues.Create([]));
    }

    [Theory]
    [MemberData(nameof(ValidLists))]
    public void Index(string[] items)
    {
        var sut = ScopeSearchValues.Create(items);

        for (int i = 0; i < items.Length; i++)
        {
            sut[i].ShouldBe(items[i]);
        }
    }

    [Theory]
    [MemberData(nameof(ValidLists))]
    public void Count(string[] items)
    {
        var sut = ScopeSearchValues.Create(items);

        sut.Count.ShouldBe(items.Length);
    }

    [Theory]
    [MemberData(nameof(ValidLists))]
    public void Enumerator(string[] items)
    {
        var sut = ScopeSearchValues.Create(items);
        int index = 0;

        foreach (var item in sut)
        {
            item.ShouldBe(items[index]);
            index++;
        }
    }

    [Theory]
    [MemberData(nameof(ValidLists))]
    public void Check_Empty_False(string[] items)
    {
        var sut = ScopeSearchValues.Create(items);

        sut.Check(string.Empty).ShouldBeFalse();
    }

    [Theory]
    [MemberData(nameof(ValidLists))]
    public void Check_NotInSet_False(string[] items)
    {
        var sut = ScopeSearchValues.Create(items);

        sut.Check("xyz").ShouldBeFalse();
    }

    [Theory]
    [MemberData(nameof(ValidLists))]
    public void Check_ExactString_True(string[] items)
    {
        var sut = ScopeSearchValues.Create(items);

        foreach (var item in sut)
        {
            sut.Check(item).ShouldBeTrue();
        }
    }

    [Theory]
    [MemberData(nameof(ValidLists))]
    public void Check_StartOfString_True(string[] items)
    {
        var sut = ScopeSearchValues.Create(items);

        foreach (var item in sut)
        {
            sut.Check($"{item} xyz").ShouldBeTrue();
        }
    }

    [Theory]
    [MemberData(nameof(ValidLists))]
    public void Check_EndOfString_True(string[] items)
    {
        var sut = ScopeSearchValues.Create(items);

        foreach (var item in sut)
        {
            sut.Check($"xyz {item}").ShouldBeTrue();
        }
    }

    [Theory]
    [MemberData(nameof(ValidLists))]
    public void Check_MiddleOfString_True(string[] items)
    {
        var sut = ScopeSearchValues.Create(items);

        foreach (var item in sut)
        {
            sut.Check($"xyz {item} zyx").ShouldBeTrue();
        }
    }

    [Theory]
    [MemberData(nameof(ValidLists))]
    public void Check_MiddleOfString_Duplicated_True(string[] items)
    {
        var sut = ScopeSearchValues.Create(items);

        foreach (var item in sut)
        {
            sut.Check($"xyz {item} {item} zyx").ShouldBeTrue();
        }
    }

    [Theory]
    [MemberData(nameof(ValidLists))]
    public void Check_Substring_False(string[] items)
    {
        var sut = ScopeSearchValues.Create(items);

        foreach (var item in sut)
        {
            sut.Check($"xyz{item} {item}{item} {item}xyz xyz{item}xyz").ShouldBeFalse();
        }
    }

    public static TheoryData<string[]> ValidLists => new()
    {
        { ["abc"] },
        { ["def"] },
        { ["abc", "def"] },
        { ["abc", "abcdef", "def"] },
        { ["a", "b", "c", "d", "e", "f"] },
    };
}
