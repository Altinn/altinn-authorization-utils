using System.Diagnostics.CodeAnalysis;
using Altinn.Authorization.ProblemDetails.Validation;

namespace Altinn.Authorization.ProblemDetails.Tests;

public class ListValidationTests
{
    [Fact]
    public void IEnumerable_InputModel_Success()
    {
        IEnumerable<StringInputModel> inputs = [
            new("0"),
            new("1"),
            new("2"),
            new("3"),
        ];

        ValidationProblemBuilder builder = new();
        builder.TryValidate("/", inputs, ListValidator.ForEnumerable<StringInputModel, NonNullString>(), out IReadOnlyList<NonNullString>? result)
            .ShouldBeTrue();

        builder.TryBuild(out _).ShouldBeFalse();

        result.ShouldNotBeNull();
        result.ShouldBe([
            new NonNullString("0"),
            new NonNullString("1"),
            new NonNullString("2"),
            new NonNullString("3"),
        ]);
    }

    [Fact]
    public void IEnumerable_InputModel_WithErrors()
    {
        IEnumerable<StringInputModel> inputs = [
            new("0"),
            new(null),
            new("2"),
            new(null),
        ];

        ValidationProblemBuilder builder = new();
        builder.TryValidate("/", inputs, ListValidator.ForEnumerable<StringInputModel, NonNullString>(), out IReadOnlyList<NonNullString>? _)
            .ShouldBeFalse();

        builder.TryBuild(out var error).ShouldBeTrue();
        error.Errors.Length.ShouldBe(2);

        error.Errors[0].ShouldSatisfyAllConditions(
            e => e.Paths.ShouldBe(["/1"], ignoreOrder: true),
            e => e.ErrorCode.ShouldBe(StdValidationErrors.ErrorCodes.Required));

        error.Errors[1].ShouldSatisfyAllConditions(
            e => e.Paths.ShouldBe(["/3"], ignoreOrder: true),
            e => e.ErrorCode.ShouldBe(StdValidationErrors.ErrorCodes.Required));
    }

    [Fact]
    public void IEnumerable_Delegate_Success()
    {
        IEnumerable<string?> inputs = [
            "0",
            "1",
            "2",
            "3",
        ];

        ValidationProblemBuilder builder = new();
        builder.TryValidate("/", inputs, ListValidator.ForEnumerable(_delegateValidator), out IReadOnlyList<NonNullString>? result)
            .ShouldBeTrue();

        builder.TryBuild(out _).ShouldBeFalse();

        result.ShouldNotBeNull();
        result.ShouldBe([
            new NonNullString("0"),
            new NonNullString("1"),
            new NonNullString("2"),
            new NonNullString("3"),
        ]);
    }

    [Fact]
    public void IEnumerable_Delegate_WithErrors()
    {
        IEnumerable<string?> inputs = [
            "0",
            null,
            "2",
            null,
        ];

        ValidationProblemBuilder builder = new();
        builder.TryValidate("/", inputs, ListValidator.ForEnumerable(_delegateValidator), out IReadOnlyList<NonNullString>? _)
            .ShouldBeFalse();

        builder.TryBuild(out var error).ShouldBeTrue();
        error.Errors.Length.ShouldBe(2);

        error.Errors[0].ShouldSatisfyAllConditions(
            e => e.Paths.ShouldBe(["/1"], ignoreOrder: true),
            e => e.ErrorCode.ShouldBe(StdValidationErrors.ErrorCodes.Required));

        error.Errors[1].ShouldSatisfyAllConditions(
            e => e.Paths.ShouldBe(["/3"], ignoreOrder: true),
            e => e.ErrorCode.ShouldBe(StdValidationErrors.ErrorCodes.Required));
    }

    [Fact]
    public void IEnumerable_CustomValidator_Success()
    {
        IEnumerable<string?> inputs = [
            "0",
            "1",
            "2",
            "3",
        ];

        ValidationProblemBuilder builder = new();
        builder.TryValidate("/", inputs, ListValidator.ForEnumerable<string?, NonNullString, NonNullStringValidator>(default), out IReadOnlyList<NonNullString>? result)
            .ShouldBeTrue();

        builder.TryBuild(out _).ShouldBeFalse();

        result.ShouldNotBeNull();
        result.ShouldBe([
            new NonNullString("0"),
            new NonNullString("1"),
            new NonNullString("2"),
            new NonNullString("3"),
        ]);
    }

    [Fact]
    public void IEnumerable_CustomValidator_WithErrors()
    {
        IEnumerable<string?> inputs = [
            "0",
            null,
            "2",
            null,
        ];

        ValidationProblemBuilder builder = new();
        builder.TryValidate("/", inputs, ListValidator.ForEnumerable<string?, NonNullString, NonNullStringValidator>(default), out IReadOnlyList<NonNullString>? _)
            .ShouldBeFalse();

        builder.TryBuild(out var error).ShouldBeTrue();
        error.Errors.Length.ShouldBe(2);

        error.Errors[0].ShouldSatisfyAllConditions(
            e => e.Paths.ShouldBe(["/1"], ignoreOrder: true),
            e => e.ErrorCode.ShouldBe(StdValidationErrors.ErrorCodes.Required));

        error.Errors[1].ShouldSatisfyAllConditions(
            e => e.Paths.ShouldBe(["/3"], ignoreOrder: true),
            e => e.ErrorCode.ShouldBe(StdValidationErrors.ErrorCodes.Required));
    }

    [Fact]
    public void IEnumerable_CustomValidator_ToList_WithErrors()
    {
        IEnumerable<string?> inputs = [
            "0",
            null,
            "2",
            null,
        ];

        ValidationProblemBuilder builder = new();
        builder.TryValidate("/", inputs, ListValidator.ForEnumerable<string?, NonNullString, NonNullStringValidator>(default), out List<NonNullString>? _)
            .ShouldBeFalse();

        builder.TryBuild(out var error).ShouldBeTrue();
        error.Errors.Length.ShouldBe(2);

        error.Errors[0].ShouldSatisfyAllConditions(
            e => e.Paths.ShouldBe(["/1"], ignoreOrder: true),
            e => e.ErrorCode.ShouldBe(StdValidationErrors.ErrorCodes.Required));

        error.Errors[1].ShouldSatisfyAllConditions(
            e => e.Paths.ShouldBe(["/3"], ignoreOrder: true),
            e => e.ErrorCode.ShouldBe(StdValidationErrors.ErrorCodes.Required));
    }

    private record NonNullString(string Value);

    private record StringInputModel(string? Value) : IInputModel<NonNullString>
    {
        public bool TryValidate(ref ValidationContext context, [NotNullWhen(true)] out NonNullString? validated)
        {
            if (Value is null)
            {
                context.AddProblem(StdValidationErrors.Required);

                validated = null;
                return false;
            }

            validated = new NonNullString(Value);
            return true;
        }
    }

    private readonly struct NonNullStringValidator
        : IValidator<string?, NonNullString>
#if NET9_0_OR_GREATER
        // Note: this is here just to make sure it keeps compiling
        , IValidator<ReadOnlySpan<char>, NonNullString>
#endif
    {
        public bool TryValidate(ref ValidationContext context, string? input, [NotNullWhen(true)] out NonNullString? validated)
        {
            if (input is null)
            {
                context.AddProblem(StdValidationErrors.Required);

                validated = null;
                return false;
            }

            validated = new NonNullString(input);
            return true;
        }

        public bool TryValidate(ref ValidationContext context, ReadOnlySpan<char> input, [NotNullWhen(true)] out NonNullString? validated)
        {
            validated = new(new(input));
            return true;
        }
    }

    private static readonly Validator<string?, NonNullString> _delegateValidator = static (ref ValidationContext context, string? input, [NotNullWhen(true)] out NonNullString? validated) =>
    {
        if (input is null)
        {
            context.AddProblem(StdValidationErrors.Required);

            validated = null;
            return false;
        }

        validated = new NonNullString(input);
        return true;
    };
}
