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
    /// <returns>A <see cref="ProblemInstance"/>.</returns>
    public static ProblemInstance Create(ProblemDescriptor descriptor) 
        => new ProblemInstance(descriptor, detail: null, []);

    /// <summary>
    /// Creates a new <see cref="ProblemInstance"/> with the specified <paramref name="descriptor"/> and <paramref name="detail"/>.
    /// </summary>
    /// <param name="descriptor">The <see cref="ProblemDescriptor"/>.</param>
    /// <param name="detail">The detail message.</param>
    /// <returns>A <see cref="ProblemInstance"/>.</returns>
    public static ProblemInstance Create(ProblemDescriptor descriptor, string? detail)
        => new ProblemInstance(descriptor, detail, []);

    /// <summary>
    /// Creates a new <see cref="ProblemInstance"/> with the specified <paramref name="descriptor"/> and <paramref name="extensions"/>.
    /// </summary>
    /// <param name="descriptor">The <see cref="ProblemDescriptor"/>.</param>
    /// <param name="extensions">The extensions.</param>
    /// <returns>A <see cref="ProblemInstance"/>.</returns>
    public static ProblemInstance Create(ProblemDescriptor descriptor, ProblemExtensionData extensions) 
        => new ProblemInstance(descriptor, detail: null, extensions);

    /// <summary>
    /// Creates a new <see cref="ProblemInstance"/> with the specified <paramref name="descriptor"/>, <paramref name="detail"/>, and <paramref name="extensions"/>.
    /// </summary>
    /// <param name="descriptor">The <see cref="ProblemDescriptor"/>.</param>
    /// <param name="detail">The detail message.</param>
    /// <param name="extensions">The extensions.</param>
    /// <returns>A <see cref="ProblemInstance"/>.</returns>
    public static ProblemInstance Create(ProblemDescriptor descriptor, string? detail, ProblemExtensionData extensions)
        => new ProblemInstance(descriptor, detail, extensions);

    /// <summary>
    /// Creates a new <see cref="ProblemInstance"/> with the specified <paramref name="descriptor"/> and <paramref name="extensions"/>.
    /// </summary>
    /// <param name="descriptor">The <see cref="ProblemDescriptor"/>.</param>
    /// <param name="extensions">The extensions.</param>
    /// <returns>A <see cref="ProblemInstance"/>.</returns>
    public static ProblemInstance Create(ProblemDescriptor descriptor, IReadOnlyDictionary<string, string> extensions) 
        => new ProblemInstance(descriptor, detail: null, [..extensions]);

    /// <summary>
    /// Creates a new <see cref="ProblemInstance"/> with the specified <paramref name="descriptor"/>, <paramref name="detail"/>, and <paramref name="extensions"/>.
    /// </summary>
    /// <param name="descriptor">The <see cref="ProblemDescriptor"/>.</param>
    /// <param name="detail">The detail message.</param>
    /// <param name="extensions">The extensions.</param>
    /// <returns>A <see cref="ProblemInstance"/>.</returns>
    public static ProblemInstance Create(ProblemDescriptor descriptor, string? detail, IReadOnlyDictionary<string, string> extensions)
        => new ProblemInstance(descriptor, detail, [.. extensions]);

    private readonly ProblemDescriptor _descriptor;
    private readonly string? _detail;
    private readonly ProblemExtensionData _extensions;
    private readonly string? _traceId = Activity.Current?.Id;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProblemInstance"/> class.
    /// </summary>
    /// <param name="descriptor">The problem descriptor.</param>
    /// <param name="detail">The detail message.</param>
    /// <param name="extensions">The extensions.</param>
    internal ProblemInstance(ProblemDescriptor descriptor, string? detail, ProblemExtensionData extensions)
    {
        _descriptor = descriptor;
        _detail = detail;
        _extensions = extensions;
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
        => new ProblemInstance(descriptor, detail: null, []);
}
