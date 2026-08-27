namespace Altinn.Authorization.ProblemDetails;

/// <summary>
/// Interface for providing a <see cref="ValidationErrorInstance"/>. Typically, this is implemented
/// for an exception, such that it can carry validation error information through model binding.
/// </summary>
public interface IHasValidationError
{
    /// <summary>
    /// Gets the <see cref="ValidationErrorInstance"/> for the current instance.
    /// </summary>
    public ValidationErrorInstance? ValidationError { get; }
}
