using CommunityToolkit.Diagnostics;
using System.Buffers;
using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.IO;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;

namespace Altinn.Authorization.ProblemDetails.Validation;

/// <summary>
/// A context for validation.
/// </summary>
public ref struct ValidationContext
{
    internal static ValidationContext Create(ref ValidationErrorBuilder builder, Span<string?> segmentsStore)
        => new(ref builder, ValidationPath.NewRoot(segmentsStore));

    private readonly ref ValidationErrorBuilder _builder;
    private readonly ValidationPath _path;

    private ValidationContext(ref ValidationErrorBuilder builder, ValidationPath path)
    {
        _builder = ref builder;
        _path = path;
    }

    internal ValidationContext ChildContext(string segment)
        => new(ref _builder, _path.Append(segment));

    /// <summary>
    /// Adds a validation error with the specified <paramref name="descriptor"/> and <paramref name="path"/>.
    /// </summary>
    /// <param name="descriptor">A <see cref="ValidationErrorDescriptor"/> describing the error.</param>
    /// <param name="path">The JSON path relative to the object currently being validated.</param>
    /// <inheritdoc cref="ValidationErrorInstance.Paths" path="/remarks"/>
    public void AddError(ValidationErrorDescriptor descriptor, string path)
    {
        var fullPath = _path.Append(path).ToString();

        _builder.Add(descriptor, fullPath);
    }

    /// <summary>
    /// Adds a validation error with the specified <paramref name="descriptor"/> and <paramref name="paths"/>.
    /// </summary>
    /// <param name="descriptor">A <see cref="ValidationErrorDescriptor"/> describing the error.</param>
    /// <param name="paths">The JSON path relative to the object currently being validated.</param>
    /// <inheritdoc cref="ValidationErrorInstance.Paths" path="/remarks"/>
    public void AddError(ValidationErrorDescriptor descriptor, ReadOnlySpan<string> paths)
    {
        if (paths.Length == 0)
        {
            _builder.Add(descriptor);
            return;
        }

        if (paths.Length == 1)
        {
            AddError(descriptor, paths[0]);
            return;
        }

        var builder = ImmutableArray.CreateBuilder<string>(paths.Length);
        foreach (var path in paths)
        {
            builder.Add(_path.Append(path).ToString());
        }

        _builder.Add(descriptor, builder.MoveToImmutable());
    }
}

[DebuggerDisplay("{ToString()}")]
internal readonly ref struct ValidationPath
{
    public static ValidationPath NewRoot(Span<string?> storage)
        => new(storage, 0);

    private readonly Span<string?> _segments;
    private readonly int _length;

    private ValidationPath(Span<string?> segments, int length)
    {
        _segments = segments;
        _length = length;
    }

    public override string ToString()
    {
        if (_length == 0)
        {
            return "/";
        }

        int length = 1, i = 0;
        for (i = 0; i < _length; i++)
        {
            length += _segments[i]!.Length;
        }

        var builder = ArrayPool<char>.Shared.Rent(length);
        try
        {
            length = 0;

            for (i = 0; i < _length; i++)
            {
                var segment = _segments[i]!;
                segment.AsSpan().CopyTo(builder.AsSpan(length));
                length += segment.Length;
            }

            return new string(builder, 0, length);
        }
        finally
        {
            ArrayPool<char>.Shared.Return(builder);
        }
    }

    public ValidationPath Append(string segment)
    {
        if (_length == _segments.Length)
        {
            ThrowHelper.ThrowInvalidOperationException("Validation path overflow");
        }

        _segments[_length] = segment;
        return new(_segments, _length + 1);
    }
}

public interface IValidator<in TValue, in TContext>
{
    void Validate(ref ValidationContext validationContext, TValue value, TContext context);
}

public interface IValidator<in TValue>
{
    void Validate(ref ValidationContext validationContext, TValue value);
}

public interface IValidatable<in TContext>
{
    void Validate(ref ValidationContext validationContext, TContext context);
}

public interface IValidatable
{
    void Validate(ref ValidationContext validationContext);
}

public static class AltinnProblemDetailsValidationExtensions
{
    public static void ValidateWith<TValidator, TValue, TContext>(this ref ValidationErrorBuilder builder, TValidator validator, TValue value, TContext context)
        where TValidator : IValidator<TValue, TContext>
    {
        SegmentsArray segments = default;
        var validationContext = ValidationContext.Create(ref builder, segments);

        validator.Validate(ref validationContext, value, context);
    }

    public static void ValidateWith<TValidator, TValue>(this ref ValidationErrorBuilder builder, TValidator validator, TValue value)
        where TValidator : IValidator<TValue>
    {
        SegmentsArray segments = default;
        var validationContext = ValidationContext.Create(ref builder, segments);

        validator.Validate(ref validationContext, value);
    }

    public static void Validate<TValue, TContext>(this ref ValidationErrorBuilder builder, TValue value, TContext context)
        where TValue : IValidatable<TContext>
    {
        SegmentsArray segments = default;
        var validationContext = ValidationContext.Create(ref builder, segments);

        value.Validate(ref validationContext, context);
    }

    public static void Validate<TValue>(this ref ValidationErrorBuilder builder, TValue value)
        where TValue : IValidatable
    {
        SegmentsArray segments = default;
        var validationContext = ValidationContext.Create(ref builder, segments);

        value.Validate(ref validationContext);
    }

    public static void Validate(this ref ValidationErrorBuilder builder, bool condition, ValidationErrorDescriptor descriptor, string path)
    {
        if (!condition)
        {
            builder.Add(descriptor, path);
        }
    }

    public static void Validate(this ref ValidationErrorBuilder builder, bool condition, ValidationErrorDescriptor descriptor, ReadOnlySpan<string> paths)
    {
        if (!condition)
        {
            builder.Add(descriptor, paths.ToImmutableArray());
        }
    }

    public static void ValidateWith<TValidator, TValue, TContext>(
        this ref ValidationContext validationContext,
        TValidator validator,
        TValue value,
        TContext context,
        string path)
        where TValidator : IValidator<TValue, TContext>
    {
        var childContext = validationContext.ChildContext(path);
        validator.Validate(ref childContext, value, context);
    }

    public static void ValidateWith<TValidator, TValue>(this ref ValidationContext validationContext, TValidator validator, TValue value, string path)
        where TValidator : IValidator<TValue>
    {
        var childContext = validationContext.ChildContext(path);
        validator.Validate(ref childContext, value);
    }

    public static void Validate<TValue, TContext>(this ref ValidationContext validationContext, TValue value, TContext context, string path)
        where TValue : IValidatable<TContext>
    {
        var childContext = validationContext.ChildContext(path);
        value.Validate(ref childContext, context);
    }

    public static void Validate<TValue>(this ref ValidationContext validationContext, TValue value, string path)
        where TValue : IValidatable
    {
        var childContext = validationContext.ChildContext(path);
        value.Validate(ref childContext);
    }

    public static void Validate(this ref ValidationContext validationContext, bool condition, ValidationErrorDescriptor descriptor, string path)
    {
        if (!condition)
        {
            validationContext.AddError(descriptor, path);
        }
    }

    public static void Validate(this ref ValidationContext validationContext, bool condition, ValidationErrorDescriptor descriptor, ReadOnlySpan<string> paths)
    {
        if (!condition)
        {
            validationContext.AddError(descriptor, paths);
        }
    }

    [InlineArray(8)]
    private struct SegmentsArray
    {
        private string? _item;
    }
}
