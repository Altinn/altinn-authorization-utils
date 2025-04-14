using Microsoft.AspNetCore.Mvc;

namespace Altinn.Authorization.ModelUtils.Sample.Api.Controllers;

/// <summary>
/// Gets and receives field-value-records.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class FieldValueRecordController
    : ControllerBase
{
    /// <summary>
    /// Gets a field-value-record.
    /// </summary>
    /// <returns>The field-value-record.</returns>
    [HttpGet("outer")]
    public ActionResult<Models.FieldValueRecords.Outer> Outer()
    {
        return new Models.FieldValueRecords.Outer
        {
            Inner = new Models.FieldValueRecords.Inner 
            {
                Value = "some value",
            },
        };
    }

    /// <summary>
    /// Posts a field-value-record.
    /// </summary>
    /// <param name="outer">The value.</param>
    /// <returns><paramref name="outer"/>.</returns>
    [HttpPost("outer")]
    public ActionResult<Models.FieldValueRecords.Outer> Outer(
        [FromBody] Models.FieldValueRecords.Outer outer)
    {
        return outer;
    }

    /// <summary>
    /// Gets a field-value-record.
    /// </summary>
    /// <returns></returns>
    [HttpGet("required-optional")]
    public ActionResult<Models.FieldValueRecords.RequiredOptional> RequiredOptional()
    {
        return new Models.FieldValueRecords.RequiredOptional
        {
            RequiredNullable = "required nullable",
            RequiredNonNullable = "required non-nullable",
            OptionalNullable = "optional nullable",
            FieldValue = "field value",
        };
    }
}
