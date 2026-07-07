using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Altinn.Authorization.ProblemDetails.Mvc;

/// <summary>
/// Interface for providing additional information for a <see cref="ValidationErrorInstance"/> based on a <see cref="ModelError"/>.
/// </summary>
public interface IModelStateErrorValidationErrorProvider
{
    /// <summary>
    /// Enrich a <see cref="ValidationErrorInstance"/> based on the given <see cref="ValidationErrorContext"/>.
    /// </summary>
    /// <param name="context">The <see cref="ValidationErrorContext"/>.</param>
    void BuildValidationError(ValidationErrorContext context);
}
