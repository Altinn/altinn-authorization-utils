using Altinn.Authorization.ModelUtils.FieldValueRecords;
using Altinn.Authorization.ModelUtils.Tests.Utils;
using Altinn.Authorization.ModelUtils.Tests.Utils.Shouldly;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace Altinn.Authorization.ModelUtils.Tests.FieldValueRecords;

public class FieldValueRecordModelTests
{
    [Fact]
    public void ChildModel()
    {
        var model = FieldValueRecordModel.For<Child>();

        model.Type.ShouldBe(typeof(Child));

        var parentModel = model.Parent.ShouldNotBeNull();
        parentModel.Type.ShouldBe(typeof(Parent));

        var grandParentModel = parentModel.Parent.ShouldNotBeNull();
        grandParentModel.Type.ShouldBe(typeof(GrandParent));
        grandParentModel.Parent.ShouldBeNull();

        model.Properties(includeInherited: false).Length.ShouldBe(2);
        model.Properties(includeInherited: true).Length.ShouldBe(6);
        model.Properties(includeInherited: true).ShouldAllBe(static p => p.Type == typeof(string));

        model.Constructor.Parameters.Length.ShouldBe(0);
    }

    [Fact]
    public void BaseWithCtorPropJsonRoundtrip()
    {
        CheckRoundTrip(
            new BaseWithCtorProp("parameter") { OptionalStringInBase = default },
            """
            {
                "requiredStringInBase": "parameter"
            }
            """);

        CheckRoundTrip(
            new BaseWithCtorProp("parameter") { OptionalStringInBase = "optional" },
            """
            {
                "optionalStringInBase": "optional",
                "requiredStringInBase": "parameter"
            }
            """);
    }

    [Fact]
    public void DerivedWithSetBaseCtorPropJsonRoundtrip()
    {
        CheckRoundTrip(
            new DerivedWithSetBaseCtorProp
            {
                RequiredStringInDerived = "required-string-in-derived",
                OptionalStringInBase = "optional-string-in-base",
                OptionalStringInDerived = "optional-string-in-derived",
            },
            """
            {
                "requiredStringInBase": "base-value-set-in-constructor",
                "requiredStringInDerived": "required-string-in-derived",
                "optionalStringInBase": "optional-string-in-base",
                "optionalStringInDerived": "optional-string-in-derived"
            }
            """);

        CheckRoundTrip(
            new DerivedWithSetBaseCtorProp
            {
                RequiredStringInDerived = "required-string-in-derived",
                OptionalStringInBase = default,
                OptionalStringInDerived = default,
            },
            """
            {
                "requiredStringInBase": "base-value-set-in-constructor",
                "requiredStringInDerived": "required-string-in-derived"
            }
            """);

        CheckRoundTrip(
            new DerivedWithSetBaseCtorProp
            {
                RequiredStringInDerived = "required-string-in-derived",
                OptionalStringInBase = default,
                OptionalStringInDerived = "optional-string-in-derived",
            },
            """
            {
                "requiredStringInBase": "base-value-set-in-constructor",
                "requiredStringInDerived": "required-string-in-derived",
                "optionalStringInDerived": "optional-string-in-derived"
            }
            """);
    }

    [Fact]
    public void DerivedWithCtorPropJsonRoundtrip()
    {
        CheckRoundTrip(
            new DerivedWithCtorProp("required-string-in-base", "required-string-in-derived")
            {
                OptionalStringInBase = "optional-string-in-base",
                OptionalStringInDerived = "optional-string-in-derived",
            },
            """
            {
                "requiredStringInBase": "required-string-in-base",
                "requiredStringInDerived": "required-string-in-derived",
                "optionalStringInDerived": "optional-string-in-derived",
                "optionalStringInBase": "optional-string-in-base"
            }
            """);

        CheckRoundTrip(
            new DerivedWithCtorProp("required-string-in-base", "required-string-in-derived")
            {
                OptionalStringInBase = default,
                OptionalStringInDerived = default,
            },
            """
            {
                "requiredStringInBase": "required-string-in-base",
                "requiredStringInDerived": "required-string-in-derived"
            }
            """);
    }

    [Fact]
    public void ChildJsonRoundtrip()
    {
        CheckRoundTrip(
            new Child
            {
                RequiredStringInChild = "required-string-in-child",
                RequiredStringInParent = "required-string-in-parent",
                RequiredStringInGrandParent = "required-string-in-grand-parent",
                OptionalStringInChild = "optional-string-in-child",
                OptionalStringInParent = "optional-string-in-parent",
                OptionalStringInGrandParent = "optional-string-in-grand-parent"
            },
            """
            {
                "requiredStringInChild": "required-string-in-child",
                "requiredStringInParent": "required-string-in-parent",
                "requiredStringInGrandParent": "required-string-in-grand-parent",
                "optionalStringInChild": "optional-string-in-child",
                "optionalStringInParent": "optional-string-in-parent",
                "optionalStringInGrandParent": "optional-string-in-grand-parent"
            }
            """);

        CheckRoundTrip(
            new Child
            {
                RequiredStringInChild = "required-string-in-child",
                RequiredStringInParent = "required-string-in-parent",
                RequiredStringInGrandParent = "required-string-in-grand-parent",
                OptionalStringInChild = default,
                OptionalStringInParent = default,
                OptionalStringInGrandParent = default
            },
            """
            {
                "requiredStringInChild": "required-string-in-child",
                "requiredStringInParent": "required-string-in-parent",
                "requiredStringInGrandParent": "required-string-in-grand-parent"
            }
            """);
    }

    [Fact]
    public void CtorWithOptionalParameterJsonRoundtrips()
    {
        CheckRoundTrip(
            new CtorWithOptionalParameter()
            {
                OptionalProperty = "optional-property"
            },
            """
            {
                "optionalStringParameter": "default-value-for-optional-parameter",
                "optionalProperty": "optional-property"
            }
            """);

        CheckRoundTrip(
            new CtorWithOptionalParameter()
            {
                OptionalProperty = default
            },
            """
            {
                "optionalStringParameter": "default-value-for-optional-parameter"
            }
            """);

        CheckParses(
            """
            {
                "optionalProperty": "optional-property"
            }
            """,
            new CtorWithOptionalParameter()
            {
                OptionalProperty = "optional-property"
            });
    }

    [Fact]
    public void MissingRequiredParameter()
    {
        CheckRoundTrip(
            new BaseWithCtorProp("required-string-in-base") { OptionalStringInBase = default },
            """
            {
                "requiredStringInBase": "required-string-in-base"
            }
            """);

        Should.Throw<JsonException>(() => Json.Deserialize<BaseWithCtorProp>(
            """
            {
                "optionalStringInBase": "optional-string-in-base"
            }
            """));
    }

    [Fact]
    public void MissingRequiredProperty()
    {
        CheckRoundTrip(
            new RequiredProp { RequiredString = "required-string" },
            """
            {
                "requiredString": "required-string"
            }
            """);

        Should.Throw<JsonException>(() => Json.Deserialize<RequiredProp>(
            """
            {
            }
            """));
    }

    private void CheckRoundTrip<T>(T value, [StringSyntax(StringSyntaxAttribute.Json)] string json)
    {
        var serialized = Json.SerializeToDocument(value);

        serialized.ShouldNotBeNull();
        serialized.ShouldBeStructurallyEquivalentTo(json);

        var deserialized = Json.Deserialize<T>(serialized);
        deserialized.ShouldBe(value);
    }

    private void CheckParses<T>([StringSyntax(StringSyntaxAttribute.Json)] string json, T value)
    {
        var deserialized = Json.Deserialize<T>(json);
        deserialized.ShouldBe(value);
    }

    [FieldValueRecord]
    public record EmptyBase { }

    [FieldValueRecord]
    public record EmptyDerived : EmptyBase { }

    [FieldValueRecord]
    public record RequiredProp
    {
        public required string RequiredString { get; init; }
    }

    [FieldValueRecord]
    public record Base
    {
        public required FieldValue<string> OptionalStringInBase { get; init; }

        public required string RequiredStringInBase { get; init; }
    }

    [FieldValueRecord]
    public record Derived : Base
    {
        public required FieldValue<string> OptionalStringInDerived { get; init; }

        public required string RequiredStringInDerived { get; init; }
    }

    [FieldValueRecord]
    public record BaseWithCtorProp
    {
        public BaseWithCtorProp(string requiredStringInBase)
        {
            RequiredStringInBase = requiredStringInBase;
        }

        public required FieldValue<string> OptionalStringInBase { get; init; }

        public string RequiredStringInBase { get; }
    }

    [FieldValueRecord]
    public record DerivedWithCtorProp : BaseWithCtorProp
    {
        public DerivedWithCtorProp(string requiredStringInBase, string requiredStringInDerived)
            : base(requiredStringInBase)
        {
            RequiredStringInDerived = requiredStringInDerived;
        }

        public required FieldValue<string> OptionalStringInDerived { get; init; }

        public string RequiredStringInDerived { get; }
    }

    [FieldValueRecord]
    public record DerivedWithSetBaseCtorProp : BaseWithCtorProp
    {
        public DerivedWithSetBaseCtorProp()
            : base("base-value-set-in-constructor")
        {
        }

        public required FieldValue<string> OptionalStringInDerived { get; init; }

        public required string RequiredStringInDerived { get; init; }
    }

    [FieldValueRecord]
    public record GrandParent
    {
        public required FieldValue<string> OptionalStringInGrandParent { get; init; }

        public required string RequiredStringInGrandParent { get; init; }
    }

    [FieldValueRecord]
    public record Parent : GrandParent
    {
        public required FieldValue<string> OptionalStringInParent { get; init; }

        public required string RequiredStringInParent { get; init; }
    }

    [FieldValueRecord]
    public record Child : Parent
    {
        public required FieldValue<string> OptionalStringInChild { get; init; }

        public required string RequiredStringInChild { get; init; }
    }

    [FieldValueRecord]
    public record CtorWithOptionalParameter
    {
        public CtorWithOptionalParameter(string? optionalStringParameter = default)
        {
            OptionalStringParameter = optionalStringParameter ?? "default-value-for-optional-parameter";
        }

        public string OptionalStringParameter { get; }

        public required FieldValue<string> OptionalProperty { get; init; }
    }
}
