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
public class ExtensibleEnumController 
    : ControllerBase
{
    /// <summary>
    /// Gets a default enum value.
    /// </summary>
    /// <param name="value">The enum value as an integer.</param>
    /// <returns>The enum value.</returns>
    [HttpGet("default")]
    public ActionResult<IEnumerable<NonExhaustiveEnum<Enums.Default>>> Default(
        [FromQuery] int value = 0)
    {
        var enumValue = (Enums.Default)value;
        IEnumerable<NonExhaustiveEnum<Enums.Default>> result = [enumValue];

        return new(result);
    }

    /// <summary>
    /// Gets a camel-case enum value.
    /// </summary>
    /// <param name="value">The enum value as an integer.</param>
    /// <returns>The enum value.</returns>
    [HttpGet("camel-case")]
    public ActionResult<IEnumerable<NonExhaustiveEnum<Enums.CamelCase>>> CamelCase(
        [FromQuery] int value = 0)
    {
        var enumValue = (Enums.CamelCase)value;
        IEnumerable<NonExhaustiveEnum<Enums.CamelCase>> result = [enumValue];
        
        return new(result);
    }

    /// <summary>
    /// Gets a lower-case enum value.
    /// </summary>
    /// <param name="value">The enum value as an integer.</param>
    /// <returns>The enum value.</returns>
    [HttpGet("lower-case")]
    public ActionResult<IEnumerable<NonExhaustiveEnum<Enums.LowerCase>>> LowerCase(
        [FromQuery] int value = 0)
    {
        var enumValue = (Enums.LowerCase)value;
        IEnumerable<NonExhaustiveEnum<Enums.LowerCase>> result = [enumValue];
        
        return new(result);
    }

    /// <summary>
    /// Gets a kebab-case-lower enum value.
    /// </summary>
    /// <param name="value">The enum value as an integer.</param>
    /// <returns>The enum value.</returns>
    [HttpGet("kebab-case-lower")]
    public ActionResult<IEnumerable<NonExhaustiveEnum<Enums.KebabCaseLower>>> KebabCaseLower(
        [FromQuery] int value = 0)
    {
        var enumValue = (Enums.KebabCaseLower)value;
        IEnumerable<NonExhaustiveEnum<Enums.KebabCaseLower>> result = [enumValue];
        
        return new(result);
    }

    /// <summary>
    /// Gets a kebab-case-upper enum value.
    /// </summary>
    /// <param name="value">The enum value as an integer.</param>
    /// <returns>The enum value.</returns>
    [HttpGet("kebab-case-upper")]
    public ActionResult<IEnumerable<NonExhaustiveEnum<Enums.KebabCaseUpper>>> KebabCaseUpper(
        [FromQuery] int value = 0)
    {
        var enumValue = (Enums.KebabCaseUpper)value;
        IEnumerable<NonExhaustiveEnum<Enums.KebabCaseUpper>> result = [enumValue];

        return new(result);
    }

    /// <summary>
    /// Gets a snake-case-lower enum value.
    /// </summary>
    /// <param name="value">The enum value as an integer.</param>
    /// <returns>The enum value.</returns>
    [HttpGet("snake-case-lower")]
    public ActionResult<IEnumerable<NonExhaustiveEnum<Enums.SnakeCaseLower>>> SnakeCaseLower(
        [FromQuery] int value = 0)
    {
        var enumValue = (Enums.SnakeCaseLower)value;
        IEnumerable<NonExhaustiveEnum<Enums.SnakeCaseLower>> result = [enumValue];
        
        return new(result);
    }

    /// <summary>
    /// Gets a snake-case-upper enum value.
    /// </summary>
    /// <param name="value">The enum value as an integer.</param>
    /// <returns>The enum value.</returns>
    [HttpGet("snake-case-upper")]
    public ActionResult<IEnumerable<NonExhaustiveEnum<Enums.SnakeCaseUpper>>> SnakeCaseUpper(
        [FromQuery] int value = 0)
    {
        var enumValue = (Enums.SnakeCaseUpper)value;
        IEnumerable<NonExhaustiveEnum<Enums.SnakeCaseUpper>> result = [enumValue];
        
        return new(result);
    }
}
