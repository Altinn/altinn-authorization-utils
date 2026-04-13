using System.Collections.Immutable;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

namespace Altinn.Authorization.ProblemDetails.Validation;

/// <summary>
/// Provides validation for lists of input models.
/// </summary>
public static class ListValidator
{
    /// <summary>
    /// Creates an <see cref="IInputModel{T}"/> for an <see cref="IEnumerable{T}"/> of input models.
    /// </summary>
    /// <typeparam name="TIn">The type of the input model.</typeparam>
    /// <typeparam name="TOut">The type of the output model.</typeparam>
    /// <returns>An <see cref="IInputModel{T}"/> for the input enumerable.</returns>
    public static InputEnumerableValidator<TIn, TOut, InputModelValidator<TIn, TOut>> ForEnumerable<TIn, TOut>()
        where TIn : IInputModel<TOut>
        where TOut : notnull
    {
        return new InputEnumerableValidator<TIn, TOut, InputModelValidator<TIn, TOut>>(default);
    }

    /// <summary>
    /// Creates an <see cref="IInputModel{T}"/> for an <see cref="IEnumerable{T}"/> of input models.
    /// </summary>
    /// <typeparam name="TIn">The type of the input model.</typeparam>
    /// <typeparam name="TOut">The type of the output model.</typeparam>
    /// <returns>An <see cref="IInputModel{T}"/> for the input enumerable.</returns>
    public static InputEnumerableValidator<TIn, TOut, DelegateValidator<TIn, TOut>> ForEnumerable<TIn, TOut>(Validator<TIn, TOut> validator)
        where TOut : notnull
    {
        return new InputEnumerableValidator<TIn, TOut, DelegateValidator<TIn, TOut>>(new DelegateValidator<TIn, TOut>(validator));
    }

    /// <summary>
    /// Creates an <see cref="IInputModel{T}"/> for an <see cref="IEnumerable{T}"/> of input models.
    /// </summary>
    /// <typeparam name="TIn">The type of the input model.</typeparam>
    /// <typeparam name="TOut">The type of the output model.</typeparam>
    /// <typeparam name="TValidator">The type of the custom validator.</typeparam>
    /// <returns>An <see cref="IInputModel{T}"/> for the input enumerable.</returns>
    public static InputEnumerableValidator<TIn, TOut, TValidator> ForEnumerable<TIn, TOut, TValidator>(TValidator validator)
        where TOut : notnull
        where TValidator : IValidator<TIn, TOut>
    {
        return new InputEnumerableValidator<TIn, TOut, TValidator>(validator);
    }

    /// <summary>
    /// An <see cref="IInputModel{T}"/> that implements validation for an <see cref="IEnumerable{T}"/> of input models of type <typeparamref name="TIn"/> and produces a validated <see cref="List{T}"/> of type <typeparamref name="TOut"/>.
    /// </summary>
    /// <typeparam name="TIn">The type of the input model.</typeparam>
    /// <typeparam name="TOut">The type of the output model.</typeparam>
    /// <typeparam name="TValidator">The type of the custom validator.</typeparam>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public readonly struct InputEnumerableValidator<TIn, TOut, TValidator>
        : IValidator<IEnumerable<TIn>, IEnumerable<TOut>>
        , IValidator<IEnumerable<TIn>, IReadOnlyList<TOut>>
        , IValidator<IEnumerable<TIn>, List<TOut>>
        , IValidator<IEnumerable<TIn>, ImmutableArray<TOut>>
        where TOut : notnull
        where TValidator : IValidator<TIn, TOut>
    {
        private readonly TValidator _validator;

        internal InputEnumerableValidator(TValidator validator)
        {
            _validator = validator;
        }

        /// <inheritdoc/>
        public bool TryValidate(ref ValidationContext context, IEnumerable<TIn> input, [NotNullWhen(true)] out List<TOut>? validated)
        {
            List<TOut> validatedList;

            if (input.TryGetNonEnumeratedCount(out var count))
            {
                validatedList = new(count);
            }
            else
            {
                validatedList = new();
            }

            var index = 0;
            foreach (TIn item in input)
            {
                if (context.TryValidateChild(path: $"/{index}", item, _validator, out TOut? validatedItem))
                {
                    validatedList.Add(validatedItem);
                }

                index++;
            }

            if (context.HasErrors)
            {
                validated = null;
                return false;
            }

            validated = validatedList;
            return true;
        }

        /// <inheritdoc/>
        public bool TryValidate(ref ValidationContext context, IEnumerable<TIn> input, [NotNullWhen(true)] out ImmutableArray<TOut> validated)
        {
            ImmutableArray<TOut>.Builder validatedList;

            if (input.TryGetNonEnumeratedCount(out var count))
            {
                validatedList = ImmutableArray.CreateBuilder<TOut>(count);
            }
            else
            {
                validatedList = ImmutableArray.CreateBuilder<TOut>();
            }

            var index = 0;
            foreach (TIn item in input)
            {
                if (context.TryValidateChild(path: $"/{index}", item, _validator, out TOut? validatedItem))
                {
                    validatedList.Add(validatedItem);
                }

                index++;
            }

            if (context.HasErrors)
            {
                validated = [];
                return false;
            }

            validated = validatedList.DrainToImmutable();
            return true;
        }

        /// <inheritdoc/>
        public bool TryValidate(ref ValidationContext context, IEnumerable<TIn> input, [NotNullWhen(true)] out IReadOnlyList<TOut>? validated)
        {
            if (TryValidate(ref context, input, out ImmutableArray<TOut> validatedList))
            {
                validated = validatedList;
                return true;
            }

            validated = null;
            return false;
        }

        /// <inheritdoc/>
        public bool TryValidate(ref ValidationContext context, IEnumerable<TIn> input, [NotNullWhen(true)] out IEnumerable<TOut>? validated)
        {
            if (TryValidate(ref context, input, out ImmutableArray<TOut> validatedList))
            {
                validated = validatedList;
                return true;
            }

            validated = null;
            return false;
        }
    }
}
