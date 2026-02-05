using System.Collections.Immutable;
using System.Text.Json.Serialization;

namespace Altinn.Authorization.ProblemDetails;

/// <summary>
/// Represents multiple problems.
/// </summary>
public sealed class AltinnMultipleProblemDetails
    : AltinnProblemDetails
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AltinnMultipleProblemDetails"/> class from a
    /// <see cref="MultipleProblemInstance"/>.
    /// </summary>
    /// <param name="instance">The <see cref="MultipleProblemInstance"/>.</param>
    internal AltinnMultipleProblemDetails(MultipleProblemInstance instance)
        : base(instance)
    {
        var errors = instance.Problems;
        if (!errors.IsDefaultOrEmpty)
        {
            var builder = ImmutableArray.CreateBuilder<AltinnProblemDetails>(errors.Length);
            foreach (var error in errors)
            {
                builder.Add(error.ToProblemDetails());
            }

            Problems = builder.ToImmutable();
        }
        else
        {
            Problems = [];
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AltinnMultipleProblemDetails"/> class.
    /// </summary>
    public AltinnMultipleProblemDetails()
        : base(StdProblemDescriptors.MultipleProblems)
    {
        Problems = [];
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AltinnMultipleProblemDetails"/> class.
    /// </summary>
    /// <param name="problems">The validation errors.</param>
    public AltinnMultipleProblemDetails(ReadOnlySpan<AltinnProblemDetails> problems)
        : this()
    {
        var list = new List<AltinnProblemDetails>(problems.Length);

        foreach (var problem in problems)
        {
            list.Add(problem);
        }

        Problems = list;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AltinnValidationProblemDetails"/> class.
    /// </summary>
    /// <param name="problems">The validation errors.</param>
    public AltinnMultipleProblemDetails(IEnumerable<AltinnProblemDetails> problems)
        : this()
    {
        Problems = problems.ToList();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AltinnValidationProblemDetails"/> class.
    /// </summary>
    /// <param name="problems">The validation errors.</param>
    public AltinnMultipleProblemDetails(IList<AltinnProblemDetails> problems)
        : this()
    {
        Problems = problems;
    }

    /// <summary>
    /// Gets or sets the validation errors.
    /// </summary>
    [JsonPropertyName("problems")]
    [JsonPropertyOrder(3)]
    public ICollection<AltinnProblemDetails> Problems { get; set; }
}
