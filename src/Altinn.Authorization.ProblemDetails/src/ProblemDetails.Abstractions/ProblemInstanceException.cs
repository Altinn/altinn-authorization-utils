﻿namespace Altinn.Authorization.ProblemDetails;

/// <summary>
/// An exception that represents a <see cref="Problem"/>.
/// </summary>
public class ProblemInstanceException
    : InvalidOperationException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ProblemInstanceException"/> class.
    /// </summary>
    /// <param name="problemInstance">The <see cref="Problem"/>.</param>
    public ProblemInstanceException(ProblemInstance problemInstance)
        : this(null, problemInstance)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ProblemInstanceException"/> class.
    /// </summary>
    /// <param name="message">The message.</param>
    /// <param name="problemInstance">The <see cref="Problem"/>.</param>
    public ProblemInstanceException(string? message, ProblemInstance problemInstance)
        : this(message, null, problemInstance)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ProblemInstanceException"/> class.
    /// </summary>
    /// <param name="message">The message.</param>
    /// <param name="innerException">The inner exception.</param>
    /// <param name="problemInstance">The <see cref="Problem"/>.</param>
    public ProblemInstanceException(string? message, Exception? innerException, ProblemInstance problemInstance)
        : base(message ?? problemInstance.Detail, innerException)
    {
        Problem = problemInstance;
    }

    /// <summary>
    /// Gets the <see cref="Problem"/>.
    /// </summary>
    public ProblemInstance Problem { get; }
}