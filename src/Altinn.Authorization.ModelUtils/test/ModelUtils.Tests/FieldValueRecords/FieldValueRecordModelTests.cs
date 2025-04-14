﻿using Altinn.Authorization.ModelUtils.FieldValueRecords;
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
    public void ReadWriteOnlyPropertiesModel()
    {
        var model = FieldValueRecordModel.For<Properties>();

        model.Type.ShouldBe(typeof(Properties));
        model.Parent.ShouldBeNull();

        var props = model.Properties();
        props.Length.ShouldBe(6);

        var readOnlyProp = props.ShouldContainSingle(static p => p.Name == nameof(Properties.ReadOnlyProperty))
            .ShouldBeAssignableTo<IFieldValueRecordPropertyModel<Properties, string>>();
        readOnlyProp.ShouldSatisfyAllConditions([
            static p => p.Type.ShouldBe(typeof(string)),
            static p => p.CanRead.ShouldBeTrue(),
            static p => p.CanWrite.ShouldBeFalse(),
            static p => p.IsNullable.ShouldBeTrue(),
            static p => p.IsRequired.ShouldBeFalse(),
            static p => p.IsUnsettable.ShouldBeFalse(),
        ]);

        var writeOnlyProp = props.ShouldContainSingle(static p => p.Name == nameof(Properties.WriteOnlyProperty))
            .ShouldBeAssignableTo<IFieldValueRecordPropertyModel<Properties, string>>();
        writeOnlyProp.ShouldSatisfyAllConditions([
            static p => p.Type.ShouldBe(typeof(string)),
            static p => p.CanRead.ShouldBeFalse(),
            static p => p.CanWrite.ShouldBeTrue(),
            static p => p.IsNullable.ShouldBeTrue(),
            static p => p.IsRequired.ShouldBeFalse(),
            static p => p.IsUnsettable.ShouldBeFalse(),
        ]);

        var readOnlyFieldValue = props.ShouldContainSingle(static p => p.Name == nameof(Properties.ReadOnlyFieldValue))
            .ShouldBeAssignableTo<IFieldValueRecordPropertyModel<Properties, string>>();
        readOnlyFieldValue.ShouldSatisfyAllConditions([
            static p => p.Type.ShouldBe(typeof(string)),
            static p => p.CanRead.ShouldBeTrue(),
            static p => p.CanWrite.ShouldBeFalse(),
            static p => p.IsNullable.ShouldBeTrue(),
            static p => p.IsRequired.ShouldBeFalse(),
            static p => p.IsUnsettable.ShouldBeTrue(),
        ]);

        var writeOnlyFieldValue = props.ShouldContainSingle(static p => p.Name == nameof(Properties.WriteOnlyFieldValue))
            .ShouldBeAssignableTo<IFieldValueRecordPropertyModel<Properties, string>>();
        writeOnlyFieldValue.ShouldSatisfyAllConditions([
            static p => p.Type.ShouldBe(typeof(string)),
            static p => p.CanRead.ShouldBeFalse(),
            static p => p.CanWrite.ShouldBeTrue(),
            static p => p.IsNullable.ShouldBeTrue(),
            static p => p.IsRequired.ShouldBeFalse(),
            static p => p.IsUnsettable.ShouldBeTrue(),
        ]);

        var optionalNonNullable = props.ShouldContainSingle(static p => p.Name == nameof(Properties.OptionalNonNullable))
            .ShouldBeAssignableTo<IFieldValueRecordPropertyModel<Properties, string>>();
        optionalNonNullable.ShouldSatisfyAllConditions([
            static p => p.Type.ShouldBe(typeof(string)),
            static p => p.CanRead.ShouldBeTrue(),
            static p => p.CanWrite.ShouldBeTrue(),
            static p => p.IsNullable.ShouldBeFalse(),
            static p => p.IsRequired.ShouldBeFalse(),
            static p => p.IsUnsettable.ShouldBeFalse(),
        ]);

        var requiredNonNullable = props.ShouldContainSingle(static p => p.Name == nameof(Properties.RequiredNonNullable))
            .ShouldBeAssignableTo<IFieldValueRecordPropertyModel<Properties, string>>();
        requiredNonNullable.ShouldSatisfyAllConditions([
            static p => p.Type.ShouldBe(typeof(string)),
            static p => p.CanRead.ShouldBeTrue(),
            static p => p.CanWrite.ShouldBeTrue(),
            static p => p.IsNullable.ShouldBeFalse(),
            static p => p.IsRequired.ShouldBeTrue(),
            static p => p.IsUnsettable.ShouldBeFalse(),
        ]);

        var obj = new Properties { RequiredNonNullable = "required" };
        readOnlyProp.Read(obj).ShouldBeNull();
        readOnlyFieldValue.Read(obj).ShouldBeUnset();

        writeOnlyProp.Write(obj, "prop");
        writeOnlyFieldValue.Write(obj, "field-value");

        readOnlyProp.Read(obj).Value.ShouldBe("prop");
        readOnlyFieldValue.Read(obj).Value.ShouldBe("field-value");

        writeOnlyFieldValue.Write(obj, FieldValue.Unset);
        readOnlyFieldValue.Read(obj).ShouldBeUnset();

        Should.Throw<InvalidOperationException>(() => readOnlyProp.Write(obj, "prop")).Message.ShouldBe($"Property {readOnlyProp.Name} is not writable.");
        Should.Throw<InvalidOperationException>(() => readOnlyFieldValue.Write(obj, "field-value")).Message.ShouldBe($"Property {readOnlyFieldValue.Name} is not writable.");
        Should.Throw<InvalidOperationException>(() => writeOnlyProp.Read(obj)).Message.ShouldBe($"Property {writeOnlyProp.Name} is not readable.");
        Should.Throw<InvalidOperationException>(() => writeOnlyProp.Write(obj, FieldValue.Unset)).Message.ShouldBe($"Property {writeOnlyProp.Name} is not unsettable.");
        Should.Throw<InvalidOperationException>(() => writeOnlyFieldValue.Read(obj)).Message.ShouldBe($"Property {writeOnlyFieldValue.Name} is not readable.");
        Should.Throw<InvalidOperationException>(() => optionalNonNullable.Write(obj, FieldValue.Null)).Message.ShouldBe($"Property {optionalNonNullable.Name} is not nullable.");
        Should.Throw<InvalidOperationException>(() => requiredNonNullable.Write(obj, FieldValue.Null)).Message.ShouldBe($"Property {requiredNonNullable.Name} is not nullable.");
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

    [FieldValueRecord]
    public record Properties
    {
        private string? _value;
        private FieldValue<string> _fieldValue;

        public string OptionalNonNullable { get; set; } = "";

        public required string RequiredNonNullable { get; set; }

        public string? ReadOnlyProperty
        {
            get => _value;
        }

        public string? WriteOnlyProperty
        {
            set => _value = value;
        }

        public FieldValue<string> ReadOnlyFieldValue
        {
            get => _fieldValue;
        }

        public FieldValue<string> WriteOnlyFieldValue
        {
            set => _fieldValue = value;
        }
    }
}
