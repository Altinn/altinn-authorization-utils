using System.Collections.Immutable;
using System.Text.Json.Serialization;

namespace Altinn.Authorization.ProblemDetails;

/// <summary>
/// Represents a validation problem.
/// </summary>
public sealed class AltinnValidationProblemDetails
    : AltinnProblemDetails
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AltinnValidationProblemDetails"/> class from a
    /// <see cref="ValidationProblemInstance"/>.
    /// </summary>
    /// <param name="instance">The <see cref="ValidationProblemInstance"/>.</param>
    internal AltinnValidationProblemDetails(ValidationProblemInstance instance)
        : base(instance)
    {
        var errors = instance.Errors;
        if (!errors.IsDefaultOrEmpty)
        {
            var builder = ImmutableArray.CreateBuilder<AltinnValidationError>(errors.Length);
            foreach (var error in errors)
            {
                builder.Add(new AltinnValidationError(error));
            }

            Errors = builder.ToImmutable();
        }
        else
        {
            Errors = [];
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AltinnValidationProblemDetails"/> class.
    /// </summary>
    public AltinnValidationProblemDetails()
        : base(StdProblemDescriptors.ValidationError)
    {
        Errors = [];
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AltinnValidationProblemDetails"/> class.
    /// </summary>
    /// <param name="errors">The validation errors.</param>
    public AltinnValidationProblemDetails(ReadOnlySpan<AltinnValidationError> errors)
        : this()
    {
        var list = new List<AltinnValidationError>(errors.Length);

        foreach (var error in errors)
        {
            list.Add(error);
        }

        Errors = list;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AltinnValidationProblemDetails"/> class.
    /// </summary>
    /// <param name="errors">The validation errors.</param>
    public AltinnValidationProblemDetails(IEnumerable<AltinnValidationError> errors)
        : this()
    {
        Errors = errors.ToList();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AltinnValidationProblemDetails"/> class.
    /// </summary>
    /// <param name="errors">The validation errors.</param>
    public AltinnValidationProblemDetails(IList<AltinnValidationError> errors)
        : this()
    {
        Errors = errors;
    }

    /// <summary>
    /// Gets or sets the validation errors.
    /// </summary>
    [JsonPropertyName("validationErrors")]
    [JsonPropertyOrder(3)]
    public ICollection<AltinnValidationError> Errors { get; set; }
}
