using Altinn.Swashbuckle.Examples;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Altinn.Urn.Swashbuckle.Tests;

public class UrnSwaggerFilterTests
{
    //private readonly UrnSwaggerFilter _sut;
    private readonly SchemaGenerator _schemaGenerator;
    private readonly SchemaRepository _schemaRepository;

    public UrnSwaggerFilterTests()
    {
        var exampleOptions = new ExampleDataOptions(ExampleDataOptions.DefaultOptions);
        exampleOptions.ProviderResolverChain.Add(new UrnExampleDataProviderResolver());

        var jsonOptions = new JsonOptions();
        var openApiExampleProvider = new OpenApiExampleProvider(
            TestOptions.Create(exampleOptions),
            TestOptions.Create(jsonOptions));

        var sut = new UrnSwaggerFilter(openApiExampleProvider);
        var typeResolver = new JsonSerializerDataContractResolver(jsonOptions.SerializerOptions);
        _schemaGenerator = new SchemaGenerator(new SchemaGeneratorOptions()
        {
            SchemaFilters = [sut],
        }, typeResolver);
        _schemaRepository = new SchemaRepository();
        //var schemaGenerator = new SchemaGenerator(new SchemaGeneratorOptions(), schemaRepository);
    }

    [Fact]
    public void Apply_ForUrnVariantType_UpdateToUrnStringWithExample()
    {
        var schema = SchemaFor<PersonUrn.PartyUuid>();

        schema.Type.Should().Be("string");
        schema.Format.Should().Be("urn");
        schema.Pattern.Should()
            .NotBeNullOrEmpty()
            .And.StartWith("^urn:altinn:party:uuid:");

        schema.Example.Should().NotBeNull();
    }

    [Fact]
    public void Apply_ForUrn_CreatesVariantsWithExamples()
    {
        var schema = SchemaFor<PersonUrn>();

        schema.OneOf.Should().NotBeNullOrEmpty()
            .And.HaveCount(PersonUrn.Variants.Length);

        foreach (var variant in schema.OneOf.Select(Resolve))
        {
            variant.Type.Should().Be("string");
            variant.Format.Should().Be("urn");
            variant.Pattern.Should()
                .NotBeNullOrEmpty()
                .And.StartWith("^urn:");
        }
    }

    //private ISchemaFilter SchemaFilter => _sut;
    private OpenApiSchema SchemaFor(Type type)
    {
        var reference = _schemaGenerator.GenerateSchema(type, _schemaRepository);
        return Resolve(reference);
    }

    private OpenApiSchema SchemaFor<T>()
    {
        return SchemaFor(typeof(T));
    }

    private OpenApiSchema Resolve(OpenApiSchema schema)
    {
        if (schema.Reference is { } schemaRef)
        {
            return _schemaRepository.Schemas[schemaRef.Id];
        }

        return schema;
    }
}
