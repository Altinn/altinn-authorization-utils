namespace Altinn.Authorization.RepoCtl.Model.Checks;

/// <summary>
/// The severity level of a diagnostic.
/// </summary>
public enum DiagnosticSeverity
{
    /// <summary>
    /// Something suspicious but allowed.
    /// </summary>
    Warning = 2,

    /// <summary>
    /// Something not allowed by the rules of the language or other authority.
    /// </summary>
    Error = 3,
}
