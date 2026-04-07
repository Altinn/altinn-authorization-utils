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

    private static class ValidationDescriptors
    {
        private static readonly ValidationErrorDescriptorFactory _factory
            = ValidationErrorDescriptorFactory.New("TEST");

        public static ValidationErrorDescriptor FieldOutOfRange { get; }
            = _factory.Create(2, "Field is out of range.");
    }
}
