using Altinn.Authorization.RepoCtl.Model.Checks;

namespace Altinn.Authorization.RepoCtl.Model;

/// <summary>
/// RepoCtl diagnostics.
/// </summary>
public static class Diagnostics
{
    /// <summary>
    /// Gets a <see cref="Diagnostic"/> indicating that a tool project should be executable.
    /// </summary>
    /// <param name="name">The name of the tool project.</param>
    /// <param name="fullPath">The full path to the tool project file.</param>
    /// <returns>A <see cref="Diagnostic"/> indicating that the tool project should be executable.</returns>
    public static Diagnostic ToolProjectShouldBeExecutable(string name, string fullPath)
        => new Diagnostic(
            severity: DiagnosticSeverity.Warning,
            text: $"Tool project '{name}' should be executable",
            origin: DiagnosticOrigin.File(fullPath),
            category: "Project",
            code: "REPOCTL001",
            projectContext: DiagnosticProjectContext.File(fullPath));

    /// <summary>
    /// Gets a <see cref="Diagnostic"/> indicating that a project should not be executable.
    /// </summary>
    /// <param name="name">The name of the project.</param>
    /// <param name="fullPath">The full path to the project file.</param>
    /// <returns>A <see cref="Diagnostic"/> indicating that the project should not be executable.</returns>
    public static Diagnostic ProjectShouldNotBeExecutable(string name, string fullPath)
        => new Diagnostic(
            severity: DiagnosticSeverity.Warning,
            text: $"Project '{name}' should not be executable",
            origin: DiagnosticOrigin.File(fullPath),
            category: "Project",
            code: "REPOCTL002",
            projectContext: DiagnosticProjectContext.File(fullPath));

    /// <summary>
    /// Gets a <see cref="Diagnostic"/> indicating that a project should not be packable.
    /// </summary>
    /// <param name="name">The name of the project.</param>
    /// <param name="fullPath">The full path to the project file.</param>
    /// <returns>A <see cref="Diagnostic"/> indicating that the project should not be packable.</returns>
    public static Diagnostic ProjectShouldNotBePackable(string name, string fullPath)
        => new Diagnostic(
            severity: DiagnosticSeverity.Warning,
            text: $"Project '{name}' should not be packable",
            origin: DiagnosticOrigin.File(fullPath),
            category: "Project",
            code: "REPOCTL003",
            projectContext: DiagnosticProjectContext.File(fullPath));

    /// <summary>
    /// Gets a <see cref="Diagnostic"/> indicating that a project should not be a tool.
    /// </summary>
    /// <param name="name">The name of the project.</param>
    /// <param name="fullPath">The full path to the project file.</param>
    /// <returns>A <see cref="Diagnostic"/> indicating that the project should not be a tool.</returns>
    public static Diagnostic ProjectShouldNotBeTool(string name, string fullPath)
        => new Diagnostic(
            severity: DiagnosticSeverity.Warning,
            text: $"Project '{name}' should not be a tool",
            origin: DiagnosticOrigin.File(fullPath),
            category: "Project",
            code: "REPOCTL004",
            projectContext: DiagnosticProjectContext.File(fullPath));

    /// <summary>
    /// Gets a <see cref="Diagnostic"/> indicating that a project should not be a test project.
    /// </summary>
    /// <param name="name">The name of the project.</param>
    /// <param name="fullPath">The full path to the project file.</param>
    /// <returns>A <see cref="Diagnostic"/> indicating that the project should not be a test project.</returns>
    public static Diagnostic ProjectShouldNotBeTestProject(string name, string fullPath)
        => new Diagnostic(
            severity: DiagnosticSeverity.Warning,
            text: $"Project '{name}' should not be a test project",
            origin: DiagnosticOrigin.File(fullPath),
            category: "Project",
            code: "REPOCTL005",
            projectContext: DiagnosticProjectContext.File(fullPath));

    /// <summary>
    /// Gets a <see cref="Diagnostic"/> indicating that a project should not be a test library.
    /// </summary>
    /// <param name="name">The name of the project.</param>
    /// <param name="fullPath">The full path to the project file.</param>
    /// <returns>A <see cref="Diagnostic"/> indicating that the project should not be a test library.</returns>
    public static Diagnostic ProjectShouldNotBeTestLibrary(string name, string fullPath)
        => new Diagnostic(
            severity: DiagnosticSeverity.Warning,
            text: $"Project '{name}' should not be a test library",
            origin: DiagnosticOrigin.File(fullPath),
            category: "Project",
            code: "REPOCTL006",
            projectContext: DiagnosticProjectContext.File(fullPath));

    /// <summary>
    /// Gets a <see cref="Diagnostic"/> indicating that a project should not be a sample project.
    /// </summary>
    /// <param name="name">The name of the project.</param>
    /// <param name="fullPath">The full path to the project file.</param>
    /// <returns>A <see cref="Diagnostic"/> indicating that the project should not be a sample project.</returns>
    public static Diagnostic ProjectShouldNotBeSampleProject(string name, string fullPath)
        => new Diagnostic(
            severity: DiagnosticSeverity.Warning,
            text: $"Project '{name}' should not be a sample project",
            origin: DiagnosticOrigin.File(fullPath),
            category: "Project",
            code: "REPOCTL007",
            projectContext: DiagnosticProjectContext.File(fullPath));

    /// <summary>
    /// Gets a <see cref="Diagnostic"/> indicating that a project should be a test project or test library.
    /// </summary>
    /// <param name="name">The name of the project.</param>
    /// <param name="fullPath">The full path to the project file.</param>
    /// <returns>A <see cref="Diagnostic"/> indicating that the project should be a test project or test library.</returns>
    public static Diagnostic ProjectShouldBeTestProjectOrLibrary(string name, string fullPath)
        => new Diagnostic(
            severity: DiagnosticSeverity.Warning,
            text: $"Project '{name}' should be a test project or test library",
            origin: DiagnosticOrigin.File(fullPath),
            category: "Project",
            code: "REPOCTL008",
            projectContext: DiagnosticProjectContext.File(fullPath));

    /// <summary>
    /// Gets a <see cref="Diagnostic"/> indicating that a project should be a sample project.
    /// </summary>
    /// <param name="name">The name of the project.</param>
    /// <param name="fullPath">The full path to the project file.</param>
    /// <returns>A <see cref="Diagnostic"/> indicating that the project should be a sample project.</returns>
    public static Diagnostic ProjectShouldBeSampleProject(string name, string fullPath)
        => new Diagnostic(
            severity: DiagnosticSeverity.Warning,
            text: $"Project '{name}' should be a sample project",
            origin: DiagnosticOrigin.File(fullPath),
            category: "Project",
            code: "REPOCTL009",
            projectContext: DiagnosticProjectContext.File(fullPath));

    /// <summary>
    /// Gets a diagnostic indicating that a packable project is not configured for deterministic builds.
    /// </summary>
    /// <param name="name">The name of the project.</param>
    /// <param name="fullPath">The full path to the project file.</param>
    /// <param name="propertyName">The property that should be true.</param>
    /// <returns>The diagnostic.</returns>
    public static Diagnostic PackableProjectShouldBeDeterministic(string name, string fullPath, string propertyName)
        => new Diagnostic(
            severity: DiagnosticSeverity.Warning,
            text: $"Packable project '{name}' should be deterministic: '{propertyName}' should be true",
            origin: DiagnosticOrigin.File(fullPath),
            category: "Project",
            code: "REPOCTL010",
            projectContext: DiagnosticProjectContext.File(fullPath));

    /// <summary>
    /// Gets a diagnostic indicating that a packable project has invalid SourceLink configuration.
    /// </summary>
    /// <param name="name">The name of the project.</param>
    /// <param name="fullPath">The full path to the project file.</param>
    /// <param name="reason">The reason the SourceLink configuration is invalid.</param>
    /// <returns>The diagnostic.</returns>
    public static Diagnostic PackableProjectInvalidSourceLink(string name, string fullPath, string reason)
        => new Diagnostic(
            severity: DiagnosticSeverity.Warning,
            text: $"Packable project '{name}' has invalid SourceLink configuration: {reason}",
            origin: DiagnosticOrigin.File(fullPath),
            category: "Project",
            code: "REPOCTL011",
            projectContext: DiagnosticProjectContext.File(fullPath));

    /// <summary>
    /// Gets a diagnostic indicating that a packable project does not use portable debug information.
    /// </summary>
    /// <param name="name">The name of the project.</param>
    /// <param name="fullPath">The full path to the project file.</param>
    /// <param name="debugType">The configured debug type.</param>
    /// <returns>The diagnostic.</returns>
    public static Diagnostic PackableProjectInvalidDebugType(string name, string fullPath, string debugType)
        => new Diagnostic(
            severity: DiagnosticSeverity.Warning,
            text: $"Packable project '{name}' has invalid DebugType '{debugType}': expected 'embedded'",
            origin: DiagnosticOrigin.File(fullPath),
            category: "Project",
            code: "REPOCTL012",
            projectContext: DiagnosticProjectContext.File(fullPath));

    /// <summary>
    /// Gets a diagnostic indicating that an MSBuild target could not be executed.
    /// </summary>
    /// <param name="name">The name of the project.</param>
    /// <param name="fullPath">The full path to the project file.</param>
    /// <param name="targetName">The target that failed to execute.</param>
    /// <returns>The diagnostic.</returns>
    public static Diagnostic ProjectTargetExecutionFailed(string name, string fullPath, string targetName)
        => new Diagnostic(
            severity: DiagnosticSeverity.Error,
            text: $"Project '{name}' failed to execute target '{targetName}'",
            origin: DiagnosticOrigin.File(fullPath),
            category: "Project",
            code: "REPOCTL013",
            projectContext: DiagnosticProjectContext.File(fullPath));

    /// <summary>
    /// Gets a diagnostic indicating that an external tool failed to execute.
    /// </summary>
    /// <param name="command">The command that was executed.</param>
    /// <param name="exitCode">The exit code returned by the external tool.</param>
    /// <returns>The diagnostic.</returns>
    public static Diagnostic ExternalToolFailed(string command, int? exitCode)
        => new Diagnostic(
            severity: DiagnosticSeverity.Error,
            text: $"External tool failed with exit code: {(exitCode.HasValue ? exitCode.Value.ToString() : "unknown")}",
            origin: DiagnosticOrigin.Command(command),
            category: "ExternalTool",
            code: "REPOCTL014",
            projectContext: null);

    /// <summary>
    /// Gets a diagnostic indicating that a generated file is missing or out of date.
    /// </summary>
    /// <param name="fullPath">The full path to the generated file.</param>
    /// <returns>The diagnostic.</returns>
    public static Diagnostic FileOutOfDate(string fullPath)
        => new Diagnostic(
            severity: DiagnosticSeverity.Error,
            text: "Generated file is missing or out of date",
            origin: DiagnosticOrigin.File(fullPath),
            category: "File",
            code: "REPOCTL015",
            projectContext: null);
}
