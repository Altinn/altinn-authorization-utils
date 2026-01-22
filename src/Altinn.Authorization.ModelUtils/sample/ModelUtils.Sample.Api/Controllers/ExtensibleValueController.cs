using Altinn.Authorization.ModelUtils.Sample.Api.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.CodeAnalysis;

namespace Altinn.Authorization.ModelUtils.Sample.Api.Controllers;

/// <summary>
/// Gets and receives extensible values.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[ExcludeFromCodeCoverage]
public class ExtensibleValueController
    : ControllerBase
{
    /// <summary>
    /// Gets foo or bar values.
    /// </summary>
    [HttpGet("get-foo-or-bar")]
    public ActionResult<IEnumerable<NonExhaustive<FooOrBar>>> GetFooOrBar()
    {
        return Ok(new NonExhaustive<FooOrBar>[]
        {
            FooOrBar.Bar,
            FooOrBar.Foo,
        });
    }
}
