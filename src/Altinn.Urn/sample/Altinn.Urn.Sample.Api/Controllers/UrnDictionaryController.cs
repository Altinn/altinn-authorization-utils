using Altinn.Urn.Json;
using Altinn.Urn.Sample.Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace Altinn.Urn.Sample.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class UrnDictionaryController : ControllerBase
{
    [HttpPost("urn-body")]
    public ActionResult<Nil> UrnBody([FromBody] KeyValueUrnDictionary<PersonUrn, PersonUrn.Type> urn)
    {
        return Ok(Nil.Instance);
    }

    [HttpPost("urn-list-body")]
    public ActionResult<Nil> UrnListBody([FromBody] List<KeyValueUrnDictionary<PersonUrn, PersonUrn.Type>> urns)
    {
        return Ok(Nil.Instance);
    }

    [HttpPost("urn-wrapper-body")]
    public ActionResult<Nil> UrnWrapperBody([FromBody] Data<KeyValueUrnDictionary<PersonUrn, PersonUrn.Type>> urn)
    {
        return Ok(Nil.Instance);
    }

    [HttpPost("urn-wrapper-list-body")]
    public ActionResult<Nil> UrnWrapperListBody([FromBody] Data<List<KeyValueUrnDictionary<PersonUrn, PersonUrn.Type>>> urns)
    {
        return Ok(Nil.Instance);
    }

    [HttpGet("urn-response")]
    public ActionResult<KeyValueUrnDictionary<PersonUrn, PersonUrn.Type>> UrnResponse()
    {
        return default!;
    }

    [HttpGet("urn-list-response")]
    public ActionResult<List<KeyValueUrnDictionary<PersonUrn, PersonUrn.Type>>> UrnListResponse()
    {
        return default!;
    }

    [HttpGet("urn-wrapper-response")]
    public ActionResult<Data<KeyValueUrnDictionary<PersonUrn, PersonUrn.Type>>> UrnWrapperResponse()
    {
        return default!;
    }

    [HttpGet("urn-wrapper-list-response")]
    public ActionResult<Data<List<KeyValueUrnDictionary<PersonUrn, PersonUrn.Type>>>> UrnWrapperListResponse()
    {
        return default!;
    }
}
