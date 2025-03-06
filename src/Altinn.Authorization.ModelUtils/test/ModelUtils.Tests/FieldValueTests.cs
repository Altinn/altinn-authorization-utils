namespace Altinn.Authorization.ModelUtils.Tests;

public class FieldValueTests
{
    [Fact]
    public void UnsetSentinel_IsUnset()
    {
        FieldValue<string> value = FieldValue.Unset;

        value.IsUnset.ShouldBeTrue();
        value.IsNull.ShouldBeFalse();
        value.HasValue.ShouldBeFalse();
    }

    [Fact]
    public void UnsetField_IsUnset()
    {
        FieldValue<string> value = FieldValue<string>.Unset;

        value.IsUnset.ShouldBeTrue();
        value.IsNull.ShouldBeFalse();
        value.HasValue.ShouldBeFalse();
    }

    [Fact]
    public void NullSentinel_IsNull()
    {
        FieldValue<string> value = FieldValue.Null;

        value.IsNull.ShouldBeTrue();
        value.IsUnset.ShouldBeFalse();
        value.HasValue.ShouldBeFalse();
    }

    [Fact]
    public void NullField_IsNull()
    {
        FieldValue<string> value = FieldValue<string>.Null;

        value.IsNull.ShouldBeTrue();
        value.IsUnset.ShouldBeFalse();
        value.HasValue.ShouldBeFalse();
    }

    [Fact]
    public void Null_IsNull()
    {
        FieldValue<string> value = null;

        value.IsNull.ShouldBeTrue();
        value.IsUnset.ShouldBeFalse();
        value.HasValue.ShouldBeFalse();
    }

    [Fact]
    public void NonNull_HasValue()
    {
        FieldValue<string> value = "Hello";

        value.HasValue.ShouldBeTrue();
        value.IsNull.ShouldBeFalse();
        value.IsUnset.ShouldBeFalse();
        value.Value.ShouldBe("Hello");
    }

    [Fact]
    public void Unset_Select_IsUnset()
    {
        FieldValue<int> value = FieldValue.Unset;
        FieldValue<string> mapped = value.Select(static i => i.ToString());

        mapped.IsUnset.ShouldBeTrue();
    }

    [Fact]
    public void Null_Select_IsNull()
    {
        FieldValue<int> value = FieldValue.Null;
        FieldValue<string> mapped = value.Select(static i => i.ToString());

        mapped.IsNull.ShouldBeTrue();
    }

    [Fact]
    public void NonNull_Select_HasValue()
    {
        FieldValue<int> value = 42;
        FieldValue<string> mapped = value.Select(static i => i.ToString());

        mapped.HasValue.ShouldBeTrue();
        mapped.Value.ShouldBe(42.ToString());
    }

    [Fact]
    public void Unset_SelectWithState_IsUnset()
    {
        FieldValue<int> value = FieldValue.Unset;
        FieldValue<int> mapped = value.Select(10, static (i, s) => i + s);

        mapped.IsUnset.ShouldBeTrue();
    }

    [Fact]
    public void Null_SelectWithState_IsNull()
    {
        FieldValue<int> value = FieldValue.Null;
        FieldValue<int> mapped = value.Select(10, static (i, s) => i + s);

        mapped.IsNull.ShouldBeTrue();
    }

    [Fact]
    public void NonNull_SelectWithState_HasValue()
    {
        FieldValue<int> value = 42;
        FieldValue<int> mapped = value.Select(10, static (i, s) => i + s);

        mapped.HasValue.ShouldBeTrue();
        mapped.Value.ShouldBe(42 + 10);
    }

    [Fact]
    public void CastToValue_ThrowsForUnset()
    {
        FieldValue<int> value = FieldValue.Unset;

        Should.Throw<InvalidOperationException>(() => _ = (int)value);
    }

    [Fact]
    public void CastToValue_ThrowsForNull() {
        FieldValue<int> value = FieldValue.Null;

        Should.Throw<InvalidOperationException>(() => _ = (int)value);
    }

    [Fact]
    public void CastToValue_ReturnsValue()
    {
        FieldValue<int> value = 42;

        ((int)value).ShouldBe(42);
    }

    [Theory]
    [MemberData(nameof(FieldValueEqualityData))]
    public void FieldValue_Equality(FieldValue<int> left, FieldValue<int> right, bool expected)
    {
        left.Equals(right).ShouldBe(expected);
        right.Equals(left).ShouldBe(expected);
        (left == right).ShouldBe(expected);
        (right == left).ShouldBe(expected);
        (left != right).ShouldBe(!expected);
        (right != left).ShouldBe(!expected);

        if (expected)
        {
            left.GetHashCode().ShouldBe(right.GetHashCode());
        }
    }

    [Fact]
    public void IsFieldValueType_FalseForNonFieldValues()
    {
        FieldValue.IsFieldValueType(typeof(int), out _).ShouldBeFalse();
    }

    [Fact]
    public void IsFieldValueType_TrueForFieldValue()
    {
        FieldValue.IsFieldValueType(typeof(FieldValue<int>), out Type? fieldType).ShouldBeTrue();
        fieldType.ShouldBe(typeof(int));
    }

    [Fact]
    public void IsFieldValueType_TrueNestedFieldValue()
    {
        FieldValue.IsFieldValueType(typeof(FieldValue<FieldValue<int>>), out Type? fieldType).ShouldBeTrue();
        fieldType.ShouldBe(typeof(FieldValue<int>));
    }

    public static TheoryData<FieldValue<int>, FieldValue<int>, bool> FieldValueEqualityData => [
            (FieldValue.Unset, FieldValue.Unset, true),
            (FieldValue.Null, FieldValue.Null, true),
            (42, 42, true),

            (FieldValue.Unset, FieldValue.Null, false),
            (FieldValue.Unset, 42, false),

            (FieldValue.Null, 42, false),

            (42, 41, false),
        ];
}
