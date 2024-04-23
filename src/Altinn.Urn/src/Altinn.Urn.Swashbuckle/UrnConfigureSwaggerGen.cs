using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Altinn.Urn.Swashbuckle;

internal class UrnConfigureSwaggerGen
    : IConfigureNamedOptions<SwaggerGenOptions>
{
    private readonly UrnSwaggerFilter _urnFilter;

    public UrnConfigureSwaggerGen(UrnSwaggerFilter urnFilter)
    {
        _urnFilter = urnFilter;
    }

    public void Configure(string? name, SwaggerGenOptions options)
    {
        options.SchemaGeneratorOptions.SchemaFilters.Add(_urnFilter);
    }

    public void Configure(SwaggerGenOptions options)
        => Configure(null, options);
}
