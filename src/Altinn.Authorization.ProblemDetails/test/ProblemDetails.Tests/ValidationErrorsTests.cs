namespace Altinn.Authorization.ProblemDetails.Tests;

public class ValidationErrorsTests
{
    [Fact]
    public void DefaultValidationErrors_IsEmpty()
    {
        var errors = new ValidationErrorBuilder();

        errors.Count.Should().Be(0);
        errors.IsEmpty.Should().BeTrue();
    }

    [Fact]
    public void CanAddValidationErrors()
    {
        var errors = new ValidationErrorBuilder();

        errors.Add(StdValidationErrors.Required, "/path");
        
        errors.Count.Should().Be(1);
        errors.IsEmpty.Should().BeFalse();

        errors.Add(StdValidationErrors.Required, "/path2");

        errors.Count.Should().Be(2);
        errors.IsEmpty.Should().BeFalse();
    }

    [Fact]
    public void Empty_TryTo_Returns_False()
    {
        var errors = new ValidationErrorBuilder();

        errors.TryBuild(out var instance).Should().BeFalse();
        errors.TryToProblemDetails(out var details).Should().BeFalse();
        errors.TryToActionResult(out var result).Should().BeFalse();

        instance.Should().BeNull();
        details.Should().BeNull();
        result.Should().BeNull();
    }

    [Fact]
    public void Errors_TryTo_Returns_True()
    {
        var errors = new ValidationErrorBuilder();

        errors.Add(StdValidationErrors.Required, "/path");
        errors.Add(StdValidationErrors.Required, "/path2");

        errors.TryBuild(out var instance).Should().BeTrue();
        errors.TryToProblemDetails(out var details).Should().BeTrue();
        errors.TryToActionResult(out var result).Should().BeTrue();

        instance.Should().NotBeNull();
        details.Should().NotBeNull();
        result.Should().NotBeNull();
    }
}
