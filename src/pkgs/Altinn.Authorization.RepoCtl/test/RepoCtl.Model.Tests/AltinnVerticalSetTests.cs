using System.Collections.Immutable;
using Semver;

namespace Altinn.Authorization.RepoCtl.Model.Tests;

public class AltinnVerticalSetTests
{
    [Fact]
    public void Create_WithNoVerticals_ReturnsEmptySet()
    {
        var verticals = ImmutableArray.CreateBuilder<AltinnVertical>();

        var set = AltinnVerticalSet.Create(verticals);

        set.AsEnumerable().ShouldBeEmpty();
    }

    [Fact]
    public void Create_WithOneVertical_ReturnsThatVertical()
    {
        var vertical = CreateVertical("app:Alpha");
        var verticals = ImmutableArray.CreateBuilder<AltinnVertical>();
        verticals.Add(vertical);

        var set = AltinnVerticalSet.Create(verticals);

        set.AsEnumerable().ShouldHaveSingleItem().ShouldBeSameAs(vertical);
    }

    [Fact]
    public void Create_WithDuplicateIds_ReturnsSortedDistinctVerticals()
    {
        var verticals = ImmutableArray.CreateBuilder<AltinnVertical>();
        AltinnVertical[] input = [
            CreateVertical("tool:Zulu"),
            CreateVertical("app:Beta"),
            CreateVertical("lib:Library"),
            CreateVertical("app:Alpha"),
            CreateVertical("app:Beta"),
            CreateVertical("pkg:Package"),
            CreateVertical("app:Alpha"),
            CreateVertical("app:Alpha"),
            CreateVertical("tool:Zulu"),
        ];
        verticals.AddRange(input);
        var expected = input.Select(static vertical => vertical.Id).Distinct().Order().ToArray();

        var set = AltinnVerticalSet.Create(verticals);

        set.AsEnumerable().Select(static vertical => vertical.Id).ShouldBe(expected);
    }

    private static AltinnVertical CreateVertical(string id)
        => new(
            new DirectoryInfo("."),
            AltinnVerticalId.Parse(id, provider: null),
            new SemVersion(1, 0, 0),
            [],
            AltinnVerticalConfiguration.Default);
}
