using Altinn.Swashbuckle.Examples;
using Microsoft.AspNetCore.Http.Json;

namespace Altinn.Swashbuckle.Tests;

public class OpenApiExampleProviderTests
{
    [Fact]
    public void CanGetExample()
    {
        var dataOptions = TestOptions.Create(ExampleDataOptions.DefaultOptions);
        var jsonOptions = TestOptions.Create(new JsonOptions());
        var provider = new OpenApiExampleProvider(dataOptions, jsonOptions);

        var example = provider.GetExample<DateTimeOffset>();
        Assert.NotNull(example);

        example.Should().NotBeEmpty();
    }
}
