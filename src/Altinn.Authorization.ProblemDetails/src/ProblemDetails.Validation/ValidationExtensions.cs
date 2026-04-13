using System.Diagnostics.CodeAnalysis;

namespace Altinn.Authorization.ProblemDetails.Validation;

/// <summary>
/// Extension methods for validation.
/// </summary>
public static class ValidationExtensions
{
    /// <param name="context">The validation context.</param>
    extension(ref ValidationContext context)
    {
        /// <summary>
        /// Tries to validate a child input model of type <typeparamref name="TIn"/> and produce a validated output model of type <typeparamref name="TOut"/>.
        /// </summary>
        /// <typeparam name="TIn">The type of the input model.</typeparam>
        /// <typeparam name="TOut">The type of the validated model.</typeparam>
        /// <param name="path">The path to the child model.</param>
        /// <param name="input">The input model to validate.</param>
        /// <param name="validated">The validated model.</param>
        /// <inheritdoc cref="ValidationErrorInstance.Paths" path="/remarks"/>
        /// <returns><see langword="true"/> if the input model is valid; otherwise, <see langword="false"/>.</returns>
        public bool TryValidateChild<TIn, TOut>(
            string path,
            TIn? input,
            [NotNullWhen(true)] out TOut? validated)
#if NET9_0_OR_GREATER
            where TIn : notnull, IInputModel<TOut>, allows ref struct
            where TOut : notnull
#else
            where TIn : notnull, IInputModel<TOut>
            where TOut : notnull
#endif
        {
            return context.TryValidateChild(
                path,
                input,
                default(InputModelValidator<TIn?, TOut>),
                out validated);
        }

        /// <summary>
        /// Tries to validate a child input model of type <typeparamref name="TIn"/> using a custom validator of type <typeparamref name="TValidator"/> and produce a validated output model of type <typeparamref name="TOut"/>.
        /// </summary>
        /// <typeparam name="TIn">The type of the input model.</typeparam>
        /// <typeparam name="TOut">The type of the validated model.</typeparam>
        /// <typeparam name="TValidator">The type of the custom validator.</typeparam>
        /// <param name="path">The path to the child model.</param>
        /// <param name="input">The input model to validate.</param>
        /// <param name="validator">The custom validator to use.</param>
        /// <param name="validated">The validated model.</param>
        /// <inheritdoc cref="ValidationErrorInstance.Paths" path="/remarks"/>
        /// <returns><see langword="true"/> if the input model is valid; otherwise, <see langword="false"/>.</returns>
        [Obsolete(message: $"Use {nameof(ValidationContext)}.{nameof(ValidationContext.TryValidateChild)} instead.")]
        public bool TryValidateChild<TIn, TOut, TValidator>(
            string path,
            TIn input,
            TValidator validator,
            [NotNullWhen(true)] out TOut? validated)
            where TValidator : IValidator<TIn, TOut>
            where TOut : notnull
            => context.TryValidateChild(path, input, validator, out validated);
    }

    /// <typeparam name="TIn">The type of the input model.</typeparam>
    /// <typeparam name="TOut">The type of the validated model.</typeparam>
    /// <param name="model">The input model to validate.</param>
    extension<TIn, TOut>(TIn model)
#if NET9_0_OR_GREATER
        where TIn : notnull, IInputModel<TOut>, allows ref struct
        where TOut : notnull
#else
        where TIn : notnull, IInputModel<TOut>
        where TOut : notnull
#endif
    {
        /// <summary>
        /// Tries to validate the input model and produce a validated output model.
        /// </summary>
        /// <param name="builder">A <see cref="ValidationProblemBuilder"/> to collect validation errors.</param>
        /// <param name="path">The path to the input model.</param>
        /// <param name="validated">The validated model.</param>
        /// <inheritdoc cref="ValidationErrorInstance.Paths" path="/remarks"/>
        /// <returns><see langword="true"/> if the input model is valid; otherwise, <see langword="false"/>.</returns>
        public bool TryValidate(ref ValidationProblemBuilder builder, string path, [NotNullWhen(true)] out TOut? validated)
        {
            return ValidationContext.TryValidate(
                ref builder,
                path,
                model,
                default(InputModelValidator<TIn, TOut>),
                out validated);
        }

        /// <summary>
        /// Tries to validate the root input model and produce a validated output model.
        /// </summary>
        /// <param name="builder">A <see cref="ValidationProblemBuilder"/> to collect validation errors.</param>
        /// <param name="validated">The validated model.</param>
        /// <returns><see langword="true"/> if the input model is valid; otherwise, <see langword="false"/>.</returns>
        public bool TryValidate(ref ValidationProblemBuilder builder, [NotNullWhen(true)] out TOut? validated)
            => model.TryValidate(ref builder, path: "/", out validated);
    }

    /// <typeparam name="TIn">The type of the input model.</typeparam>
    /// <typeparam name="TOut">The type of the validated model.</typeparam>
    /// <typeparam name="TValidator">The type of the custom validator.</typeparam>
    /// <param name="validator">The custom validator to use.</param>
    extension<TIn, TOut, TValidator>(TValidator validator)
#if NET9_0_OR_GREATER
        where TIn : allows ref struct
        where TOut : notnull
        where TValidator : IValidator<TIn, TOut>
#else
        where TOut : notnull
        where TValidator : IValidator<TIn, TOut>
#endif
    {
        /// <summary>
        /// Tries to validate the input model using the custom validator and produce a validated output model.
        /// </summary>
        /// <param name="builder">A <see cref="ValidationProblemBuilder"/> to collect validation errors.</param>
        /// <param name="path">The path to the input model.</param>
        /// <param name="input">The input model to validate.</param>
        /// <param name="validated">The validated model.</param>
        /// <inheritdoc cref="ValidationErrorInstance.Paths" path="/remarks"/>
        /// <returns><see langword="true"/> if the input model is valid; otherwise, <see langword="false"/>.</returns>
        public bool TryValidate(ref ValidationProblemBuilder builder, string path, TIn input, [NotNullWhen(true)] out TOut? validated)
        {
            return ValidationContext.TryValidate(
                ref builder,
                path,
                input,
                validator,
                out validated);
        }

        /// <summary>
        /// Tries to validate the root input model using the custom validator and produce a validated output model.
        /// </summary>
        /// <param name="builder">A <see cref="ValidationProblemBuilder"/> to collect validation errors.</param>
        /// <param name="input">The input model to validate.</param>
        /// <param name="validated">The validated model.</param>
        /// <returns><see langword="true"/> if the input model is valid; otherwise, <see langword="false"/>.</returns>
        public bool TryValidate(ref ValidationProblemBuilder builder, TIn input, [NotNullWhen(true)] out TOut? validated)
            => validator.TryValidate(ref builder, path: "/", input, out validated);
    }

    /// <param name="builder">A <see cref="ValidationProblemBuilder"/> to collect validation errors.</param>
    extension(ref ValidationProblemBuilder builder)
    {
        /// <summary>
        /// Tries to validate a input model of type <typeparamref name="TIn"/> and produce a validated output model of type <typeparamref name="TOut"/>.
        /// </summary>
        /// <typeparam name="TIn">The type of the input model.</typeparam>
        /// <typeparam name="TOut">The type of the validated model.</typeparam>
        /// <param name="path">The path to the input model.</param>
        /// <param name="input">The input model to validate.</param>
        /// <param name="validated">The validated model.</param>
        /// <inheritdoc cref="ValidationErrorInstance.Paths" path="/remarks"/>
        /// <returns><see langword="true"/> if the input model is valid; otherwise, <see langword="false"/>.</returns>
        public bool TryValidate<TIn, TOut>(
            string path,
            TIn? input,
            [NotNullWhen(true)] out TOut? validated)
#if NET9_0_OR_GREATER
            where TIn : notnull, IInputModel<TOut>, allows ref struct
            where TOut : notnull
#else
            where TIn : notnull, IInputModel<TOut>
            where TOut : notnull
#endif
        {
            return builder.TryValidate(
                path,
                input,
                default(InputModelValidator<TIn?, TOut>),
                out validated);
        }

        /// <summary>
        /// Tries to validate a input model of type <typeparamref name="TIn"/> using a custom validator of type <typeparamref name="TValidator"/> and produce a validated output model of type <typeparamref name="TOut"/>.
        /// </summary>
        /// <typeparam name="TIn">The type of the input model.</typeparam>
        /// <typeparam name="TOut">The type of the validated model.</typeparam>
        /// <typeparam name="TValidator">The type of the custom validator.</typeparam>
        /// <param name="path">The path to the input model.</param>
        /// <param name="input">The input model to validate.</param>
        /// <param name="validator">The custom validator to use.</param>
        /// <param name="validated">The validated model.</param>
        /// <inheritdoc cref="ValidationErrorInstance.Paths" path="/remarks"/>
        /// <returns><see langword="true"/> if the input model is valid; otherwise, <see langword="false"/>.</returns>
        public bool TryValidate<TIn, TOut, TValidator>(
            string path,
            TIn input,
            TValidator validator,
            [NotNullWhen(true)] out TOut? validated)
#if NET9_0_OR_GREATER
            where TIn : allows ref struct
            where TOut : notnull
            where TValidator : IValidator<TIn, TOut>
#else
            where TOut : notnull
            where TValidator : IValidator<TIn, TOut>
#endif
        {
            return ValidationContext.TryValidate(
                ref builder,
                path,
                input,
                validator,
                out validated);
        }

        /// <summary>
        /// Tries to validate a input model of type <typeparamref name="TIn"/> using a custom validator and produce a validated output model of type <typeparamref name="TOut"/>.
        /// </summary>
        /// <typeparam name="TIn">The type of the input model.</typeparam>
        /// <typeparam name="TOut">The type of the validated model.</typeparam>
        /// <param name="path">The path to the input model.</param>
        /// <param name="input">The input model to validate.</param>
        /// <param name="validator">The custom validator to use.</param>
        /// <param name="validated">The validated model.</param>
        /// <inheritdoc cref="ValidationErrorInstance.Paths" path="/remarks"/>
        /// <returns><see langword="true"/> if the input model is valid; otherwise, <see langword="false"/>.</returns>
        public bool TryValidate<TIn, TOut>(
            string path,
            TIn input,
            Validator<TIn, TOut> validator,
            [NotNullWhen(true)] out TOut? validated)
#if NET9_0_OR_GREATER
            where TIn : allows ref struct
            where TOut : notnull
#else
            where TOut : notnull
#endif
        {
            return builder.TryValidate(
                path,
                input,
                new DelegateValidator<TIn, TOut>(validator),
                out validated);
        }
    }
}
