﻿using CommunityToolkit.Diagnostics;
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
    private readonly ImmutableArray<ProblemInstance> _problems;

    internal MultipleProblemInstance(
        ImmutableArray<ProblemInstance> problems,
        ProblemExtensionData extensions)
        : base(StdProblemDescriptors.MultipleProblems, extensions)
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
            builder.Append(indent).AppendLine($" - {problem.ErrorCode}: {problem.Detail}");
            problem.AddExceptionDetails(builder, $"{indent}   ");
        }
    }
}
