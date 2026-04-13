using System.Diagnostics.CodeAnalysis;

namespace Altinn.Authorization.ProblemDetails.Validation;

/// <summary>
/// A <see cref="IValidator{TIn, TOut}"/> that delegates validation to the input model itself.
/// </summary>
/// <typeparam name="TIn">The type of the input model.</typeparam>
/// <typeparam name="TOut">The type of the output model.</typeparam>
public readonly struct InputModelValidator<TIn, TOut>
    : IValidator<TIn, TOut>
#if NET9_0_OR_GREATER
    where TIn : IInputModel<TOut>?, allows ref struct
    where TOut : notnull
#else
    where TIn : IInputModel<TOut>?
    where TOut : notnull
#endif
{
    /// <inheritdoc/>
    public bool TryValidate(ref ValidationContext context, TIn input, [NotNullWhen(true)] out TOut? validated)
    {
        if (input is null)
        {
            context.AddProblem(StdValidationErrors.Required);

            validated = default;
            return false;
        }

        return input.TryValidate(ref context, out validated);
    }
}
