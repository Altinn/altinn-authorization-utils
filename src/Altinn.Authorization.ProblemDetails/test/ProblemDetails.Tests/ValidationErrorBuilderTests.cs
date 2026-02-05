namespace Altinn.Authorization.ProblemDetails.Tests;

public class ValidationErrorBuilderTests
    : CollectionTests<ValidationErrorInstance, ValidationProblemBuilder, CollectionBuilderEnumerator<ValidationErrorInstance>>
{
    private readonly ValidationErrorDescriptorFactory _factory = ValidationErrorDescriptorFactory.New("TEST");
    private uint _nextId = 0;

    protected override ValidationProblemBuilder Create(ReadOnlySpan<ValidationErrorInstance> items)
    {
        var builder = ValidationProblemInstance.CreateBuilder();

        foreach (ref readonly var item in items)
        {
            builder.Add(item);
        }

        return builder;
    }

    protected override ValidationErrorInstance CreateDistinctItem()
    {
        var id = _nextId++;

        return _factory.Create(id, $"Validation error {id}").Create("/path");
    }

    protected override CollectionBuilderEnumerator<ValidationErrorInstance> GetEnumerator(ValidationProblemBuilder list)
    => list.GetEnumerator();

    [Fact]
    public void Empty_TryTo_Returns_False()
    {
        var errors = ValidationProblemInstance.CreateBuilder();

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
        var errors = ValidationProblemInstance.CreateBuilder();

        errors.Add(StdValidationErrors.Required, "/path");
        errors.Add(StdValidationErrors.Required, "/path2");

        errors.TryBuild(out var instance).ShouldBeTrue();
        errors.TryToProblemDetails(out var details).ShouldBeTrue();
        errors.TryToActionResult(out var result).ShouldBeTrue();

        instance.ShouldNotBeNull();
        details.ShouldNotBeNull();
        result.ShouldNotBeNull();
    }
}
