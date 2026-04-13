using System.Diagnostics.CodeAnalysis;

namespace Altinn.Authorization.ProblemDetails.Validation;

/// <summary>
/// A <see cref="IValidator{TIn, TOut}"/> that delegates validation to a <see cref="Validator{TIn, TOut}"/> delegate.
/// </summary>
/// <typeparam name="TIn">The type of the input model.</typeparam>
/// <typeparam name="TOut">The type of the output model.</typeparam>
public readonly struct DelegateValidator<TIn, TOut>
    : IValidator<TIn, TOut>
#if NET9_0_OR_GREATER
        where TIn : allows ref struct
#endif
        where TOut : notnull
{
    private readonly Validator<TIn, TOut> _validator;

    internal DelegateValidator(Validator<TIn, TOut> validator)
    {
        _validator = validator;
    }

    /// <inheritdoc/>
    public bool TryValidate(ref ValidationContext context, TIn input, [NotNullWhen(true)] out TOut? validated)
    {
        return _validator(ref context, input, out validated);
    }
}
