namespace Altinn.Authorization.RepoCtl.Model.Checks;

/// <summary>
/// Represents a diagnostic message.
/// </summary>
public sealed record Diagnostic
{
    /// <summary>
    /// Gets the severity level of the diagnostic.
    /// </summary>
    public DiagnosticSeverity Severity { get; }

    /// <summary>
    /// Gets the diagnostic message text.
    /// </summary>
    public string Text { get; }

    /// <summary>
    /// Gets the origin of the diagnostic.
    /// </summary>
    public DiagnosticOrigin? Origin { get; }

    /// <summary>
    /// Gets the category of the diagnostic.
    /// </summary>
    /// <remarks>
    /// MSBuild calls this subcategory, and calls the severity level "category".
    /// </remarks>
    public string? Category { get; }

    /// <summary>
    /// Gets the code of the diagnostic.
    /// </summary>
    public string? Code { get; }

    /// <summary>
    /// Gets the project context associated with the diagnostic.
    /// </summary>
    public DiagnosticProjectContext? ProjectContext { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Diagnostic"/> class.
    /// </summary>
    /// <param name="severity">The severity level of the diagnostic.</param>
    /// <param name="text">The diagnostic message text.</param>
    /// <param name="origin">The origin of the diagnostic.</param>
    /// <param name="category">The category of the diagnostic.</param>
    /// <param name="code">The code of the diagnostic.</param>
    /// <param name="projectContext">The project context associated with the diagnostic.</param>
    public Diagnostic(
        DiagnosticSeverity severity,
        string text,
        DiagnosticOrigin? origin,
        string? category,
        string? code,
        DiagnosticProjectContext? projectContext)
    {
        Severity = severity;
        Text = text;
        Origin = origin;
        Category = category;
        Code = code;
        ProjectContext = projectContext;
    }
}
