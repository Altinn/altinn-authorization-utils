using Altinn.Authorization.ModelUtils.AspNet;
using Altinn.Authorization.ModelUtils.Sample.Api.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.CodeAnalysis;

namespace Altinn.Authorization.ModelUtils.Sample.Api.Controllers;

/// <summary>
/// Gets and receives extensible enum values.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[ExcludeFromCodeCoverage]
public class FlagsEnumController
    : ControllerBase
{
    [HttpGet("optional-query-flags")]
    public ActionResult<FlagsEnum<Enums.PartyFieldIncludes>> OptionalQueryFlags(
        [FromQuery] FlagsEnum<Enums.PartyFieldIncludes> value)
    {
        return value;
    }
}
