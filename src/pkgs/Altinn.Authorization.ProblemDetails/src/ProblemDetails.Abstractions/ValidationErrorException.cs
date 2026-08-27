using System.Text;
using CommunityToolkit.Diagnostics;

namespace Altinn.Authorization.ProblemDetails;

/// <summary>
/// An exception that carries a <see cref="ValidationErrorInstance"/>.
/// </summary>
/// <remarks>
/// Typically, this is not thrown directly, but rather attached as either
/// an inner exception or a wrapper exception to allow validation error
/// information to be carried through model binding and other layers of the application.
/// </remarks>
internal sealed class ValidationErrorException
    : Exception
    , IHasValidationError
{
    private readonly ValidationErrorInstance _validationError;

    private string? _message;

    public ValidationErrorInstance ValidationError
        => _validationError;

    public override sealed string Message
        => _message ??= CreateMessage(_validationError);

    public ValidationErrorException(ValidationErrorInstance validationError, Exception? innerException)
        : base(null, innerException)
    {
        _validationError = validationError;
    }

    private static string CreateMessage(ValidationErrorInstance validationError)
    {
        Guard.IsNotNull(validationError);

        var builder = new StringBuilder();
        builder.AppendLine($"Validation error: {validationError.ErrorCode} - {validationError.Title}");
        if (!string.IsNullOrWhiteSpace(validationError.Detail))
        {
            builder.AppendLine(validationError.Detail);
        }

        foreach (var path in validationError.Paths)
        {
            builder.AppendLine($"path: {path}");
        }

        foreach (var (key, value) in validationError.Extensions)
        {
            builder.AppendLine($"{key}: {value}");
        }

        return builder.ToString();
    }
}
