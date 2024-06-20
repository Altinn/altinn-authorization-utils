using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Altinn.Urn.Sample.Api.Models;

[ExcludeFromCodeCoverage]
public record Data<T>(
    [property: JsonPropertyName("data"), JsonRequired] T Value);
