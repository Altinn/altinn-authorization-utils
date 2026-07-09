using System.Diagnostics;
using System.Net;
using System.Text;

namespace Altinn.Authorization.ProblemDetails;

/// <summary>
/// An instance of a <see cref="ProblemDescriptor"/>, with optional extensions.
/// </summary>
[DebuggerDisplay("{ErrorCode,nq}: {Title,nq}")]
public record class ProblemInstance
{
    /// <summary>
    /// Creates a new <see cref="ProblemInstance"/> with the specified <paramref name="descriptor"/>.
    /// </summary>
    /// <param name="descriptor">The <see cref="ProblemDescriptor"/>.</param>
    /// <param name="detail">The detail message.</param>
    /// <param name="source">The source problem instance.</param>
    /// <returns>A <see cref="ProblemInstance"/>.</returns>
    public static ProblemInstance Create(ProblemDescriptor descriptor, string? detail = null, ProblemInstance? source = null)
        => new ProblemInstance(descriptor, detail, [], exception: null, source);

    /// <summary>
    /// Creates a new <see cref="ProblemInstance"/> with the specified <paramref name="descriptor"/> and <paramref name="exception"/>.
    /// </summary>
    /// <param name="descriptor">The <see cref="ProblemDescriptor"/>.</param>
    /// <param name="exception">The exception.</param>
    /// <param name="source">The source problem instance.</param>
    /// <returns>A <see cref="ProblemInstance"/>.</returns>
    public static ProblemInstance Create(ProblemDescriptor descriptor, Exception exception, ProblemInstance? source = null)
        => new ProblemInstance(descriptor, detail: null, [], exception, source);

    /// <summary>
    /// Creates a new <see cref="ProblemInstance"/> with the specified <paramref name="descriptor"/>, <paramref name="detail"/>, and <paramref name="exception"/>.
    /// </summary>
    /// <param name="descriptor">The <see cref="ProblemDescriptor"/>.</param>
    /// <param name="detail">The detail message.</param>
    /// <param name="exception">The exception.</param>
    /// <param name="source">The source problem instance.</param>
    /// <returns>A <see cref="ProblemInstance"/>.</returns>
    public static ProblemInstance Create(ProblemDescriptor descriptor, string? detail, Exception exception, ProblemInstance? source = null)
        => new ProblemInstance(descriptor, detail, [], exception, source);

    /// <summary>
    /// Creates a new <see cref="ProblemInstance"/> with the specified <paramref name="descriptor"/> and <paramref name="extensions"/>.
    /// </summary>
    /// <param name="descriptor">The <see cref="ProblemDescriptor"/>.</param>
    /// <param name="extensions">The extensions.</param>
    /// <param name="exception">The exception.</param>
    /// <param name="source">The source problem instance.</param>
    /// <returns>A <see cref="ProblemInstance"/>.</returns>
    public static ProblemInstance Create(
        ProblemDescriptor descriptor,
        ProblemExtensionData extensions,
        Exception? exception = null,
        ProblemInstance? source = null)
        => new ProblemInstance(descriptor, detail: null, extensions, exception, source);

    /// <summary>
    /// Creates a new <see cref="ProblemInstance"/> with the specified <paramref name="descriptor"/>, <paramref name="detail"/>, and <paramref name="extensions"/>.
    /// </summary>
    /// <param name="descriptor">The <see cref="ProblemDescriptor"/>.</param>
    /// <param name="detail">The detail message.</param>
    /// <param name="extensions">The extensions.</param>
    /// <param name="exception">The exception.</param>
    /// <param name="source">The source problem instance.</param>
    /// <returns>A <see cref="ProblemInstance"/>.</returns>
    public static ProblemInstance Create(
        ProblemDescriptor descriptor,
        string? detail,
        ProblemExtensionData extensions,
        Exception? exception = null,
        ProblemInstance? source = null)
        => new ProblemInstance(descriptor, detail, extensions, exception, source);

    /// <summary>
    /// Creates a new <see cref="ProblemInstance"/> with the specified <paramref name="descriptor"/> and <paramref name="extensions"/>.
    /// </summary>
    /// <param name="descriptor">The <see cref="ProblemDescriptor"/>.</param>
    /// <param name="extensions">The extensions.</param>
    /// <param name="exception">The exception.</param>
    /// <param name="source">The source problem instance.</param>
    /// <returns>A <see cref="ProblemInstance"/>.</returns>
    public static ProblemInstance Create(
        ProblemDescriptor descriptor,
        IReadOnlyDictionary<string, string> extensions,
        Exception? exception = null,
        ProblemInstance? source = null)
        => new ProblemInstance(descriptor, detail: null, [.. extensions], exception, source);

    /// <summary>
    /// Creates a new <see cref="ProblemInstance"/> with the specified <paramref name="descriptor"/>, <paramref name="detail"/>, and <paramref name="extensions"/>.
    /// </summary>
    /// <param name="descriptor">The <see cref="ProblemDescriptor"/>.</param>
    /// <param name="detail">The detail message.</param>
    /// <param name="extensions">The extensions.</param>
    /// <param name="exception">The exception.</param>
    /// <param name="source">The source problem instance.</param>
    /// <returns>A <see cref="ProblemInstance"/>.</returns>
    public static ProblemInstance Create(
        ProblemDescriptor descriptor,
        string? detail,
        IReadOnlyDictionary<string, string> extensions,
        Exception? exception = null,
        ProblemInstance? source = null)
        => new ProblemInstance(descriptor, detail, [.. extensions], exception, source);

    private readonly ProblemDescriptor _descriptor;
    private readonly string? _detail;
    private readonly ProblemExtensionData _extensions;
    private readonly string? _traceId = Activity.Current?.Id;
    private readonly EqualityIgnoredOptional<Exception> _exception;
    private readonly ProblemInstance? _source;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProblemInstance"/> class.
    /// </summary>
    /// <param name="descriptor">The problem descriptor.</param>
    /// <param name="detail">The detail message.</param>
    /// <param name="extensions">The extensions.</param>
    /// <param name="exception">The exception.</param>
    /// <param name="source">The source problem instance.</param>
    internal ProblemInstance(
        ProblemDescriptor descriptor,
        string? detail,
        ProblemExtensionData extensions,
        Exception? exception,
        ProblemInstance? source)
    {
        _descriptor = descriptor;
        _detail = detail;
        _extensions = extensions;
        _exception = exception;
        _source = source;
    }

    /// <summary>
    /// Gets the problem descriptor.
    /// </summary>
    internal ProblemDescriptor Descriptor => _descriptor;

    /// <inheritdoc cref="ProblemDescriptor.ErrorCode"/>
    public ErrorCode ErrorCode => _descriptor.ErrorCode;

    /// <inheritdoc cref="ProblemDescriptor.StatusCode"/>
    public HttpStatusCode StatusCode => _descriptor.StatusCode;

    /// <inheritdoc cref="ProblemDescriptor.Title"/>
    public string Title => _descriptor.Title;

    /// <summary>
    /// Gets the error detail.
    /// </summary>
    public string? Detail => _detail;

    /// <summary>
    /// Gets the unique identifier for the current trace, if available.
    /// </summary>
    public string? TraceId => _traceId;

    /// <summary>
    /// Gets the extensions.
    /// </summary>
    public ProblemExtensionData Extensions
    {
        get => _extensions;
        internal init => _extensions = value;
    }

    /// <summary>
    /// Gets the exception associated with this problem instance, if any.
    /// </summary>
    internal Exception? Exception => _exception.Value;

    /// <summary>
    /// Gets the source problem instance, if any.
    /// </summary>
    public ProblemInstance? Source => _source;

    internal virtual void AddExceptionDetails(StringBuilder builder, string indent)
    {
        if (!_extensions.IsDefaultOrEmpty)
        {
            foreach (var (key, value) in _extensions)
            {
                builder.Append(indent).AppendLine($"{key}: {value}");
            }
        }
    }

    /// <summary>
    /// Implicitly converts a <see cref="ProblemDescriptor"/> to a <see cref="ProblemInstance"/>.
    /// </summary>
    /// <param name="descriptor">The <see cref="ProblemDescriptor"/>.</param>
    public static implicit operator ProblemInstance(ProblemDescriptor descriptor)
        => new ProblemInstance(descriptor, detail: null, extensions: [], exception: null, source: null);
}
