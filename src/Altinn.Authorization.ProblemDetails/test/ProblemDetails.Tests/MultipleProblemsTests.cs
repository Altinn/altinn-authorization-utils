using System.Net;

namespace Altinn.Authorization.ProblemDetails.Tests;

public class MultipleProblemsTests
{
    [Fact]
    public void DefaultValidationErrors_IsEmpty()
    {
        var errors = new MultipleProblemBuilder();

        errors.Count.ShouldBe(0);
        errors.IsEmpty.ShouldBeTrue();
    }

    [Fact]
    public void CanAddValidationErrors()
    {
        var errors = new MultipleProblemBuilder();

        errors.Add(TestErrors.InternalServerError);

        errors.Count.ShouldBe(1);
        errors.IsEmpty.ShouldBeFalse();

        errors.Add(TestErrors.BadRequest);

        errors.Count.ShouldBe(2);
        errors.IsEmpty.ShouldBeFalse();
    }

    [Fact]
    public void Empty_TryTo_Returns_False()
    {
        var errors = new MultipleProblemBuilder();

        errors.TryBuild(out var instance).ShouldBeFalse();
        errors.TryToProblemDetails(out var details).ShouldBeFalse();
        errors.TryToActionResult(out var result).ShouldBeFalse();

        instance.ShouldBeNull();
        details.ShouldBeNull();
        result.ShouldBeNull();
    }

    [Fact]
    public void Single_NoExtensions_TryTo_Returns_Inner()
    {
        var errors = new MultipleProblemBuilder();

        errors.Add(TestErrors.InternalServerError);

        errors.TryBuild(out var instance).ShouldBeTrue();
        errors.TryToProblemDetails(out var details).ShouldBeTrue();
        errors.TryToActionResult(out var result).ShouldBeTrue();

        instance.ShouldNotBeNull();
        details.ShouldNotBeNull();
        result.ShouldNotBeNull();

        instance.ErrorCode.ShouldBe(TestErrors.InternalServerError.ErrorCode);
        details.ErrorCode.ShouldBe(TestErrors.InternalServerError.ErrorCode);
    }

    [Fact]
    public void Single_WithExtensions_TryTo_Returns_Multiple()
    {
        var errors = new MultipleProblemBuilder();

        errors.Add(TestErrors.InternalServerError);
        errors.AddExtension("foo", "bar");

        errors.TryBuild(out var instance).ShouldBeTrue();
        errors.TryToProblemDetails(out var details).ShouldBeTrue();
        errors.TryToActionResult(out var result).ShouldBeTrue();

        instance.ShouldNotBeNull();
        details.ShouldNotBeNull();
        result.ShouldNotBeNull();

        var multipleInstance = instance.ShouldBeOfType<MultipleProblemInstance>();
        multipleInstance.Problems.Length.ShouldBe(1);

        instance.ErrorCode.ShouldBe(StdProblemDescriptors.ErrorCodes.MultipleProblems);
        details.ErrorCode.ShouldBe(StdProblemDescriptors.ErrorCodes.MultipleProblems);
    }

    [Fact]
    public void Multiple_TryTo_Returns_Multiple()
    {
        var errors = new MultipleProblemBuilder();

        errors.Add(TestErrors.InternalServerError);
        errors.Add(TestErrors.NotImplemented, [new("foo", "bar")]);

        errors.TryBuild(out var instance).ShouldBeTrue();
        errors.TryToProblemDetails(out var details).ShouldBeTrue();
        errors.TryToActionResult(out var result).ShouldBeTrue();

        instance.ShouldNotBeNull();
        details.ShouldNotBeNull();
        result.ShouldNotBeNull();

        var multipleInstance = instance.ShouldBeOfType<MultipleProblemInstance>();
        multipleInstance.Problems.Length.ShouldBe(2);

        instance.ErrorCode.ShouldBe(StdProblemDescriptors.ErrorCodes.MultipleProblems);
        details.ErrorCode.ShouldBe(StdProblemDescriptors.ErrorCodes.MultipleProblems);
    }

    [Fact]
    public void Errors_IncludedInExceptionMessage()
    {
        var validationBuilder = new ValidationErrorBuilder()
        {
            { StdValidationErrors.Required, "/path", [new("ext", "val")] },
            { StdValidationErrors.Required, ["/path2", "/path3"] },
        };
        validationBuilder.AddExtension("vld-ext", "vld-val");
        validationBuilder.TryBuild(out var validationInstance).ShouldBeTrue();

        var errors = new MultipleProblemBuilder()
        {
            validationInstance,
            { TestErrors.InternalServerError, [new("foo", "bar")] },
        };
        errors.AddExtension("root-ext", "root-val");

        errors.TryBuild(out var instance).ShouldBeTrue();
        var exception = new ProblemInstanceException(instance);

        exception.Message.ShouldBe(
            $"""
            {StdProblemDescriptors.MultipleProblems.Detail}
            code: {StdProblemDescriptors.MultipleProblems.ErrorCode}
            root-ext: root-val

            Problems:
             - {StdProblemDescriptors.ValidationError.ErrorCode}: {StdProblemDescriptors.ValidationError.Detail}
               vld-ext: vld-val

               Validation errors:
                - {StdValidationErrors.Required.ErrorCode}: {StdValidationErrors.Required.Detail}
                  path: /path
                  ext: val
                - {StdValidationErrors.Required.ErrorCode}: {StdValidationErrors.Required.Detail}
                  path: /path2
                  path: /path3
             - {TestErrors.InternalServerError.ErrorCode}: {TestErrors.InternalServerError.Detail}
               foo: bar

            """);
    }
}
