using Altinn.Authorization.ModelUtils.Tests.Utils;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Altinn.Authorization.ModelUtils.Tests;

public class NonExhaustiveFlagsEnumTests
{
    [Fact]
    public void Enum_ImplicitlyConverts()
    {
        var value = FlagsEnum.Second | FlagsEnum.ValueTheThird;
        NonExhaustiveFlagsEnum<FlagsEnum> nonExhaustive = value;

        nonExhaustive.ShouldBe(value);
        nonExhaustive.IsWellKnown.ShouldBeTrue();
        nonExhaustive.HasUnknownValues.ShouldBeFalse();
    }

    [Fact]
    public void WellKnownValue_ExplicitlyCastsToEnum_DoesNotThrow()
    {
        NonExhaustiveFlagsEnum<FlagsEnum> nonExhaustive = FlagsEnum.Second | FlagsEnum.ValueTheThird;
        var value = Should.NotThrow(() => (FlagsEnum)nonExhaustive);

        value.ShouldBe(FlagsEnum.Second | FlagsEnum.ValueTheThird);
    }

    [Fact]
    public void WellKnownValue_UnknownValues_IsEmpty()
    {
        NonExhaustiveFlagsEnum<FlagsEnum> nonExhaustive = FlagsEnum.Second | FlagsEnum.ValueTheThird;

        nonExhaustive.UnknownValues.ShouldBeEmpty();
    }

    [Fact]
    public void UnknownValuesOnly_ExplicitlyCastsToEnum_Throws()
    {
        NonExhaustiveFlagsEnum<FlagsEnum> nonExhaustive = Json.Deserialize<NonExhaustiveFlagsEnum<FlagsEnum>>("""["not-a-value"]""");
        
        Should.Throw<InvalidCastException>(() => (FlagsEnum)nonExhaustive);
    }

    [Fact]
    public void PartialKnownValues_ExplicitlyCastsToEnum_Throws()
    {
        NonExhaustiveFlagsEnum<FlagsEnum> nonExhaustive = Json.Deserialize<NonExhaustiveFlagsEnum<FlagsEnum>>("""["first-value", "not-a-value"]""");
        
        Should.Throw<InvalidCastException>(() => (FlagsEnum)nonExhaustive);
    }

    [Fact]
    public void WellKnownValue_Value_DoesNotThrow()
    {
        NonExhaustiveFlagsEnum<FlagsEnum> nonExhaustive = FlagsEnum.Second | FlagsEnum.ValueTheThird;
        var value = nonExhaustive.Value;

        value.ShouldBe(FlagsEnum.Second | FlagsEnum.ValueTheThird);
    }

    [Fact]
    public void UnknownValuesOnly_HasUnknownValues()
    {
        NonExhaustiveFlagsEnum<FlagsEnum> nonExhaustive = Json.Deserialize<NonExhaustiveFlagsEnum<FlagsEnum>>("""["not-a-value"]""");

        nonExhaustive.HasUnknownValues.ShouldBeTrue();
        nonExhaustive.UnknownValues.ShouldBe(["not-a-value"], ignoreOrder: true);
    }

    [Fact]
    public void PartialKnownValues_HasUnknownValues()
    {
        NonExhaustiveFlagsEnum<FlagsEnum> nonExhaustive = Json.Deserialize<NonExhaustiveFlagsEnum<FlagsEnum>>("""["first-value", "not-a-value"]""");

        nonExhaustive.HasUnknownValues.ShouldBeTrue();
        nonExhaustive.UnknownValues.ShouldBe(["not-a-value"], ignoreOrder: true);
    }

    [Fact]
    public void UnknownValues_DuplicatesAreRemoved()
    {
        NonExhaustiveFlagsEnum<FlagsEnum> nonExhaustive = Json.Deserialize<NonExhaustiveFlagsEnum<FlagsEnum>>("""["not-a-value", "super-invalid", "not-a-value"]""");

        nonExhaustive.HasUnknownValues.ShouldBeTrue();
        nonExhaustive.UnknownValues.ShouldBe(["not-a-value", "super-invalid"], ignoreOrder: true);
    }

    [Fact]
    public void PartialKnownValues_HasValue()
    {
        NonExhaustiveFlagsEnum<FlagsEnum> nonExhaustive = Json.Deserialize<NonExhaustiveFlagsEnum<FlagsEnum>>("""["first-value", "not-a-value"]""");

        nonExhaustive.PartialValue.ShouldBe(FlagsEnum.FirstValue);
    }

    [Fact]
    public void None_IsNone()
    {
        NonExhaustiveFlagsEnum<FlagsEnum> nonExhaustive = FlagsEnum.None;

        nonExhaustive.IsNone.ShouldBeTrue();
    }

    [Fact]
    public void EmptyArray_ParsesToNone()
    {
        NonExhaustiveFlagsEnum<FlagsEnum> nonExhaustive = Json.Deserialize<NonExhaustiveFlagsEnum<FlagsEnum>>("""[]""");

        nonExhaustive.IsNone.ShouldBeTrue();
    }

    [Fact]
    public void None_IsValidJson()
    {
        NonExhaustiveFlagsEnum<FlagsEnum> nonExhaustive = Json.Deserialize<NonExhaustiveFlagsEnum<FlagsEnum>>("""["none"]""");

        nonExhaustive.IsNone.ShouldBeTrue();
    }

#pragma warning disable CS1718 // Comparison made to same variable
    [Fact]
    public void Equality()
    {
        NonExhaustiveFlagsEnum<FlagsEnum> known3 = FlagsEnum.ValueTheThird;
        NonExhaustiveFlagsEnum<FlagsEnum> unknown = Json.Deserialize<NonExhaustiveFlagsEnum<FlagsEnum>>("""["not-a-value"]""");
        NonExhaustiveFlagsEnum<FlagsEnum> partial3 = Json.Deserialize<NonExhaustiveFlagsEnum<FlagsEnum>>("""["value-the-third", "not-a-value"]""");
        NonExhaustiveFlagsEnum<FlagsEnum> partial3Other = Json.Deserialize<NonExhaustiveFlagsEnum<FlagsEnum>>("""["value-the-third", "other-not-a-value"]""");

        known3.Equals(known3).ShouldBeTrue();
        known3.Equals(FlagsEnum.ValueTheThird).ShouldBeTrue();
        known3.Equals(FlagsEnum.FirstValue).ShouldBeFalse();
        known3.Equals(unknown).ShouldBeFalse();
        known3.Equals(partial3).ShouldBeFalse();

        unknown.Equals(known3).ShouldBeFalse();
        unknown.Equals(FlagsEnum.ValueTheThird).ShouldBeFalse();
        unknown.Equals(FlagsEnum.FirstValue).ShouldBeFalse();
        unknown.Equals(unknown).ShouldBeTrue();
        unknown.Equals(partial3).ShouldBeFalse();

        known3.Equals((object)known3).ShouldBeTrue();
        known3.Equals((object)FlagsEnum.ValueTheThird).ShouldBeTrue();
        known3.Equals((object)FlagsEnum.FirstValue).ShouldBeFalse();
        known3.Equals((object)unknown).ShouldBeFalse();
        known3.Equals((object)partial3).ShouldBeFalse();

        unknown.Equals((object)known3).ShouldBeFalse();
        unknown.Equals((object)FlagsEnum.ValueTheThird).ShouldBeFalse();
        unknown.Equals((object)FlagsEnum.FirstValue).ShouldBeFalse();
        unknown.Equals((object)unknown).ShouldBeTrue();
        unknown.Equals((object)partial3).ShouldBeFalse();

        (known3 == known3).ShouldBeTrue();
        (known3 == FlagsEnum.ValueTheThird).ShouldBeTrue();
        (known3 == FlagsEnum.FirstValue).ShouldBeFalse();
        (known3 == unknown).ShouldBeFalse();
        (known3 == partial3).ShouldBeFalse();

        (unknown == known3).ShouldBeFalse();
        (unknown == FlagsEnum.ValueTheThird).ShouldBeFalse();
        (unknown == FlagsEnum.FirstValue).ShouldBeFalse();
        (unknown == unknown).ShouldBeTrue();
        (unknown == partial3).ShouldBeFalse();

        (known3 != known3).ShouldBeFalse();
        (known3 != FlagsEnum.ValueTheThird).ShouldBeFalse();
        (known3 != FlagsEnum.FirstValue).ShouldBeTrue();
        (known3 != unknown).ShouldBeTrue();
        (known3 != partial3).ShouldBeTrue();

        (unknown != known3).ShouldBeTrue();
        (unknown != FlagsEnum.ValueTheThird).ShouldBeTrue();
        (unknown != FlagsEnum.FirstValue).ShouldBeTrue();
        (unknown != unknown).ShouldBeFalse();
        (unknown != partial3).ShouldBeTrue();

        (partial3 == partial3).ShouldBeTrue();
        (partial3 == partial3Other).ShouldBeFalse();
    }
#pragma warning restore CS1718 // Comparison made to same variable

    [Flags]
    [StringEnumConverter(namingPolicy: JsonKnownNamingPolicy.KebabCaseLower)]
    private enum FlagsEnum
    {
        None = 0,
        FirstValue = 1 << 0,
        Second = 1 << 1,
        ValueTheThird = 1 << 2,
    }
}
