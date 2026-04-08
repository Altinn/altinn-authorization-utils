using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Altinn.Authorization.ProblemDetails.Validation;

namespace Altinn.Authorization.ProblemDetails.Tests;

public class ValidationTests
{
    [Fact]
    public void InputModel_ValidInput_ReturnsValidatedValue_AndNoProblems()
    {
        ValidationProblemBuilder builder = new();
        var input = new SingleNumberInput
        {
            Value = "42",
        };

        var result = input.TryValidate(ref builder, out SingleNumber? validated);

        result.ShouldBeTrue();
        validated.ShouldNotBeNull();
        validated.Value.ShouldBe(42);
        builder.Count.ShouldBe(0);
        builder.TryBuild(out _).ShouldBeFalse();
    }

    [Fact]
    public void InputModel_InvalidInput_AddsProblemAtRootPath()
    {
        ValidationProblemBuilder builder = new();
        var input = new SingleNumberInput
        {
            Value = "nope",
        };

        var result = input.TryValidate(ref builder, out SingleNumber? validated);

        result.ShouldBeFalse();
        validated.ShouldBeNull();
        builder.Count.ShouldBe(1);

        builder.TryBuild(out var instance).ShouldBeTrue();
        instance.ShouldNotBeNull();
        instance.Errors.Length.ShouldBe(1);
        instance.Errors[0].ErrorCode.ShouldBe(ValidationDescriptors.FieldOutOfRange.ErrorCode);
        instance.Errors[0].Detail.ShouldBe("Value must be a positive integer.");
        instance.Errors[0].Paths.ShouldBe(["/value"], ignoreOrder: true);
    }

    [Fact]
    public void CompositeValidator_ValidInput_ReturnsValidatedValue()
    {
        ValidationProblemBuilder builder = new();
        var input = new InnerInput
        {
            PositiveNumber = "42",
            NegativeNumber = "-7",
        };

        var result = input.TryValidate(ref builder, out Inner? validated);

        result.ShouldBeTrue();
        validated.ShouldNotBeNull();
        validated.PositiveNumberValidated.ShouldBe(42);
        validated.NegativeNumberValidated.ShouldBe(-7);
        builder.Count.ShouldBe(0);
        builder.TryBuild(out _).ShouldBeFalse();
    }

    [Fact]
    public void CompositeValidator_InvalidChildren_ReturnsFalse_AndAddsChildProblems()
    {
        ValidationProblemBuilder builder = new();
        var input = new InnerInput
        {
            PositiveNumber = "-1",
            NegativeNumber = "7",
        };

        var result = input.TryValidate(ref builder, out Inner? validated);

        result.ShouldBeFalse();
        validated.ShouldBeNull();
        builder.Count.ShouldBe(2);

        builder.TryBuild(out var instance).ShouldBeTrue();
        instance.ShouldNotBeNull();
        instance.Errors.Length.ShouldBe(2);

        instance.Errors[0].ErrorCode.ShouldBe(ValidationDescriptors.FieldOutOfRange.ErrorCode);
        instance.Errors[0].Detail.ShouldBe("Value must be a positive integer.");
        instance.Errors[0].Paths.ShouldBe(["/positiveNumber"], ignoreOrder: true);

        instance.Errors[1].ErrorCode.ShouldBe(ValidationDescriptors.FieldOutOfRange.ErrorCode);
        instance.Errors[1].Detail.ShouldBe("Value must be a negative integer.");
        instance.Errors[1].Paths.ShouldBe(["/negativeNumber"], ignoreOrder: true);
    }

    [Fact]
    public void OuterInput_ValidInput_ReturnsValidatedValue()
    {
        ValidationProblemBuilder builder = new();
        var input = new OuterInput
        {
            Left = new InnerInput
            {
                PositiveNumber = "42",
                NegativeNumber = "-7",
            },
            Right = new InnerInput
            {
                PositiveNumber = "9",
                NegativeNumber = "-3",
            },
        };

        var result = input.TryValidate(ref builder, out Outer? validated);

        result.ShouldBeTrue();
        validated.ShouldNotBeNull();
        validated.Left.PositiveNumberValidated.ShouldBe(42);
        validated.Left.NegativeNumberValidated.ShouldBe(-7);
        validated.Right.PositiveNumberValidated.ShouldBe(9);
        validated.Right.NegativeNumberValidated.ShouldBe(-3);
        builder.Count.ShouldBe(0);
        builder.TryBuild(out _).ShouldBeFalse();
    }

    [Fact]
    public void OuterInput_InvalidChildren_ReturnsFalse_AndAddsNestedProblems()
    {
        ValidationProblemBuilder builder = new();
        var input = new OuterInput
        {
            Left = new InnerInput
            {
                PositiveNumber = "-1",
                NegativeNumber = "-7",
            },
            Right = new InnerInput
            {
                PositiveNumber = "9",
                NegativeNumber = "7",
            },
        };

        var result = input.TryValidate(ref builder, out Outer? validated);

        result.ShouldBeFalse();
        validated.ShouldBeNull();
        builder.Count.ShouldBe(2);

        builder.TryBuild(out var instance).ShouldBeTrue();
        instance.ShouldNotBeNull();
        instance.Errors.Length.ShouldBe(2);

        instance.Errors[0].ErrorCode.ShouldBe(ValidationDescriptors.FieldOutOfRange.ErrorCode);
        instance.Errors[0].Detail.ShouldBe("Value must be a positive integer.");
        instance.Errors[0].Paths.ShouldBe(["/left/positiveNumber"], ignoreOrder: true);

        instance.Errors[1].ErrorCode.ShouldBe(ValidationDescriptors.FieldOutOfRange.ErrorCode);
        instance.Errors[1].Detail.ShouldBe("Value must be a negative integer.");
        instance.Errors[1].Paths.ShouldBe(["/right/negativeNumber"], ignoreOrder: true);
    }

    [Fact]
    public void InputModel_WithNullChild_AddsRequiredProblemAtChildPath()
    {
        ValidationProblemBuilder builder = new();
        var input = new OptionalChildInput
        {
            Child = null,
        };

        var result = input.TryValidate(ref builder, out OptionalChildValidated? validated);

        result.ShouldBeFalse();
        validated.ShouldBeNull();

        builder.TryBuild(out var instance).ShouldBeTrue();
        instance.ShouldNotBeNull();
        instance.Errors.Length.ShouldBe(1);
        instance.Errors[0].ErrorCode.ShouldBe(StdValidationErrors.ErrorCodes.Required);
        instance.Errors[0].Paths.ShouldBe(["/child"], ignoreOrder: true);
    }

    [Fact]
    public void ValidatorRootOverloads_UseNormalizedRootAndCustomPrefixPaths()
    {
        ValidationProblemBuilder builder = new();

        default(RootPathValidator).TryValidate(ref builder, input: "ignored", out string? rootValidated).ShouldBeFalse();
        default(RootPathValidator).TryValidate(ref builder, "/prefix", input: "ignored", out string? prefixedValidated).ShouldBeFalse();

        rootValidated.ShouldBeNull();
        prefixedValidated.ShouldBeNull();

        builder.TryBuild(out var instance).ShouldBeTrue();
        instance.ShouldNotBeNull();
        instance.Errors.Length.ShouldBe(2);
        instance.Errors[0].Paths.ShouldBe(["/"], ignoreOrder: true);
        instance.Errors[1].Paths.ShouldBe(["/prefix"], ignoreOrder: true);
    }

    [Fact]
    public void ValidationContext_ProblemOverloads_AddExpectedPathsDetailsAndExtensions()
    {
        ValidationProblemBuilder builder = new();
        var result = new OverloadExerciseInput().TryValidate(ref builder, "/root", out OverloadExerciseValidated? validated);

        result.ShouldBeFalse();
        validated.ShouldBeNull();

        builder.TryBuild(out var instance).ShouldBeTrue();
        instance.ShouldNotBeNull();
        instance.Errors.Length.ShouldBe(20);

        instance.Errors[0].ShouldSatisfyAllConditions(
            e => e.ErrorCode.ShouldBe(ValidationDescriptors.OverloadExercise01.ErrorCode),
            e => e.Detail.ShouldBeNull(),
            e => e.Paths.ShouldBe(["/root"], ignoreOrder: true),
            e => e.Extensions.Length.ShouldBe(0));

        instance.Errors[1].ShouldSatisfyAllConditions(
            e => e.ErrorCode.ShouldBe(ValidationDescriptors.OverloadExercise02.ErrorCode),
            e => e.Detail.ShouldBe("detail"),
            e => e.Paths.ShouldBe(["/root"], ignoreOrder: true),
            e => e.Extensions.Length.ShouldBe(0));

        instance.Errors[2].ShouldSatisfyAllConditions(
            e => e.ErrorCode.ShouldBe(ValidationDescriptors.OverloadExercise03.ErrorCode),
            e => e.Detail.ShouldBeNull(),
            e => e.Paths.ShouldBe(["/root"], ignoreOrder: true),
            e => e.Extensions.Length.ShouldBe(1),
            e => e.Extensions.ContainsKey("source").ShouldBeTrue());

        instance.Errors[3].ShouldSatisfyAllConditions(
            e => e.ErrorCode.ShouldBe(ValidationDescriptors.OverloadExercise04.ErrorCode),
            e => e.Detail.ShouldBe("detail"),
            e => e.Paths.ShouldBe(["/root"], ignoreOrder: true),
            e => e.Extensions.Length.ShouldBe(1),
            e => e.Extensions.ContainsKey("source").ShouldBeTrue());

        instance.Errors[4].ShouldSatisfyAllConditions(
            e => e.ErrorCode.ShouldBe(ValidationDescriptors.OverloadExercise05.ErrorCode),
            e => e.Detail.ShouldBeNull(),
            e => e.Paths.ShouldBe(["/root/string"], ignoreOrder: true),
            e => e.Extensions.Length.ShouldBe(0));

        instance.Errors[5].ShouldSatisfyAllConditions(
            e => e.ErrorCode.ShouldBe(ValidationDescriptors.OverloadExercise06.ErrorCode),
            e => e.Detail.ShouldBeNull(),
            e => e.Paths.ShouldBe(["/root/span-a", "/root/span-b"], ignoreOrder: true),
            e => e.Extensions.Length.ShouldBe(0));

        instance.Errors[6].ShouldSatisfyAllConditions(
            e => e.ErrorCode.ShouldBe(ValidationDescriptors.OverloadExercise07.ErrorCode),
            e => e.Detail.ShouldBeNull(),
            e => e.Paths.ShouldBe(["/root/immutable-a", "/root/immutable-b"], ignoreOrder: true),
            e => e.Extensions.Length.ShouldBe(0));

        instance.Errors[7].ShouldSatisfyAllConditions(
            e => e.ErrorCode.ShouldBe(ValidationDescriptors.OverloadExercise08.ErrorCode),
            e => e.Detail.ShouldBeNull(),
            e => e.Paths.ShouldBe(["/root/enumerable-a", "/root/enumerable-b"], ignoreOrder: true),
            e => e.Extensions.Length.ShouldBe(0));

        instance.Errors[8].ShouldSatisfyAllConditions(
            e => e.ErrorCode.ShouldBe(ValidationDescriptors.OverloadExercise09.ErrorCode),
            e => e.Detail.ShouldBe("detail"),
            e => e.Paths.ShouldBe(["/root/string-detail"], ignoreOrder: true),
            e => e.Extensions.Length.ShouldBe(0));

        instance.Errors[9].ShouldSatisfyAllConditions(
            e => e.ErrorCode.ShouldBe(ValidationDescriptors.OverloadExercise10.ErrorCode),
            e => e.Detail.ShouldBe("detail"),
            e => e.Paths.ShouldBe(["/root/span-a", "/root/span-b"], ignoreOrder: true),
            e => e.Extensions.Length.ShouldBe(0));

        instance.Errors[10].ShouldSatisfyAllConditions(
            e => e.ErrorCode.ShouldBe(ValidationDescriptors.OverloadExercise11.ErrorCode),
            e => e.Detail.ShouldBe("detail"),
            e => e.Paths.ShouldBe(["/root/immutable-a", "/root/immutable-b"], ignoreOrder: true),
            e => e.Extensions.Length.ShouldBe(0));

        instance.Errors[11].ShouldSatisfyAllConditions(
            e => e.ErrorCode.ShouldBe(ValidationDescriptors.OverloadExercise12.ErrorCode),
            e => e.Detail.ShouldBe("detail"),
            e => e.Paths.ShouldBe(["/root/enumerable-a", "/root/enumerable-b"], ignoreOrder: true),
            e => e.Extensions.Length.ShouldBe(0));

        instance.Errors[12].ShouldSatisfyAllConditions(
            e => e.ErrorCode.ShouldBe(ValidationDescriptors.OverloadExercise13.ErrorCode),
            e => e.Detail.ShouldBeNull(),
            e => e.Paths.ShouldBe(["/root/string-ext"], ignoreOrder: true),
            e => e.Extensions.Length.ShouldBe(1),
            e => e.Extensions.ContainsKey("source").ShouldBeTrue());

        instance.Errors[13].ShouldSatisfyAllConditions(
            e => e.ErrorCode.ShouldBe(ValidationDescriptors.OverloadExercise14.ErrorCode),
            e => e.Detail.ShouldBeNull(),
            e => e.Paths.ShouldBe(["/root/span-a", "/root/span-b"], ignoreOrder: true),
            e => e.Extensions.Length.ShouldBe(1),
            e => e.Extensions.ContainsKey("source").ShouldBeTrue());

        instance.Errors[14].ShouldSatisfyAllConditions(
            e => e.ErrorCode.ShouldBe(ValidationDescriptors.OverloadExercise15.ErrorCode),
            e => e.Detail.ShouldBeNull(),
            e => e.Paths.ShouldBe(["/root/immutable-a", "/root/immutable-b"], ignoreOrder: true),
            e => e.Extensions.Length.ShouldBe(1),
            e => e.Extensions.ContainsKey("source").ShouldBeTrue());

        instance.Errors[15].ShouldSatisfyAllConditions(
            e => e.ErrorCode.ShouldBe(ValidationDescriptors.OverloadExercise16.ErrorCode),
            e => e.Detail.ShouldBeNull(),
            e => e.Paths.ShouldBe(["/root/enumerable-a", "/root/enumerable-b"], ignoreOrder: true),
            e => e.Extensions.Length.ShouldBe(1),
            e => e.Extensions.ContainsKey("source").ShouldBeTrue());

        instance.Errors[16].ShouldSatisfyAllConditions(
            e => e.ErrorCode.ShouldBe(ValidationDescriptors.OverloadExercise17.ErrorCode),
            e => e.Detail.ShouldBe("detail"),
            e => e.Paths.ShouldBe(["/root/string-ext-detail"], ignoreOrder: true),
            e => e.Extensions.Length.ShouldBe(1),
            e => e.Extensions.ContainsKey("source").ShouldBeTrue());

        instance.Errors[17].ShouldSatisfyAllConditions(
            e => e.ErrorCode.ShouldBe(ValidationDescriptors.OverloadExercise18.ErrorCode),
            e => e.Detail.ShouldBe("detail"),
            e => e.Paths.ShouldBe(["/root/span-a", "/root/span-b"], ignoreOrder: true),
            e => e.Extensions.Length.ShouldBe(1),
            e => e.Extensions.ContainsKey("source").ShouldBeTrue());

        instance.Errors[18].ShouldSatisfyAllConditions(
            e => e.ErrorCode.ShouldBe(ValidationDescriptors.OverloadExercise19.ErrorCode),
            e => e.Detail.ShouldBe("detail"),
            e => e.Paths.ShouldBe(["/root/immutable-a", "/root/immutable-b"], ignoreOrder: true),
            e => e.Extensions.Length.ShouldBe(1),
            e => e.Extensions.ContainsKey("source").ShouldBeTrue());

        instance.Errors[19].ShouldSatisfyAllConditions(
            e => e.ErrorCode.ShouldBe(ValidationDescriptors.OverloadExercise20.ErrorCode),
            e => e.Detail.ShouldBe("detail"),
            e => e.Paths.ShouldBe(["/root/enumerable-a", "/root/enumerable-b"], ignoreOrder: true),
            e => e.Extensions.Length.ShouldBe(1),
            e => e.Extensions.ContainsKey("source").ShouldBeTrue());
    }

    private sealed class OuterInput
        : IInputModel<Outer>
    {
        public InnerInput? Left { get; init; }

        public InnerInput? Right { get; init; }

        public bool TryValidate(
            ref ValidationContext context,
            [NotNullWhen(true)] out Outer? validated)
        {
            context.TryValidateChild("/left", Left, out Inner? left);
            context.TryValidateChild("/right", Right, out Inner? right);

            if (context.HasErrors)
            {
                validated = default;
                return false;
            }

            Debug.Assert(left is not null);
            Debug.Assert(right is not null);
            validated = new Outer(Left: left, Right: right);
            return true;
        }
    }

    private sealed record Outer(
        Inner Left,
        Inner Right);

    private sealed class OptionalChildInput
        : IInputModel<OptionalChildValidated>
    {
        public ChildInput? Child { get; init; }

        public bool TryValidate(ref ValidationContext context, [NotNullWhen(true)] out OptionalChildValidated? validated)
        {
            context.TryValidateChild("/child", Child, out ChildValidated? child);
            validated = default;
            return child is not null;
        }
    }

    private sealed record OptionalChildValidated(ChildValidated Child);

    private sealed class ChildInput
        : IInputModel<ChildValidated>
    {
        public bool TryValidate(ref ValidationContext context, [NotNullWhen(true)] out ChildValidated? validated)
        {
            validated = new ChildValidated();
            return true;
        }
    }

    private sealed record ChildValidated;

    private sealed class SingleNumberInput
        : IInputModel<SingleNumber>
    {
        public string? Value { get; init; }

        public bool TryValidate(
            ref ValidationContext context,
            [MaybeNullWhen(false)] out SingleNumber validated)
        {
            context.TryValidateChild("/value", Value, default(PositiveNumberValidator), out int parsedValue);

            if (context.HasErrors)
            {
                validated = default;
                return false;
            }

            validated = new SingleNumber(parsedValue);
            return true;
        }
    }

    private sealed record SingleNumber(int Value);

    private sealed class InnerInput
        : IInputModel<Inner>
    {
        public string? PositiveNumber { get; init; }

        public string? NegativeNumber { get; init; }

        public bool TryValidate(
            ref ValidationContext context,
            [MaybeNullWhen(false)] out Inner validated)
        {
            context.TryValidateChild("/positiveNumber", PositiveNumber, default(PositiveNumberValidator), out int positiveNumber);
            context.TryValidateChild("/negativeNumber", NegativeNumber, default(NegativeNumberValidator), out int negativeNumber);

            if (context.HasErrors)
            {
                validated = default;
                return false;
            }

            validated = new Inner(PositiveNumberValidated: positiveNumber, NegativeNumberValidated: negativeNumber);

            return true;
        }
    }

    private sealed record Inner(
        int PositiveNumberValidated,
        int NegativeNumberValidated);

    private sealed class OverloadExerciseInput
        : IInputModel<OverloadExerciseValidated>
    {
        public bool TryValidate(ref ValidationContext context, [NotNullWhen(true)] out OverloadExerciseValidated? validated)
        {
            ProblemExtensionData extensions =
            [
                KeyValuePair.Create("source", "test"),
            ];

            ImmutableArray<string> immutablePaths = ["/immutable-a", "/immutable-b"];
            IEnumerable<string> enumerablePaths = ["/enumerable-a", "/enumerable-b"];

            context.AddProblem(ValidationDescriptors.OverloadExercise01);
            context.AddProblem(ValidationDescriptors.OverloadExercise02, "detail");
            context.AddProblem(ValidationDescriptors.OverloadExercise03, extensions);
            context.AddProblem(ValidationDescriptors.OverloadExercise04, extensions, "detail");

            context.AddChildProblem(ValidationDescriptors.OverloadExercise05, "/string");
            context.AddChildProblem(ValidationDescriptors.OverloadExercise06, ["/span-a", "/span-b"]);
            context.AddChildProblem(ValidationDescriptors.OverloadExercise07, immutablePaths);
            context.AddChildProblem(ValidationDescriptors.OverloadExercise08, enumerablePaths);

            context.AddChildProblem(ValidationDescriptors.OverloadExercise09, "/string-detail", "detail");
            context.AddChildProblem(ValidationDescriptors.OverloadExercise10, ["/span-a", "/span-b"], "detail");
            context.AddChildProblem(ValidationDescriptors.OverloadExercise11, immutablePaths, "detail");
            context.AddChildProblem(ValidationDescriptors.OverloadExercise12, enumerablePaths, "detail");

            context.AddChildProblem(ValidationDescriptors.OverloadExercise13, "/string-ext", extensions);
            context.AddChildProblem(ValidationDescriptors.OverloadExercise14, ["/span-a", "/span-b"], extensions);
            context.AddChildProblem(ValidationDescriptors.OverloadExercise15, immutablePaths, extensions);
            context.AddChildProblem(ValidationDescriptors.OverloadExercise16, enumerablePaths, extensions);

            context.AddChildProblem(ValidationDescriptors.OverloadExercise17, "/string-ext-detail", extensions, "detail");
            context.AddChildProblem(ValidationDescriptors.OverloadExercise18, ["/span-a", "/span-b"], extensions, "detail");
            context.AddChildProblem(ValidationDescriptors.OverloadExercise19, immutablePaths, extensions, "detail");
            context.AddChildProblem(ValidationDescriptors.OverloadExercise20, enumerablePaths, extensions, "detail");

            validated = default;
            return false;
        }
    }

    private sealed record OverloadExerciseValidated;

    private readonly struct PositiveNumberValidator
        : IValidator<string?, int>
    {
        public bool TryValidate(
            ref ValidationContext context,
            string? input,
            [MaybeNullWhen(false)] out int validated)
        {
            if (string.IsNullOrEmpty(input))
            {
                context.AddProblem(StdValidationErrors.Required);
                validated = default;
                return false;
            }

            if (int.TryParse(input, out validated) && validated > 0)
            {
                return true;
            }

            context.AddProblem(ValidationDescriptors.FieldOutOfRange, "Value must be a positive integer.");
            return false;
        }
    }

    private readonly struct NegativeNumberValidator
        : IValidator<string?, int>
    {
        public bool TryValidate(
            ref ValidationContext context,
            string? input,
            [MaybeNullWhen(false)] out int validated)
        {
            if (string.IsNullOrEmpty(input))
            {
                context.AddProblem(StdValidationErrors.Required);
                validated = default;
                return false;
            }

            if (int.TryParse(input, out validated) && validated < 0)
            {
                return true;
            }

            context.AddProblem(ValidationDescriptors.FieldOutOfRange, "Value must be a negative integer.");
            return false;
        }
    }

    private readonly struct RootPathValidator
        : IValidator<string, string>
    {
        public bool TryValidate(ref ValidationContext context, string input, [NotNullWhen(true)] out string? validated)
        {
            context.AddProblem(ValidationDescriptors.FieldOutOfRange);
            validated = default;
            return false;
        }
    }

    private static class ValidationDescriptors
    {
        private static readonly ValidationErrorDescriptorFactory _factory
            = ValidationErrorDescriptorFactory.New("TEST");

        public static ValidationErrorDescriptor FieldOutOfRange { get; }
            = _factory.Create(2, "Field is out of range.");

        public static ValidationErrorDescriptor OverloadExercise01 { get; } = _factory.Create(101, "Test validation error 1");
        public static ValidationErrorDescriptor OverloadExercise02 { get; } = _factory.Create(102, "Test validation error 2");
        public static ValidationErrorDescriptor OverloadExercise03 { get; } = _factory.Create(103, "Test validation error 3");
        public static ValidationErrorDescriptor OverloadExercise04 { get; } = _factory.Create(104, "Test validation error 4");
        public static ValidationErrorDescriptor OverloadExercise05 { get; } = _factory.Create(105, "Test validation error 5");
        public static ValidationErrorDescriptor OverloadExercise06 { get; } = _factory.Create(106, "Test validation error 6");
        public static ValidationErrorDescriptor OverloadExercise07 { get; } = _factory.Create(107, "Test validation error 7");
        public static ValidationErrorDescriptor OverloadExercise08 { get; } = _factory.Create(108, "Test validation error 8");
        public static ValidationErrorDescriptor OverloadExercise09 { get; } = _factory.Create(109, "Test validation error 9");
        public static ValidationErrorDescriptor OverloadExercise10 { get; } = _factory.Create(110, "Test validation error 10");
        public static ValidationErrorDescriptor OverloadExercise11 { get; } = _factory.Create(111, "Test validation error 11");
        public static ValidationErrorDescriptor OverloadExercise12 { get; } = _factory.Create(112, "Test validation error 12");
        public static ValidationErrorDescriptor OverloadExercise13 { get; } = _factory.Create(113, "Test validation error 13");
        public static ValidationErrorDescriptor OverloadExercise14 { get; } = _factory.Create(114, "Test validation error 14");
        public static ValidationErrorDescriptor OverloadExercise15 { get; } = _factory.Create(115, "Test validation error 15");
        public static ValidationErrorDescriptor OverloadExercise16 { get; } = _factory.Create(116, "Test validation error 16");
        public static ValidationErrorDescriptor OverloadExercise17 { get; } = _factory.Create(117, "Test validation error 17");
        public static ValidationErrorDescriptor OverloadExercise18 { get; } = _factory.Create(118, "Test validation error 18");
        public static ValidationErrorDescriptor OverloadExercise19 { get; } = _factory.Create(119, "Test validation error 19");
        public static ValidationErrorDescriptor OverloadExercise20 { get; } = _factory.Create(120, "Test validation error 20");
    }
}
