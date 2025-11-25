using Altinn.Authorization.ServiceDefaults.Authorization.Scopes;
using Altinn.Authorization.ServiceDefaults.Authorization.Scopes.PlatformAccessToken;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.CodeAnalysis;

namespace Altinn.Authorization.ModelUtils.Sample.Api.Controllers;

/// <summary>
/// Resource controller.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[ExcludeFromCodeCoverage]
[PlatformAccessTokenOrScopeAnyOfAuthorize(["read", "write", "admin"])]
public class ResourceController
    : ControllerBase
{
    /// <summary>
    /// Requires a single scope, either read, write or admin, inherited from the controller.
    /// </summary>
    /// <remarks>Version 7 UUIDs are time-ordered and may be preferable for scenarios requiring sequential
    /// identifiers, such as database keys. Version 4 GUIDs are randomly generated and suitable for general-purpose
    /// use.</remarks>
    /// <param name="v7">Specifies whether to generate a version 7 UUID. If <see langword="true"/>, a version 7 UUID is returned;
    /// otherwise, a version 4 UUID is returned.</param>
    /// <returns>An <see cref="Guid"/> value representing the newly generated identifier.</returns>
    [HttpGet("uuid")]
    public ActionResult<Guid> GetUuid(
        [FromQuery] bool v7 = true)
        => v7 ? Guid.CreateVersion7() : Guid.NewGuid();

    /// <summary>
    /// Reduces down to just write and admin scope.
    /// </summary>
    /// <param name="value">The UUID value to set. Must be a valid <see cref="Guid"/>.</param>
    /// <returns>An <see cref="ActionResult{Guid}"/> containing the UUID value that was set.</returns>
    [HttpPost("uuid")]
    [PlatformAccessTokenOrScopeAnyOfAuthorize("write", "admin")]
    public ActionResult<Guid> SetUuid(
        [FromBody] Guid value)
        => value;

    /// <summary>
    /// Further reduces down to just admin scope.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    [HttpPut("uuid")]
    [ScopeAnyOfAuthorize("admin")]
    public ActionResult<Guid> UpdateUuid(
        [FromBody] Guid value)
        => value;

    /// <summary>
    /// Requires a new "delete" scope, in addition to write or admin.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    [HttpDelete("uuid")]
    [ScopeAnyOfAuthorize("delete")]
    [ScopeAnyOfAuthorize("write", "admin")]
    [PlatformAccessTokenAuthorize]
    public ActionResult<Guid> DeleteUuid(
        [FromBody] Guid value)
        => value;
}
