using Altinn.Swashbuckle.Utils;

namespace Altinn.Swashbuckle.Tests;

public class ComparisonTests
{
    [Fact]
    public void Equals_Self()
    {
        Check(Comparison.LessThan);
        Check(Comparison.Equal);
        Check(Comparison.GreaterThan);

        static void Check(Comparison comparison)
        {
            comparison.ShouldBe(comparison);
            comparison.Equals(comparison).ShouldBeTrue();
            comparison.GetHashCode().ShouldBe(comparison.GetHashCode());
        }
    }

    [Fact]
    public void NotEquals_Other()
    {
        Comparison.GreaterThan.ShouldNotBe(Comparison.Equal);
        Comparison.GreaterThan.ShouldNotBe(Comparison.LessThan);

        Comparison.Equal.ShouldNotBe(Comparison.GreaterThan);
        Comparison.Equal.ShouldNotBe(Comparison.LessThan);

        Comparison.LessThan.ShouldNotBe(Comparison.GreaterThan);
        Comparison.LessThan.ShouldNotBe(Comparison.Equal);
    }

    [Fact]
    public void FromInt()
    {
        Comparison.From(-1).ShouldBe(Comparison.LessThan);
        Comparison.From(0).ShouldBe(Comparison.Equal);
        Comparison.From(1).ShouldBe(Comparison.GreaterThan);
    }

    [Fact]
    public void LessThan_Then()
    {
        Comparison.LessThan.Then(0, 1).ShouldBe(Comparison.LessThan);
        Comparison.LessThan.Then(0, 0).ShouldBe(Comparison.LessThan);
        Comparison.LessThan.Then(1, 0).ShouldBe(Comparison.LessThan);
    }

    [Fact]
    public void GreaterThan_Then()
    {
        Comparison.GreaterThan.Then(0, 1).ShouldBe(Comparison.GreaterThan);
        Comparison.GreaterThan.Then(0, 0).ShouldBe(Comparison.GreaterThan);
        Comparison.GreaterThan.Then(1, 0).ShouldBe(Comparison.GreaterThan);
    }

    [Fact]
    public void Equal_Then()
    {
        Comparison.Equal.Then(0, 1).ShouldBe(Comparison.LessThan);
        Comparison.Equal.Then(0, 0).ShouldBe(Comparison.Equal);
        Comparison.Equal.Then(1, 0).ShouldBe(Comparison.GreaterThan);
    }
}
