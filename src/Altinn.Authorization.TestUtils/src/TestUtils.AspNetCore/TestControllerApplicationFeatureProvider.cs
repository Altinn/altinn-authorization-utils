using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;

namespace Altinn.Authorization.TestUtils.AspNetCore;

internal class TestControllerApplicationFeatureProvider
    : IApplicationFeatureProvider<ControllerFeature>
{
    public static IApplicationFeatureProvider Instance { get; } = new TestControllerApplicationFeatureProvider();

    private TestControllerApplicationFeatureProvider()
    {
    }

    public void PopulateFeature(IEnumerable<ApplicationPart> parts, ControllerFeature feature)
    {
        foreach (var part in parts.OfType<TestControllerApplicationPart>())
        {
            feature.Controllers.Add(part.Type);
        }
    }
}
