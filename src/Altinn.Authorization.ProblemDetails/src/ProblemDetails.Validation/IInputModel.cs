using System.Diagnostics.CodeAnalysis;

namespace Altinn.Authorization.ProblemDetails.Validation;

/// <summary>
/// Represents an input model that can expose a validated representation.
/// </summary>
/// <typeparam name="TValidated">The validated model type.</typeparam>
public interface IInputModel<TValidated>
    where TValidated : notnull
{
    /// <summary>
    /// Validates the input model and produces a validated representation.
    /// </summary>
    /// <param name="context">The validation context.</param>
    /// <param name="validated">The validated representation of the input model.</param>
    /// <returns><c>true</c> if the validation succeeded; otherwise, <c>false</c>.</returns>
    bool TryValidate(ref ValidationContext context, [NotNullWhen(true)] out TValidated? validated);
}
