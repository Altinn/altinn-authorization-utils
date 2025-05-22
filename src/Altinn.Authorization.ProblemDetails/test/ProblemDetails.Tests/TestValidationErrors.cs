namespace Altinn.Authorization.ProblemDetails.Tests;

internal static class TestValidationErrors
{
    private static readonly ValidationErrorDescriptorFactory _factory
        = ValidationErrorDescriptorFactory.New("TEST");

    public static ValidationErrorDescriptor Empty { get; }
        = _factory.Create(1, "List is empty");
}
