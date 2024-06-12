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
    public void CanMapToImmutableArray()
    {
        var errors = new ValidationErrorBuilder();

        errors.Add(StdValidationErrors.Required, "/path");
        errors.Add(StdValidationErrors.Required, "/path2");

        var immutable = errors.MapToImmutable(static v => v);
        immutable.Should().HaveCount(2);
    }

    [Fact]
    public void Empty_TryTo_Returns_False()
    {
        var errors = new ValidationErrorBuilder();

        errors.TryToProblemDetails(out var details).Should().BeFalse();
        errors.TryToActionResult(out var result).Should().BeFalse();

        details.Should().BeNull();
        result.Should().BeNull();
    }
}
