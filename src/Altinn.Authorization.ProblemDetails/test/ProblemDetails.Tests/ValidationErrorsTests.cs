﻿namespace Altinn.Authorization.ProblemDetails.Tests;

public class ValidationErrorsTests
{
    [Fact]
    public void DefaultValidationErrors_IsEmpty()
    {
        var errors = new ValidationErrorBuilder();

        errors.Count.ShouldBe(0);
        errors.IsEmpty.ShouldBeTrue();
    }

    [Fact]
    public void CanAddValidationErrors()
    {
        var errors = new ValidationErrorBuilder();

        errors.Add(StdValidationErrors.Required, "/path");
        
        errors.Count.ShouldBe(1);
        errors.IsEmpty.ShouldBeFalse();

        errors.Add(StdValidationErrors.Required, "/path2");

        errors.Count.ShouldBe(2);
        errors.IsEmpty.ShouldBeFalse();
    }

    [Fact]
    public void Empty_TryTo_Returns_False()
    {
        var errors = new ValidationErrorBuilder();

        errors.TryBuild(out var instance).ShouldBeFalse();
        errors.TryToProblemDetails(out var details).ShouldBeFalse();
        errors.TryToActionResult(out var result).ShouldBeFalse();

        instance.ShouldBeNull();
        details.ShouldBeNull();
        result.ShouldBeNull();
    }

    [Fact]
    public void Errors_TryTo_Returns_True()
    {
        var errors = new ValidationErrorBuilder();

        errors.Add(StdValidationErrors.Required, "/path");
        errors.Add(StdValidationErrors.Required, "/path2");

        errors.TryBuild(out var instance).ShouldBeTrue();
        errors.TryToProblemDetails(out var details).ShouldBeTrue();
        errors.TryToActionResult(out var result).ShouldBeTrue();

        instance.ShouldNotBeNull();
        details.ShouldNotBeNull();
        result.ShouldNotBeNull();
    }

    [Fact]
    public void Errors_IncludedInExceptionMessage()
    {
        var errors = new ValidationErrorBuilder();

        errors.Add(StdValidationErrors.Required, "/path", [new("ext", "val")]);
        errors.Add(StdValidationErrors.Required, ["/path2", "/path3"]);
        errors.AddExtension("root-ext", "root-val");

        errors.TryBuild(out var instance).ShouldBeTrue();
        var exception = new ProblemInstanceException(instance);

        exception.Message.ShouldBe(
            $"""
            {StdProblemDescriptors.ValidationError.Detail}
            code: {StdProblemDescriptors.ValidationError.ErrorCode}
            root-ext: root-val

            Validation errors:
             - {StdValidationErrors.Required.ErrorCode}: {StdValidationErrors.Required.Detail}
               path: /path
               ext: val
             - {StdValidationErrors.Required.ErrorCode}: {StdValidationErrors.Required.Detail}
               path: /path2
               path: /path3

            """);
    }
}
