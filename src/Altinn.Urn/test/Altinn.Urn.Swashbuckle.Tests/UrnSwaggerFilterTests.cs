using Altinn.Swashbuckle.Examples;
using Altinn.Urn.Json;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.OpenApi.Any;
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
        exampleOptions.Providers.Add(new UrnEncodedExampleDataProvider());

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

        schema.Type.ShouldBe("string");
        schema.Format.ShouldBe("urn");
        schema.Pattern.ShouldNotBeNullOrEmpty();
        schema.Pattern.ShouldStartWith("^urn:altinn:party:uuid:");

        schema.Example.ShouldNotBeNull();
    }

    [Fact]
    public void Apply_ForUrnVariantType_WithUrnEncodedValue_HasExample()
    {
        var schema = SchemaFor<PersonUrn.PersonName>();

        schema.Type.ShouldBe("string");
        schema.Format.ShouldBe("urn");
        schema.Pattern.ShouldNotBeNullOrEmpty();
        schema.Pattern.ShouldStartWith("^urn:altinn:person:name:");

        schema.Example.ShouldBeOfType<OpenApiString>()
            .Value.ShouldContain("%3A");
    }

    [Fact]
    public void Apply_ForUrn_CreatesVariantsWithExamples()
    {
        var schema = SchemaFor<PersonUrn>();

        schema.OneOf.ShouldNotBeNull();
        schema.OneOf.ShouldNotBeEmpty();
        schema.OneOf.Count.ShouldBe(PersonUrn.Variants.Length);

        foreach (var variant in schema.OneOf.Select(Resolve))
        {
            variant.Type.ShouldBe("string");
            variant.Format.ShouldBe("urn");
            variant.Pattern.ShouldNotBeNullOrEmpty();
            variant.Pattern.ShouldStartWith("^urn:");
        }
    }

    [Fact]
    public void Apply_ForJsonTypeValueObjectUrn_CreatesDiscriminator()
    {
        var schema = SchemaFor<UrnJsonTypeValue<PersonUrn>>();

        schema.Discriminator.ShouldNotBeNull();
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
