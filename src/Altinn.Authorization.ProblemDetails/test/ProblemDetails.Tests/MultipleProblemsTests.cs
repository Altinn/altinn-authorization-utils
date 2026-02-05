using System.Net;

namespace Altinn.Authorization.ProblemDetails.Tests;

public class MultipleProblemsTests
{
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
            {StdProblemDescriptors.MultipleProblems.Title}
            code: {StdProblemDescriptors.MultipleProblems.ErrorCode}
            root-ext: root-val

            Problems:
             - {StdProblemDescriptors.ValidationError.ErrorCode}: {StdProblemDescriptors.ValidationError.Title}
               vld-ext: vld-val

               Validation errors:
                - {StdValidationErrors.Required.ErrorCode}: {StdValidationErrors.Required.Title}
                  path: /path
                  ext: val
                - {StdValidationErrors.Required.ErrorCode}: {StdValidationErrors.Required.Title}
                  path: /path2
                  path: /path3
             - {TestErrors.InternalServerError.ErrorCode}: {TestErrors.InternalServerError.Title}
               foo: bar

            """);
    }
}
