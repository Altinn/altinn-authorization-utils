namespace Altinn.Authorization.ProblemDetails.Tests;

public static class CollectionExpressionResolutionTests
{
    // Note: this "test" class is testing method resolution
    // as such, it doesn't need any actual tests, but just
    // checking that it compiles

    public static void ProblemDescriptor_Create()
    {
        _ = StdProblemDescriptors.ValidationError.Create();
        _ = StdProblemDescriptors.ValidationError.Create([KeyValuePair.Create("foo", "bar")]);

        _ = StdProblemDescriptors.ValidationError.Create("detail");
        _ = StdProblemDescriptors.ValidationError.Create("detail", [KeyValuePair.Create("foo", "bar")]);
    }

    public static void ProblemInstance_Create()
    {
        _ = ProblemInstance.Create(StdProblemDescriptors.ValidationError);
        _ = ProblemInstance.Create(StdProblemDescriptors.ValidationError, [KeyValuePair.Create("foo", "bar")]);

        _ = ProblemInstance.Create(StdProblemDescriptors.ValidationError, "detail");
        _ = ProblemInstance.Create(StdProblemDescriptors.ValidationError, "detail", [KeyValuePair.Create("foo", "bar")]);
    }

    public static void ValidationErrorDescriptor_Create()
    {
        _ = StdValidationErrors.Required.Create();
        _ = StdValidationErrors.Required.Create("/path");
        _ = StdValidationErrors.Required.Create("/path", [KeyValuePair.Create("foo", "bar")]);
        _ = StdValidationErrors.Required.Create([KeyValuePair.Create("foo", "bar")]);
        _ = StdValidationErrors.Required.Create(["/path"], [KeyValuePair.Create("foo", "bar")]);

        _ = StdValidationErrors.Required.Create("/path", "detail");
        _ = StdValidationErrors.Required.Create("/path", [KeyValuePair.Create("foo", "bar")], "detail");
        _ = StdValidationErrors.Required.Create([KeyValuePair.Create("foo", "bar")], "detail");
        _ = StdValidationErrors.Required.Create(["/path"], [KeyValuePair.Create("foo", "bar")], "detail");
    }

    public static void ValidationErrorInstance_Create()
    {
        _ = ValidationErrorInstance.Create(StdValidationErrors.Required);
        _ = ValidationErrorInstance.Create(StdValidationErrors.Required, "/path");
        _ = ValidationErrorInstance.Create(StdValidationErrors.Required, "/path", [KeyValuePair.Create("foo", "bar")]);
        _ = ValidationErrorInstance.Create(StdValidationErrors.Required, [KeyValuePair.Create("foo", "bar")]);
        _ = ValidationErrorInstance.Create(StdValidationErrors.Required, ["/path"], [KeyValuePair.Create("foo", "bar")]);

        _ = ValidationErrorInstance.Create(StdValidationErrors.Required);
        _ = ValidationErrorInstance.Create(StdValidationErrors.Required, "/path", "detail");
        _ = ValidationErrorInstance.Create(StdValidationErrors.Required, "/path", [KeyValuePair.Create("foo", "bar")], "detail");
        _ = ValidationErrorInstance.Create(StdValidationErrors.Required, [KeyValuePair.Create("foo", "bar")], "detail");
        _ = ValidationErrorInstance.Create(StdValidationErrors.Required, ["/path"], [KeyValuePair.Create("foo", "bar")], "detail");
    }

    public static void ValidationErrors_Add()
    {
        ValidationProblemBuilder errors = default;

        errors.Add(StdValidationErrors.Required);
        errors.Add(StdValidationErrors.Required, "/path");
        errors.Add(StdValidationErrors.Required, "/path", [KeyValuePair.Create("foo", "bar")]);
        errors.Add(StdValidationErrors.Required, [KeyValuePair.Create("foo", "bar")]);
        errors.Add(StdValidationErrors.Required, ["/path"], [KeyValuePair.Create("foo", "bar")]);

        errors.Add(StdValidationErrors.Required);
        errors.Add(StdValidationErrors.Required, "/path", "detail");
        errors.Add(StdValidationErrors.Required, "/path", [KeyValuePair.Create("foo", "bar")], "detail");
        errors.Add(StdValidationErrors.Required, [KeyValuePair.Create("foo", "bar")], "detail");
        errors.Add(StdValidationErrors.Required, ["/path"], [KeyValuePair.Create("foo", "bar")], "detail");
    }
}
