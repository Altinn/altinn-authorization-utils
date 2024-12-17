using CommunityToolkit.Diagnostics;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Altinn.Authorization.ProblemDetails;

/// <summary>
/// A (potentially failed) result of an operation.
/// </summary>
/// <typeparam name="T">The result type.</typeparam>
[DebuggerDisplay("{DebuggerDisplay,nq}")]
public readonly struct Result<T>
    : IEquatable<Result<T>>
    , IEqualityOperators<Result<T>, Result<T>, bool>
    where T : notnull
{
    /// <summary>
    /// <see langword="null"/> if <see cref="_result"/> has the result, otherwise a <see cref="ProblemInstance"/>.
    /// </summary>
    private readonly ProblemInstance? _problem;

    /// <summary>
    /// The result to be used if the operation completed successfully.
    /// </summary>
    private readonly T? _result;

    /// <summary>
    /// Gets a value indicating whether the operation failed.
    /// </summary>
    [MemberNotNullWhen(false, nameof(Value))]
    [MemberNotNullWhen(true, nameof(Problem))]
    public bool IsProblem
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _problem is not null;
    }

    /// <summary>
    /// Gets a value indicating whether the operation succeeded.
    /// </summary>
    [MemberNotNullWhen(true, nameof(Value))]
    [MemberNotNullWhen(false, nameof(Problem))]
    public bool IsSuccess
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _problem is null;
    }

    /// <summary>
    /// Ensures that the operation succeeded.
    /// </summary>
    /// <exception cref="ProblemInstanceException">Thrown if the operation failed.</exception>
    [MemberNotNull(nameof(Value))]
    public void EnsureSuccess()
    {
        if (!IsSuccess)
        {
            throw new ProblemInstanceException(_problem!);
        }
    }

    /// <summary>
    /// Gets the problem instance if the operation failed, otherwise <see langword="null"/>.
    /// </summary>
    public ProblemInstance? Problem
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _problem;
    }

    /// <summary>
    /// Gets the result if the operation succeeded, otherwise <see langword="default"/>(<typeparamref name="T"/>).
    /// </summary>
    public T? Value
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _result;
    }

    // An instance created with the default ctor (a zero init'd struct) represents a successfully completed operation
    // with a result of default(T).

    /// <summary>
    /// Initializes the <see cref="Result{T}"/> with a <typeparamref name="T"/> result value.
    /// </summary>
    /// <param name="result">The result.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Result(T result)
    {
        _result = result;
        _problem = null;
    }

    /// <summary>
    /// Initializes the <see cref="Result{T}"/> with a <see cref="ProblemInstance"/>.
    /// </summary>
    /// <param name="problemInstance">The <see cref="ProblemInstance"/>.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Result(ProblemInstance problemInstance)
    {
        Guard.IsNotNull(problemInstance);

        _result = default;
        _problem = problemInstance;
    }

    /// <summary>
    /// Implicitly converts a <typeparamref name="T"/> to a <see cref="Result{T}"/>.
    /// </summary>
    /// <param name="value">The value.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Result<T>(T value)
        => new(value);

    /// <summary>
    /// Implicitly converts a <see cref="ProblemInstance"/> to a <see cref="Result{T}"/>.
    /// </summary>
    /// <param name="value">The value.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Result<T>(ProblemInstance value)
        => new(value);

    /// <summary>
    /// Implicitly converts a <see cref="ProblemDescriptor"/> to a <see cref="Result{T}"/>.
    /// </summary>
    /// <param name="value">The value.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator Result<T>(ProblemDescriptor value)
        => new(value);

    /// <inheritdoc/>
    public override int GetHashCode()
        => _problem is not null ? _problem.GetHashCode()
        : _result is not null ? _result.GetHashCode()
        : 0;

    /// <inheritdoc/>
    public override bool Equals([NotNullWhen(true)] object? obj)
        => obj is Result<T> other && Equals(other);

    /// <inheritdoc/>
    public bool Equals(Result<T> other)
        => _problem is not null || other._problem is not null
        ? _problem == other._problem
        : EqualityComparer<T>.Default.Equals(_result, other._result);

    /// <inheritdoc/>
    public static bool operator ==(Result<T> left, Result<T> right)
        => left.Equals(right);

    /// <inheritdoc/>
    public static bool operator !=(Result<T> left, Result<T> right)
        => !left.Equals(right);

    private string DebuggerDisplay
        => _problem is not null
        ? $"Problem: {_problem.ErrorCode}"
        : string.Create(CultureInfo.InvariantCulture, $"Value: {_result}");
}
