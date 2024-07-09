using CommunityToolkit.Diagnostics;
using System.Collections.Immutable;

namespace Altinn.Authorization.ProblemDetails;

/// <summary>
/// A problem instance of a validation problem, containing 1 or more validation errors. Created using
/// a <see cref="ValidationErrorBuilder"/>.
/// </summary>
public sealed record ValidationProblemInstance
    : ProblemInstance
{
    private readonly ImmutableArray<ValidationErrorInstance> _errors;

    internal ValidationProblemInstance(
        ImmutableArray<ValidationErrorInstance> errors,
        ProblemExtensionData extensions)
        : base(StdProblemDescriptors.ValidationError, extensions)
    {
        Guard.IsNotEmpty(errors.AsSpan(), nameof(errors));

        _errors = errors;
    }

    /// <summary>
    /// Gets the validation errors.
    /// </summary>
    public ImmutableArray<ValidationErrorInstance> Errors => _errors;
}
