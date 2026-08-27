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
    /// Gets a validation error descriptor for a catch-all validation error.
    /// </summary>
    [Obsolete("Use specific validation error descriptors where possible.")]
    public static ValidationErrorDescriptor CatchAll { get; }
        = _factory.Create(1, "A validation error occurred.");

    /// <summary>
    /// Gets a validation error descriptor for an invalid value.
    /// </summary>
    /// <remarks>
    /// Prefer more specific validation error descriptors where possible, such as <see cref="Required"/>,
    /// or custom descriptors defined in the relevant domain.
    /// </remarks>
    public static ValidationErrorDescriptor InvalidValue { get; }
        = _factory.Create(2, "The value provided is invalid.");

    /// <summary>
    /// Gets a validation error descriptor for a value that does not satisfy string length constraints.
    /// </summary>
    public static ValidationErrorDescriptor StringLength { get; }
        = _factory.Create(3, "The value length is invalid.");

    /// <summary>
    /// Gets a validation error descriptor for a value that is too short.
    /// </summary>
    public static ValidationErrorDescriptor MinLength { get; }
        = _factory.Create(4, "The value is too short.");

    /// <summary>
    /// Gets a validation error descriptor for a value that is too long.
    /// </summary>
    public static ValidationErrorDescriptor MaxLength { get; }
        = _factory.Create(5, "The value is too long.");

    /// <summary>
    /// Gets a validation error descriptor for a value length that is outside the allowed range.
    /// </summary>
    public static ValidationErrorDescriptor Length { get; }
        = _factory.Create(6, "The value length is outside the allowed range.");

    /// <summary>
    /// Gets a validation error descriptor for a value that is outside the allowed range.
    /// </summary>
    public static ValidationErrorDescriptor Range { get; }
        = _factory.Create(7, "The value is outside the allowed range.");

    /// <summary>
    /// Gets a validation error descriptor for a value that does not match the expected pattern.
    /// </summary>
    public static ValidationErrorDescriptor RegularExpression { get; }
        = _factory.Create(8, "The value format is invalid.");

    /// <summary>
    /// Gets a validation error descriptor for a value that does not match another value.
    /// </summary>
    public static ValidationErrorDescriptor Compare { get; }
        = _factory.Create(9, "The value does not match.");

    /// <summary>
    /// Gets a validation error descriptor for an invalid email address.
    /// </summary>
    public static ValidationErrorDescriptor EmailAddress { get; }
        = _factory.Create(10, "The email address is invalid.");

    /// <summary>
    /// Gets a validation error descriptor for an invalid phone number.
    /// </summary>
    public static ValidationErrorDescriptor Phone { get; }
        = _factory.Create(11, "The phone number is invalid.");

    /// <summary>
    /// Gets a validation error descriptor for an invalid URL.
    /// </summary>
    public static ValidationErrorDescriptor Url { get; }
        = _factory.Create(12, "The URL is invalid.");

    /// <summary>
    /// Gets a validation error descriptor for an invalid credit card number.
    /// </summary>
    public static ValidationErrorDescriptor CreditCard { get; }
        = _factory.Create(13, "The credit card number is invalid.");

    /// <summary>
    /// Gets a validation error descriptor for a value that is not allowed.
    /// </summary>
    public static ValidationErrorDescriptor AllowedValues { get; }
        = _factory.Create(14, "The value is not allowed.");

    /// <summary>
    /// Gets a validation error descriptor for a denied value.
    /// </summary>
    public static ValidationErrorDescriptor DeniedValues { get; }
        = _factory.Create(15, "The value is denied.");

    /// <summary>
    /// Gets a validation error descriptor for an invalid Base64-encoded value.
    /// </summary>
    public static ValidationErrorDescriptor Base64String { get; }
        = _factory.Create(16, "The Base64 value is invalid.");

    /// <summary>
    /// Gets a validation error descriptor for an invalid JSON value.
    /// </summary>
    public static ValidationErrorDescriptor JsonError { get; }
        = _factory.Create(17, "The JSON value failed to deserialize. Check the JSON format and ensure that the property names match the expected model.");
}
