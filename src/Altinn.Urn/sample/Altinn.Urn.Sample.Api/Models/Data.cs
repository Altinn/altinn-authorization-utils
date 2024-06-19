using System.Text.Json.Serialization;

namespace Altinn.Urn.Sample.Api.Models;

public record Data<T>(
    [property: JsonPropertyName("data"), JsonRequired] T Value);
