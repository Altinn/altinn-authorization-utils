namespace Altinn.Authorization.ProblemDetails.Tests;

public class ValidationErrorsTests
{
    [Fact]
    public void Errors_IncludedInExceptionMessage()
    {
        var errors = new ValidationProblemBuilder();

        errors.Add(StdValidationErrors.Required, "/path", [new("ext", "val")]);
        errors.Add(StdValidationErrors.Required, ["/path2", "/path3"]);
        errors.AddExtension("root-ext", "root-val");

        errors.TryBuild(out var instance).ShouldBeTrue();
        var exception = new ProblemInstanceException(instance);

        exception.Message.ShouldBe(
            $"""
            {StdProblemDescriptors.ValidationError.Title}
            code: {StdProblemDescriptors.ValidationError.ErrorCode}
            root-ext: root-val

            Validation errors:
             - {StdValidationErrors.Required.ErrorCode}: {StdValidationErrors.Required.Title}
               path: /path
               ext: val
             - {StdValidationErrors.Required.ErrorCode}: {StdValidationErrors.Required.Title}
               path: /path2
               path: /path3

            """);
    }
}
