using Microsoft.Extensions.FileProviders;
using System.ComponentModel.DataAnnotations;

namespace Altinn.Authorization.ServiceDefaults.Npgsql.Yuniql;

/// <summary>
/// Options for the Yuniql database migrator.
/// </summary>
public sealed class YuniqlDatabaseMigratorOptions
    : IValidatableObject
{
    /// <summary>
    /// The environment name. Used by yuniql to conditionally apply migrations.
    /// </summary>
    public string? Environment { get; set; }

    /// <summary>
    /// The path to the yuniql workspace root. This should be a directory containing the version directories,
    /// as well as the <c>_draft</c>, <c>_erase</c>, <c>_init</c>, <c>_post</c>, and <c>_pre</c> directories.
    /// </summary>
    public string? Workspace { get; set; }

    /// <summary>
    /// The file provider to use for the workspace. If not set, the physical file system will be used.
    /// </summary>
    public IFileProvider? WorkspaceFileProvider { get; set; }

    /// <summary>
    /// Gets or sets the version table options.
    /// </summary>
    public VersionTableOptions MigrationsTable { get; set; } = new();

    /// <summary>
    /// Replacement tokens to be used during migration.
    /// </summary>
    public Dictionary<string, string> Tokens { get; set; } = [];

    IEnumerable<ValidationResult> IValidatableObject.Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrEmpty(Environment))
        {
            yield return new ValidationResult("Environment name is required", [nameof(Environment)]);
        }

        if (WorkspaceFileProvider is null && string.IsNullOrEmpty(Workspace))
        {
            yield return new ValidationResult("Workspace path or file provider is required", [nameof(Workspace), nameof(WorkspaceFileProvider)]);
        }

        if (!Directory.Exists(Workspace))
        {
            yield return new ValidationResult("Workspace path does not exist or is not a directory", [nameof(Workspace)]);
        }

        if (Tokens is null)
        {
            yield return new ValidationResult("Tokens cannot be null", [nameof(Tokens)]);
        }
        else if (Tokens.Any(t => string.IsNullOrEmpty(t.Key) || string.IsNullOrEmpty(t.Value)))
        {
            yield return new ValidationResult("Token key or value cannot be null or empty", [nameof(Tokens)]);
        }
    }
}

/// <summary>
/// Options for the version table.
/// </summary>
public sealed class VersionTableOptions
{
    /// <summary>
    /// Gets or sets the name of the version table.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the schema of the version table.
    /// </summary>
    public string? Schema { get; set; }
}
