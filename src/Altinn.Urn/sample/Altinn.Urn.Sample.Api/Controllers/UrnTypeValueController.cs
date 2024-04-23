using Altinn.Urn.Json;
using Altinn.Urn.Sample.Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace Altinn.Urn.Sample.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class UrnTypeValueController : ControllerBase 
{
    [HttpPost("urn-body")]
    public ActionResult<Nil> UrnBody([FromBody] UrnJsonTypeValue<PersonUrn> urn)
    {
        return Ok(Nil.Instance);
    }

    [HttpPost("urn-list-body")]
    public ActionResult<Nil> UrnListBody([FromBody] List<UrnJsonTypeValue<PersonUrn>> urns)
    {
        return Ok(Nil.Instance);
    }

    [HttpPost("urn-wrapper-body")]
    public ActionResult<Nil> UrnWrapperBody([FromBody] Data<UrnJsonTypeValue<PersonUrn>> urn)
    {
        return Ok(Nil.Instance);
    }

    [HttpPost("urn-wrapper-list-body")]
    public ActionResult<Nil> UrnWrapperListBody([FromBody] Data<List<UrnJsonTypeValue<PersonUrn>>> urns)
    {
        return Ok(Nil.Instance);
    }

    [HttpGet("urn-response")]
    public ActionResult<UrnJsonTypeValue<PersonUrn>> UrnResponse()
    {
        return default!;
    }

    [HttpGet("urn-list-response")]
    public ActionResult<List<UrnJsonTypeValue<PersonUrn>>> UrnListResponse()
    {
        return default!;
    }

    [HttpGet("urn-wrapper-response")]
    public ActionResult<Data<UrnJsonTypeValue<PersonUrn>>> UrnWrapperResponse()
    {
        return default!;
    }

    [HttpGet("urn-wrapper-list-response")]
    public ActionResult<Data<List<UrnJsonTypeValue<PersonUrn>>>> UrnWrapperListResponse()
    {
        return default!;
    }
}
