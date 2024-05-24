using System.Net;

namespace Altinn.Authorization.ProblemDetails.Tests;

public class ProblemDescriptorFactoryTests
{
    [Fact]
    public void Create_ReturnsCorrectAltinnProblemDetails()
    {
        // Arrange
        var factory = ProblemDescriptorFactory.New("TEST");

        // Act
        var result = factory.Create(1, HttpStatusCode.BadRequest, "Test error");

        // Assert
        result.Should().NotBeNull();
        result.ErrorCode.ToString().Should().Be("TEST-00001");
        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        result.Detail.Should().Be("Test error");
    }

    [Fact]
    public void New_WithInvalidDomainName_ThrowsArgumentException()
    {
        // Arrange & Act & Assert
        Action act = () => ProblemDescriptorFactory.New("TESTDOMAIN");
        act.Should().ThrowExactly<ArgumentException>();
    }
}
