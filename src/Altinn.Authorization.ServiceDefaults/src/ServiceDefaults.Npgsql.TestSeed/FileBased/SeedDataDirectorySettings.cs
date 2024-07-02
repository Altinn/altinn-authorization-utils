using Microsoft.Extensions.FileProviders;

namespace Altinn.Authorization.ServiceDefaults.Npgsql.TestSeed.FileBased;

/// <summary>
/// Settings for <see cref="SeedDataDirectoryTestDataSeederProvider"/>.
/// </summary>
public class SeedDataDirectorySettings
{
    /// <summary>
    /// Gets or sets the file provider.
    /// </summary>
    public IFileProvider? FileProvider { get; set; }

    /// <summary>
    /// Gets or sets the directory path.
    /// </summary>
    public string? DirectoryPath { get; set; }

    /// <summary>
    /// Gets or sets the display name of the provider (used in logging).
    /// </summary>
    public string? DisplayName { get; set; }
}
