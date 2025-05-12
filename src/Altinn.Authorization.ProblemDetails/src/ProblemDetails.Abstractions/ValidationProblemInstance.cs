using CommunityToolkit.Diagnostics;
using System.Collections.Immutable;
using System.Text;

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

    internal override void AddExceptionDetails(StringBuilder builder, string indent)
    {
        base.AddExceptionDetails(builder, indent);

        if (_errors.IsDefaultOrEmpty)
        {
            return;
        }

        builder.AppendLine();
        builder.Append(indent).AppendLine("Validation errors:");
        foreach (var error in _errors)
        {
            builder.Append(indent).AppendLine($" - {error.ErrorCode}: {error.Detail}");
            foreach (var path in error.Paths)
            {
                builder.Append(indent).AppendLine($"   path: {path}");
            }

            if (!error.Extensions.IsDefaultOrEmpty)
            {
                foreach (var (key, value) in error.Extensions)
                {
                    builder.Append(indent).AppendLine($"   {key}: {value}");
                }
            }
        }
    }
}
