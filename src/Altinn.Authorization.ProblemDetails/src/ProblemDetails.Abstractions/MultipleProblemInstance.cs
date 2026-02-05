using CommunityToolkit.Diagnostics;
using System.Collections.Immutable;
using System.Text;

namespace Altinn.Authorization.ProblemDetails;

/// <summary>
/// A problem instance of multiple problems, containing 1 or more problems. Created using
/// a <see cref="MultipleProblemBuilder"/>.
/// </summary>
public sealed record MultipleProblemInstance
    : ProblemInstance
{
    /// <summary>
    /// Creates a new instance of the builder for constructing a multiple problem response.
    /// </summary>
    /// <returns>A <see cref="MultipleProblemBuilder"/> instance that can be used to configure and build a multiple problem
    /// response.</returns>
    public static MultipleProblemBuilder CreateBuilder()
        => default;

    private readonly ImmutableArray<ProblemInstance> _problems;

    internal MultipleProblemInstance(
        ImmutableArray<ProblemInstance> problems,
        string? detail,
        ProblemExtensionData extensions)
        : base(StdProblemDescriptors.MultipleProblems, detail, extensions)
    {
        Guard.IsNotEmpty(problems.AsSpan(), nameof(problems));

        _problems = problems;
    }

    /// <summary>
    /// Gets the problems.
    /// </summary>
    public ImmutableArray<ProblemInstance> Problems => _problems;

    internal override void AddExceptionDetails(StringBuilder builder, string indent)
    {
        base.AddExceptionDetails(builder, indent);

        if (_problems.IsDefaultOrEmpty)
        {
            return;
        }

        builder.AppendLine();
        builder.Append(indent).AppendLine("Problems:");
        foreach (var problem in _problems)
        {
            builder.Append(indent).Append($" - {problem.ErrorCode}: {problem.Title}");
            if (!string.IsNullOrWhiteSpace(problem.Detail))
            {
                builder.Append($" - {problem.Detail}");
            }

            builder.AppendLine();
            problem.AddExceptionDetails(builder, $"{indent}   ");
        }
    }
}
