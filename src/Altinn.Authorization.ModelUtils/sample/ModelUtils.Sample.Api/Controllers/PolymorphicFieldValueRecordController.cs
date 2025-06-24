using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.CodeAnalysis;

namespace Altinn.Authorization.ModelUtils.Sample.Api.Controllers;

/// <summary>
/// Gets and receives polymorphic field-value-records.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[ExcludeFromCodeCoverage]
public class PolymorphicFieldValueRecordController
    : ControllerBase
{
    /// <summary>
    /// Gets a party.
    /// </summary>
    [HttpGet("party")]
    public ActionResult<IEnumerable<Models.PolymorphicFieldValueRecords.PartyRecord>> Parties()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Posts a party.
    /// </summary>
    /// <param name="party">The party.</param>
    [HttpPost("party")]
    public ActionResult<Models.PolymorphicFieldValueRecords.PartyRecord> Party(
        [FromBody] Models.PolymorphicFieldValueRecords.PartyRecord party)
    {
        return party;
    }

    /// <summary>
    /// Gets a person.
    /// </summary>
    [HttpGet("person")]
    public ActionResult<Models.PolymorphicFieldValueRecords.PersonRecord> Person()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Posts a person.
    /// </summary>
    /// <param name="person">The person.</param>
    [HttpPost("person")]
    public ActionResult<Models.PolymorphicFieldValueRecords.PersonRecord> Person(
        [FromBody] Models.PolymorphicFieldValueRecords.PersonRecord person)
    {
        return person;
    }

    /// <summary>
    /// Drops the base.
    /// </summary>
    [HttpGet("base")]
    public ActionResult<IEnumerable<Models.PolymorphicFieldValueRecords.Base>> Base()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Gets the set of exhaustive records.
    /// </summary>
    [HttpGet("exhaustive")]
    public ActionResult<IEnumerable<Models.PolymorphicFieldValueRecords.ExhaustiveRoot>> Exhaustive()
    {
        throw new NotImplementedException();
    }
}
