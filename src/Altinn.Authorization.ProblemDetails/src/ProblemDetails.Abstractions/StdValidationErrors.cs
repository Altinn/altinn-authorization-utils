namespace Altinn.Authorization.ProblemDetails;

/// <summary>
/// Standard validation errors.
/// </summary>
public static class StdValidationErrors
{
    private static readonly ValidationErrorDescriptorFactory _factory
        = ValidationErrorDescriptorFactory.New(StdProblemDescriptors.DOMAIN_NAME);

    /// <summary>
    /// Gets a validation error descriptor for a required field missing.
    /// </summary>
    public static ValidationErrorDescriptor Required { get; }
        = _factory.Create(0, "The field is required.");

    /// <summary>
    /// Standard problem descriptors' error codes.
    /// </summary>
    public static class ErrorCodes
    {
        /// <summary>
        /// Gets the error code for a required validation-error.
        /// </summary>
        public static ErrorCode Required
            => StdValidationErrors.Required.ErrorCode;
    }
}
