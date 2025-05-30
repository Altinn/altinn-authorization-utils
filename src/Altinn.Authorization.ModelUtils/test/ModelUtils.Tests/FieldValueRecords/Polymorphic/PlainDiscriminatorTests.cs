﻿using Altinn.Authorization.ModelUtils.Tests.Utils;
using Altinn.Authorization.ModelUtils.Tests.Utils.Shouldly;

namespace Altinn.Authorization.ModelUtils.Tests.FieldValueRecords.Polymorphic;

public class PlainDiscriminatorTests
{
    [Fact]
    public void LeftChild_AsBase_RoundTrips()
    {
        Base sut = new LeftChild
        {
            RequiredBaseProperty = "required-base",
            OptionalBaseProperty = "optional-base",
            RequiredLeftChildProperty = "required-left",
            OptionalLeftChildProperty = "optional-left",
        };

        sut.ShouldJsonRoundTripAs(
            """
                {
                  "type": "left-child",
                  "requiredBaseProperty": "required-base",
                  "optionalBaseProperty": "optional-base",
                  "requiredLeftChildProperty": "required-left",
                  "optionalLeftChildProperty": "optional-left"
                }
                """);
    }

    [Fact]
    public void LeftChild_AsLeftChild_RoundTrips()
    {
        LeftChild sut = new LeftChild
        {
            RequiredBaseProperty = "required-base",
            OptionalBaseProperty = "optional-base",
            RequiredLeftChildProperty = "required-left",
            OptionalLeftChildProperty = "optional-left",
        };

        sut.ShouldJsonRoundTripAs(
            """
                {
                  "type": "left-child",
                  "requiredBaseProperty": "required-base",
                  "optionalBaseProperty": "optional-base",
                  "requiredLeftChildProperty": "required-left",
                  "optionalLeftChildProperty": "optional-left"
                }
                """);
    }

    [Theory]
    [InlineData(VariantType.RightChild1)]
    [InlineData(VariantType.RightChild2)]
    public void RightChild_AsBase_RoundTrips(VariantType variantType)
    {
        Base sut = new RightChild(variantType)
        {
            RequiredBaseProperty = "required-base",
            OptionalBaseProperty = "optional-base",
            RequiredRightChildProperty = "required-right",
            OptionalRightChildProperty = "optional-right",
        };

        sut.ShouldJsonRoundTripAs(
            $$"""
                {
                  "type": {{Json.SerializeToString(variantType)}},
                  "requiredBaseProperty": "required-base",
                  "optionalBaseProperty": "optional-base",
                  "requiredRightChildProperty": "required-right",
                  "optionalRightChildProperty": "optional-right"
                }
                """);
    }

    [Theory]
    [InlineData(VariantType.RightChild1)]
    [InlineData(VariantType.RightChild2)]
    public void RightChild_AsRightChild_RoundTrips(VariantType variantType)
    {
        RightChild sut = new RightChild(variantType)
        {
            RequiredBaseProperty = "required-base",
            OptionalBaseProperty = "optional-base",
            RequiredRightChildProperty = "required-right",
            OptionalRightChildProperty = "optional-right",
        };

        sut.ShouldJsonRoundTripAs(
            $$"""
                {
                  "type": {{Json.SerializeToString(variantType)}},
                  "requiredBaseProperty": "required-base",
                  "optionalBaseProperty": "optional-base",
                  "requiredRightChildProperty": "required-right",
                  "optionalRightChildProperty": "optional-right"
                }
                """);
    }

    [Fact]
    public void RightGrandChild_AsBase_RoundTrips()
    {
        Base sut = new RightGrandChild(VariantType.RightGrandChild)
        {
            RequiredBaseProperty = "required-base",
            OptionalBaseProperty = "optional-base",
            RequiredRightChildProperty = "required-right",
            OptionalRightChildProperty = "optional-right",
            RequiredRightGrandChildProperty = "required-grandchild",
            OptionalRightGrandChildProperty = "optional-grandchild",
        };

        sut.ShouldJsonRoundTripAs(
            """
                {
                  "type": "right-grand-child",
                  "requiredBaseProperty": "required-base",
                  "optionalBaseProperty": "optional-base",
                  "requiredRightChildProperty": "required-right",
                  "optionalRightChildProperty": "optional-right",
                  "requiredRightGrandChildProperty": "required-grandchild",
                  "optionalRightGrandChildProperty": "optional-grandchild"
                }
                """);
    }

    [Fact]
    public void RightGrandChild_AsRightChild_RoundTrips()
    {
        RightChild sut = new RightGrandChild(VariantType.RightGrandChild)
        {
            RequiredBaseProperty = "required-base",
            OptionalBaseProperty = "optional-base",
            RequiredRightChildProperty = "required-right",
            OptionalRightChildProperty = "optional-right",
            RequiredRightGrandChildProperty = "required-grandchild",
            OptionalRightGrandChildProperty = "optional-grandchild",
        };

        sut.ShouldJsonRoundTripAs(
            """
                {
                  "type": "right-grand-child",
                  "requiredBaseProperty": "required-base",
                  "optionalBaseProperty": "optional-base",
                  "requiredRightChildProperty": "required-right",
                  "optionalRightChildProperty": "optional-right",
                  "requiredRightGrandChildProperty": "required-grandchild",
                  "optionalRightGrandChildProperty": "optional-grandchild"
                }
                """);
    }

    [Fact]
    public void RightGrandChild_AsRightGrandChild_RoundTrips()
    {
        RightGrandChild sut = new RightGrandChild(VariantType.RightGrandChild)
        {
            RequiredBaseProperty = "required-base",
            OptionalBaseProperty = "optional-base",
            RequiredRightChildProperty = "required-right",
            OptionalRightChildProperty = "optional-right",
            RequiredRightGrandChildProperty = "required-grandchild",
            OptionalRightGrandChildProperty = "optional-grandchild",
        };

        sut.ShouldJsonRoundTripAs(
            """
                {
                  "type": "right-grand-child",
                  "requiredBaseProperty": "required-base",
                  "optionalBaseProperty": "optional-base",
                  "requiredRightChildProperty": "required-right",
                  "optionalRightChildProperty": "optional-right",
                  "requiredRightGrandChildProperty": "required-grandchild",
                  "optionalRightGrandChildProperty": "optional-grandchild"
                }
                """);
    }

    [PolymorphicFieldValueRecord(IsRoot = true)]
    [PolymorphicDerivedType(typeof(LeftChild), VariantType.LeftChild)]
    [PolymorphicDerivedType(typeof(RightChild), VariantType.RightChild1)]
    [PolymorphicDerivedType(typeof(RightChild), VariantType.RightChild2)]
    [PolymorphicDerivedType(typeof(RightGrandChild), VariantType.RightGrandChild)]
    public abstract record Base
    {
        public Base(VariantType type)
        {
            Type = type;
        }

        [PolymorphicDiscriminatorProperty]
        public VariantType Type { get; }

        public required string RequiredBaseProperty { get; init; }

        public required FieldValue<string> OptionalBaseProperty { get; init; }
    }

    [PolymorphicFieldValueRecord]
    public record LeftChild()
        : Base(VariantType.LeftChild)
    {
        public required string RequiredLeftChildProperty { get; init; }

        public required FieldValue<string> OptionalLeftChildProperty { get; init; }
    }

    [PolymorphicFieldValueRecord]
    public record RightChild
        : Base
    {
        public RightChild(VariantType type)
            : base(type)
        {
        }

        public required string RequiredRightChildProperty { get; init; }

        public required FieldValue<string> OptionalRightChildProperty { get; init; }
    }

    [PolymorphicFieldValueRecord]
    public record RightGrandChild(VariantType type)
        : RightChild(type)
    {
        public required string RequiredRightGrandChildProperty { get; init; }

        public required FieldValue<string> OptionalRightGrandChildProperty { get; init; }
    }
}
