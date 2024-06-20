using Altinn.Urn.Sample.Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace Altinn.Urn.Sample.Api.Controllers;

[ApiController]
[Route("[controller]")]
[ExcludeFromDescription]
public class BaseUrnController : ControllerBase
{
    [HttpPost("urn-body")]
    public ActionResult<Nil> UrnBody([FromBody] PersonUrn urn)
    {
        return Ok(Nil.Instance);
    }

    [HttpPost("urn-list-body")]
    public ActionResult<Nil> UrnListBody([FromBody] List<PersonUrn> urns)
    {
        return Ok(Nil.Instance);
    }

    [HttpPost("urn-wrapper-body")]
    public ActionResult<Nil> UrnWrapperBody([FromBody] Data<PersonUrn> urn)
    {
        return Ok(Nil.Instance);
    }

    [HttpPost("urn-wrapper-list-body")]
    public ActionResult<Nil> UrnWrapperListBody([FromBody] Data<List<PersonUrn>> urns)
    {
        return Ok(Nil.Instance);
    }

    [HttpGet("urn-response")]
    public ActionResult<PersonUrn> UrnResponse()
    {
        return default!;
    }

    [HttpGet("urn-list-response")]
    public ActionResult<List<PersonUrn>> UrnListResponse()
    {
        return default!;
    }

    [HttpGet("urn-wrapper-response")]
    public ActionResult<Data<PersonUrn>> UrnWrapperResponse()
    {
        return default!;
    }

    [HttpGet("urn-wrapper-list-response")]
    public ActionResult<Data<List<PersonUrn>>> UrnWrapperListResponse()
    {
        return default!;
    }
}
