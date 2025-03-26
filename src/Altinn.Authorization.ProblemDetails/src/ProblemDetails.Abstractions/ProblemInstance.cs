using System.Diagnostics;
using System.Net;
using System.Text;

namespace Altinn.Authorization.ProblemDetails;

/// <summary>
/// An instance of a <see cref="ProblemDescriptor"/>, with optional extensions.
/// </summary>
[DebuggerDisplay("{ErrorCode,nq}: {Detail,nq}")]
public record class ProblemInstance
{
    /// <summary>
    /// Creates a new <see cref="ProblemInstance"/> with the specified <paramref name="descriptor"/>.
    /// </summary>
    /// <param name="descriptor">The <see cref="ProblemDescriptor"/>.</param>
    /// <returns>A <see cref="ProblemInstance"/>.</returns>
    public static ProblemInstance Create(ProblemDescriptor descriptor) 
        => new ProblemInstance(descriptor, []);

    /// <summary>
    /// Creates a new <see cref="ProblemInstance"/> with the specified <paramref name="descriptor"/> and <paramref name="extensions"/>.
    /// </summary>
    /// <param name="descriptor">The <see cref="ProblemDescriptor"/>.</param>
    /// <param name="extensions">The extensions.</param>
    /// <returns>A <see cref="ProblemInstance"/>.</returns>
    public static ProblemInstance Create(ProblemDescriptor descriptor, ProblemExtensionData extensions) 
        => new ProblemInstance(descriptor, extensions);

    /// <summary>
    /// Creates a new <see cref="ProblemInstance"/> with the specified <paramref name="descriptor"/> and <paramref name="extensions"/>.
    /// </summary>
    /// <param name="descriptor">The <see cref="ProblemDescriptor"/>.</param>
    /// <param name="extensions">The extensions.</param>
    /// <returns>A <see cref="ProblemInstance"/>.</returns>
    public static ProblemInstance Create(ProblemDescriptor descriptor, IReadOnlyDictionary<string, string> extensions) 
        => new ProblemInstance(descriptor, [..extensions]);

    private readonly ProblemDescriptor _descriptor;
    private readonly ProblemExtensionData _extensions;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProblemInstance"/> class.
    /// </summary>
    /// <param name="descriptor">The problem descriptor.</param>
    /// <param name="extensions">The extensions.</param>
    internal ProblemInstance(ProblemDescriptor descriptor, ProblemExtensionData extensions)
    {
        _descriptor = descriptor;
        _extensions = extensions;
    }

    /// <inheritdoc cref="ProblemDescriptor.ErrorCode"/>
    public ErrorCode ErrorCode => _descriptor.ErrorCode;

    /// <inheritdoc cref="ProblemDescriptor.StatusCode"/>
    public HttpStatusCode StatusCode => _descriptor.StatusCode;

    /// <inheritdoc cref="ProblemDescriptor.Detail"/>
    public string Detail => _descriptor.Detail;

    /// <summary>
    /// Gets the extensions.
    /// </summary>
    public ProblemExtensionData Extensions
    {
        get => _extensions;
        internal init => _extensions = value;
    }

    internal virtual void AddExceptionDetails(StringBuilder builder)
    {
        if (!_extensions.IsDefaultOrEmpty)
        {
            foreach (var (key, value) in _extensions)
            {
                builder.AppendLine($"{key}: {value}");
            }
        }
    }

    /// <summary>
    /// Implicitly converts a <see cref="ProblemDescriptor"/> to a <see cref="ProblemInstance"/>.
    /// </summary>
    /// <param name="descriptor">The <see cref="ProblemDescriptor"/>.</param>
    public static implicit operator ProblemInstance(ProblemDescriptor descriptor) 
        => new ProblemInstance(descriptor, []);
}
