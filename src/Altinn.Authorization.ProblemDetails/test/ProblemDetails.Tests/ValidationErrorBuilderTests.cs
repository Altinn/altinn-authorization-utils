namespace Altinn.Authorization.ProblemDetails.Tests;

public class ValidationErrorBuilderTests
    : CollectionTests<ValidationErrorInstance, ValidationErrorBuilder, CollectionBuilderEnumerator<ValidationErrorInstance>>
{
    private readonly ValidationErrorDescriptorFactory _factory = ValidationErrorDescriptorFactory.New("TEST");
    private uint _nextId = 0;

    protected override ValidationErrorBuilder Create(ReadOnlySpan<ValidationErrorInstance> items)
    {
        var builder = ValidationErrorInstance.CreateBuilder();

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

    protected override CollectionBuilderEnumerator<ValidationErrorInstance> GetEnumerator(ValidationErrorBuilder list)
    => list.GetEnumerator();

    [Fact]
    public void Empty_TryTo_Returns_False()
    {
        var errors = ValidationErrorInstance.CreateBuilder();

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
        var errors = ValidationErrorInstance.CreateBuilder();

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
