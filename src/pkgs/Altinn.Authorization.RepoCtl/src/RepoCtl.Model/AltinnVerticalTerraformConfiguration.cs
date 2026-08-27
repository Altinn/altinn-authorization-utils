namespace Altinn.Authorization.RepoCtl.Model;

/// <summary>
/// Represents the Terraform configuration of an Altinn vertical.
/// </summary>
public sealed record AltinnVerticalTerraformConfiguration
{
    /// <summary>
    /// Gets the path to the Terraform state file for the vertical.
    /// </summary>
    public required string StateFile { get; init; }
}
