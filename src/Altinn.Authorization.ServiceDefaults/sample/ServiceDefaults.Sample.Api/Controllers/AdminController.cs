using Altinn.Authorization.ServiceDefaults.Authorization.Scopes.PlatformAccessToken;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.CodeAnalysis;

namespace Altinn.Authorization.ModelUtils.Sample.Api.Controllers;

/// <summary>
/// Admin controller.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[ExcludeFromCodeCoverage]
[Authorize("policy:admin")]
[PlatformAccessTokenAuthorize]
public class AdminController 
    : ControllerBase
{
    /// <summary>
    /// Returns a newly generated UUID using either version 7 or version 4, based on the specified parameter.
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
}
