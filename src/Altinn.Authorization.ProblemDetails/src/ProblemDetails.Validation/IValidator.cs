using System.Diagnostics.CodeAnalysis;

namespace Altinn.Authorization.ProblemDetails.Validation;

/// <summary>
/// Defines a validator that can validate an input of type <typeparamref name="TIn"/> and produce a validated output of type <typeparamref name="TOut"/>.
/// </summary>
/// <typeparam name="TIn">The type of the input model.</typeparam>
/// <typeparam name="TOut">The type of the validated model.</typeparam>
public interface IValidator<TIn, TOut>
    where TOut : notnull
{
    /// <summary>
    /// Tries to validate the input model and produce a validated output model.
    /// </summary>
    /// <param name="context">The validation context.</param>
    /// <param name="input">The input model to validate.</param>
    /// <param name="validated">The validated model.</param>
    /// <returns><see langword="true"/> if the input model is valid; otherwise, <see langword="false"/>.</returns>
    bool TryValidate(
        ref ValidationContext context,
        TIn input,
        [NotNullWhen(true)] out TOut? validated);
}
