namespace Altinn.Authorization.RepoCtl.Model;

/// <summary>
/// Represents the infrastructure configuration of an Altinn vertical.
/// </summary>
public sealed record AltinnVerticalInfrastructureConfiguration
{
    /// <summary>
    /// Gets the Terraform configuration of the vertical.
    /// </summary>
    public required AltinnVerticalTerraformConfiguration? Terraform { get; init; }
}
