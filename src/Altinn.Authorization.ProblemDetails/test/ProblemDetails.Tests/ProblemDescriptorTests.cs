using System.Net;

namespace Altinn.Authorization.ProblemDetails.Tests;

public class ProblemDescriptorTests
{
    [Fact]
    public void Create_WithExtensionsDictionary_ReturnsAltinnProblemDetailsWithExtensions()
    {
        // Arrange
        var factory = ProblemDescriptorFactory.New("TEST");
        var descriptor = factory.Create(1, HttpStatusCode.BadRequest, "Test error");
        var extensions = new Dictionary<string, object?>
        {
            { "Key1", "Value1" },
            { "Key2", 123 },
        };

        // Act
        var result = descriptor.ToProblemDetails(extensions);

        // Assert
        result.Should().NotBeNull();
        result.ErrorCode.ToString().Should().Be("TEST-00001");
        result.Status.Should().Be((int)HttpStatusCode.BadRequest);
        result.Detail.Should().Be("Test error");
        result.Extensions.Should().ContainKey("Key1")
            .WhoseValue.Should().Be("Value1");
        result.Extensions.Should().ContainKey("Key2")
            .WhoseValue.Should().Be(123);
    }

    [Fact]
    public void Create_WithExtensionsCollectionExpression_ReturnsAltinnProblemDetailsWithExtensions()
    {
        // Arrange
        var factory = ProblemDescriptorFactory.New("TEST");
        var descriptor = factory.Create(1, HttpStatusCode.BadRequest, "Test error");

        // Act
        var result = descriptor.ToProblemDetails([
            new("Key1", "Value1"),
            new("Key2", 123),
        ]);

        // Assert
        result.Should().NotBeNull();
        result.ErrorCode.ToString().Should().Be("TEST-00001");
        result.Status.Should().Be((int)HttpStatusCode.BadRequest);
        result.Detail.Should().Be("Test error");
        result.Extensions.Should().ContainKey("Key1")
            .WhoseValue.Should().Be("Value1");
        result.Extensions.Should().ContainKey("Key2")
            .WhoseValue.Should().Be(123);
    }
}
